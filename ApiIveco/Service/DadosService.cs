using ApiIveco.Data;
using ApiIveco.DTOs;
using ApiIveco.Models;
using Google.Cloud.Firestore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace ApiIveco.Service
{
    /// <summary>
    /// Camada de serviço que orquestra as operações de negócio do sistema Iveco Green Ledger.
    /// Gerencia persistência no Firebase Firestore, integrações com APIs externas (BrasilAPI, NHTSA, Mercado Livre)
    /// e aplica regras de negócio para validação de dados, auditoria ESG e cálculo de pegada de carbono.
    /// </summary>
    public class DadosService
    {
        // ================================================================
        // DEPENDÊNCIAS E CONSTANTES
        // ================================================================

        /// <summary>Logger para rastreamento de eventos e erros.</summary>
        private readonly ILogger<DadosService> _logger;

        /// <summary>Conexão com o Firebase Firestore.</summary>
        private readonly FireBaseData _firestoreDb;

        /// <summary>Cache em memória para otimização de consultas.</summary>
        private readonly IMemoryCache _cache;

        /// <summary>Nome da coleção de fornecedores no Firestore.</summary>
        private readonly string _collectionFornecedor = "fornecedores";

        /// <summary>Nome da coleção de lotes de matéria-prima no Firestore.</summary>
        private readonly string _collectionLote = "lotes_materia_prima";

        /// <summary>Nome da coleção de veículos no Firestore.</summary>
        private readonly string _collectionVeiculo = "veiculos";

        /// <summary>Nome da coleção de componentes (peças) no Firestore.</summary>
        private readonly string _collectionComponente = "veiculo_componentes";

        // ================================================================
        // CONSTRUTOR
        // ================================================================

        /// <summary>
        /// Inicializa o serviço com as dependências injetadas.
        /// </summary>
        /// <param name="logger">Logger para rastreamento de eventos e erros.</param>
        /// <param name="firestoreDb">Conexão com o Firebase Firestore.</param>
        /// <param name="memoryCache">Cache em memória para otimizar consultas repetidas.</param>
        public DadosService(ILogger<DadosService> logger, FireBaseData firestoreDb, IMemoryCache memoryCache)
        {
            _logger = logger;
            _firestoreDb = firestoreDb;
            _cache = memoryCache;
        }

        // ================================================================
        // MÉTODOS EXTERNOS - INTEGRAÇÕES COM APIs TERCEIRAS
        // ================================================================

        /// <summary>
        /// Consulta a BrasilAPI para obter dados cadastrais de um CNPJ.
        /// </summary>
        /// <remarks>
        /// Utiliza User-Agent personalizado para evitar bloqueios.
        /// </remarks>
        /// <param name="cnpj">CNPJ (com ou sem formatação).</param>
        /// <returns>
        /// Objeto <see cref="Fornecedor"/> preenchido com os dados da Receita Federal,
        /// ou <c>null</c> se o CNPJ não for encontrado ou houver erro na consulta.
        /// </returns>
        public async Task<Fornecedor> BuscarFornecedorPorCnpjAsync(string cnpj)
        {
            try
            {
                // Remove caracteres não numéricos para padronização
                var cnpjLimpo = new string(cnpj.Where(char.IsDigit).ToArray());
                var url = $"https://brasilapi.com.br/api/cnpj/v1/{cnpjLimpo}";

                using var client = new HttpClient();
                // Headers para simular navegador e evitar bloqueios
                client.DefaultRequestHeaders.Add("User-Agent", "IvecoApp/1.0");

                var response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var data = JsonSerializer.Deserialize<BrasilApiCnpjResponse>(json, options);

                    if (data != null)
                    {
                        // Prioriza nome fantasia, fallback para razão social
                        string nomeEmpresa = !string.IsNullOrWhiteSpace(data.NomeFantasia)
                            ? data.NomeFantasia
                            : data.RazaoSocial;

                        // Monta endereço completo
                        string moradaCompleta = $"{data.Logradouro}, {data.Numero} - {data.Bairro}, {data.Municipio} - {data.Uf}";

                        return new Fornecedor
                        {
                            Id = string.Empty, // Será gerado pelo Firestore
                            Nome = nomeEmpresa,
                            Localizacao = moradaCompleta,
                            Cnpj = cnpjLimpo
                        };
                    }
                }

                // Log da falha com detalhes da resposta HTTP
                Console.WriteLine($"[FALHA BRASIL API]: HTTP {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERRO EXCEÇÃO BRASILAPI]: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Decodifica um VIN usando a API da NHTSA (National Highway Traffic Safety Administration).
        /// </summary>
        /// <remarks>
        /// Valida se o veículo pertence à marca IVECO.
        /// </remarks>
        /// <param name="vin">VIN de 17 caracteres.</param>
        /// <returns>
        /// Objeto <see cref="Veiculo"/> validado, ou <c>null</c> se não encontrado.
        /// </returns>
        /// <exception cref="Exception">
        /// Lançada quando o VIN não pertence a um veículo IVECO.
        /// </exception>
        public async Task<Veiculo> BuscarEValidarVinIvecoAsync(string vin)
        {
            try
            {
                // Normaliza o VIN (maiúsculas e sem espaços)
                var vinLimpo = vin.Trim().ToUpper();
                var url = $"https://vpic.nhtsa.dot.gov/api/vehicles/decodevin/{vinLimpo}?format=json";

                using var client = new HttpClient();
                var response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var data = JsonSerializer.Deserialize<NhtsaResponse>(json);

                    if (data != null && data.Results != null)
                    {
                        // Extrai a marca do veículo
                        var marca = data.Results.FirstOrDefault(r => r.Variable == "Make")?.Value;

                        // VALIDAÇÃO CRÍTICA: Rejeita se não for IVECO
                        if (string.IsNullOrEmpty(marca) || !marca.ToUpper().Contains("IVECO"))
                        {
                            throw new Exception($"VIN inválido para este sistema. A marca detetada foi: {marca ?? "Desconhecida"}. Apenas veículos IVECO são permitidos.");
                        }

                        // Extrai o modelo (fallback para genérico)
                        var modelo = data.Results.FirstOrDefault(r => r.Variable == "Model")?.Value;

                        return new Veiculo
                        {
                            Vin = vinLimpo,
                            Modelo = string.IsNullOrWhiteSpace(modelo) ? "Iveco Não Especificado" : modelo,
                            DataMontagem = DateTime.UtcNow // Data atual como referência de validação
                        };
                    }
                }
                return null;
            }
            catch
            {
                // Propaga a exceção para o Controller tratar e exibir mensagem ao usuário
                throw;
            }
        }

        // ================================================================
        // MÉTODOS FIREBASE: FORNECEDORES
        // ================================================================

        /// <summary>
        /// Lista todos os fornecedores cadastrados no Firestore.
        /// </summary>
        /// <returns>Lista de objetos <see cref="Fornecedor"/>.</returns>
        public async Task<List<Fornecedor>> ListarFornecedor()
        {
            CollectionReference collection = _firestoreDb.Db.Collection(_collectionFornecedor);
            QuerySnapshot snapshot = await collection.GetSnapshotAsync();
            List<Fornecedor> fornecedores = new List<Fornecedor>();

            foreach (DocumentSnapshot document in snapshot.Documents)
            {
                if (document.Exists)
                {
                    // CORRECAO: Leitura manual campo a campo para evitar conflito com o campo
                    // "Id" gravado dentro do documento (herança de [FirestoreProperty] no modelo).
                    // ConvertTo<>() falhava silenciosamente quando o campo "Id" interno
                    // nao correspondia ao tipo esperado, retornando lista vazia [].
                    var fornecedor = new Fornecedor
                    {
                        Id = document.Id,
                        Nome = document.TryGetValue("Nome", out string nome) ? nome : null,
                        Localizacao = document.TryGetValue("Localizacao", out string localizacao) ? localizacao : null,
                        Cnpj = document.TryGetValue("Cnpj", out string cnpj) ? cnpj : null,
                    };
                    fornecedores.Add(fornecedor);
                }
            }
            return fornecedores;
        }

        /// <summary>
        /// Cria um novo fornecedor no Firestore com ID incremental.
        /// </summary>
        /// <param name="fornecedor">Dados do fornecedor (Id será gerado).</param>
        /// <returns>Fornecedor criado com o ID atribuído.</returns>
        public async Task<Fornecedor> CriarFornecedor(Fornecedor fornecedor)
        {
            int novoId = await GerarProximoId("contador_fornecedor");
            fornecedor.Id = novoId.ToString();

            // CORRECAO: Grava apenas os campos de dados via dicionario explicito,
            // sem incluir o campo "Id" dentro do documento. O document.Id ja e a
            // chave primaria — gravar "Id" internamente e redundante e causava o bug.
            var dados = new Dictionary<string, object>
            {
                { "Nome",        fornecedor.Nome        ?? "" },
                { "Localizacao", fornecedor.Localizacao ?? "" },
                { "Cnpj",        fornecedor.Cnpj        ?? "" },
            };

            DocumentReference docRef = _firestoreDb.Db.Collection(_collectionFornecedor).Document(fornecedor.Id);
            await docRef.SetAsync(dados);
            return fornecedor;
        }

        /// <summary>
        /// Exclui um fornecedor do Firestore, validando se não possui lotes associados.
        /// </summary>
        /// <remarks>
        /// Regra de negócio: não permite exclusão de fornecedores com vínculo a lotes para manter rastreabilidade ESG.
        /// </remarks>
        /// <param name="id">ID do fornecedor.</param>
        /// <exception cref="ArgumentException">ID inválido.</exception>
        /// <exception cref="InvalidOperationException">Fornecedor possui lotes ativos.</exception>
        public async Task ExcluirFornecedor(string id)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentException("O ID do fornecedor não pode ser nulo ou vazio.");

            // REGRA DE NEGÓCIO: Impedir exclusão de fornecedores com lotes ativos
            var lotesAtivos = await ListarLoteMateriaPrima();
            bool possuiLotes = lotesAtivos.Any(l => l.fk_Fornecedor_Id == id);

            if (possuiLotes)
            {
                // Vai gerar um HTTP 422 (Unprocessable Entity) ou 400
                throw new InvalidOperationException("Operação Bloqueada: Este fornecedor possui lotes de matéria-prima associados. A exclusão comprometeria a rastreabilidade do Escopo 3.");
            }

            DocumentReference docRef = _firestoreDb.Db.Collection(_collectionFornecedor).Document(id);
            await docRef.DeleteAsync();
        }

        // ================================================================
        // MÉTODOS FIREBASE: LOTES DE MATÉRIA-PRIMA
        // ================================================================

        /// <summary>
        /// Lista todos os lotes de matéria-prima cadastrados no Firestore.
        /// </summary>
        /// <returns>Lista de objetos <see cref="LoteMateriaPrima"/>.</returns>
        public async Task<List<LoteMateriaPrima>> ListarLoteMateriaPrima()
        {
            CollectionReference collection = _firestoreDb.Db.Collection(_collectionLote);
            QuerySnapshot snapshot = await collection.GetSnapshotAsync();
            List<LoteMateriaPrima> lotes = new List<LoteMateriaPrima>();

            foreach (DocumentSnapshot document in snapshot.Documents)
            {
                if (document.Exists)
                {
                    // CORRECAO: Leitura manual para compatibilidade com documentos
                    // que tenham o campo "Id" gravado internamente.
                    document.TryGetValue("QuantidadeKg", out double qtd);
                    document.TryGetValue("PegadaCarbonoPorKg", out double pegada);
                    DateTime? dataProducao = null;
                    if (document.TryGetValue("DataProducao", out Timestamp ts))
                        dataProducao = ts.ToDateTime();

                    var lote = new LoteMateriaPrima
                    {
                        Id = document.Id,
                        TipoMaterial = document.TryGetValue("TipoMaterial", out string tipo) ? tipo : null,
                        fk_Fornecedor_Id = document.TryGetValue("fk_Fornecedor_Id", out string fkForn) ? fkForn : null,
                        QuantidadeKg = qtd,
                        PegadaCarbonoPorKg = pegada,
                        DataProducao = dataProducao,
                    };
                    lotes.Add(lote);
                }
            }
            return lotes;
        }

        /// <summary>
        /// Cria um novo lote de matéria-prima no Firestore com validações de negócio.
        /// </summary>
        /// <remarks>
        /// Regras: Quantidade &gt; 0, Pegada &gt;= 0, DataProducao não pode ser futura.
        /// </remarks>
        /// <param name="lote">Dados do lote.</param>
        /// <returns>Lote criado com ID atribuído.</returns>
        /// <exception cref="ArgumentException">Dados inválidos.</exception>
        public async Task<LoteMateriaPrima> CriarLoteMateriaPrima(LoteMateriaPrima lote)
        {
            // REGRAS DE NEGÓCIO: Validações físicas e temporais
            if (lote.QuantidadeKg <= 0)
                throw new ArgumentException("A quantidade de matéria-prima (Kg) deve ser maior que zero.");

            if (lote.PegadaCarbonoPorKg < 0)
                throw new ArgumentException("O fator de Pegada de Carbono não pode ser um número negativo.");

            if (lote.DataProducao.HasValue && lote.DataProducao.Value > DateTime.UtcNow)
                throw new ArgumentException("Violação Temporal: A data de produção do lote não pode estar no futuro.");

            int novoId = await GerarProximoId("contador_lote");
            lote.Id = novoId.ToString();

            // CORRECAO: Grava via dicionario para nao incluir campo "Id" interno
            var dadosLote = new Dictionary<string, object>
            {
                { "TipoMaterial",       lote.TipoMaterial       ?? "" },
                { "fk_Fornecedor_Id",   lote.fk_Fornecedor_Id   ?? "" },
                { "QuantidadeKg",       lote.QuantidadeKg },
                { "PegadaCarbonoPorKg", lote.PegadaCarbonoPorKg },
                { "DataProducao",       lote.DataProducao.HasValue
                                        ? (object)Timestamp.FromDateTime(lote.DataProducao.Value.ToUniversalTime())
                                        : null },
            };

            DocumentReference docRef = _firestoreDb.Db.Collection(_collectionLote).Document(lote.Id);
            await docRef.SetAsync(dadosLote);

            // Invalida cache de pegada média
            _cache.Remove("PegadaMediaCache");

            return lote;
        }

        /// <summary>
        /// Exclui um lote de matéria-prima do Firestore.
        /// </summary>
        /// <param name="id">ID do lote.</param>
        /// <exception cref="ArgumentException">ID inválido.</exception>
        public async Task ExcluirLoteMateriaPrima(string id)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentException("O ID não pode ser nulo ou vazio.");
            DocumentReference docRef = _firestoreDb.Db.Collection(_collectionLote).Document(id);
            await docRef.DeleteAsync();
        }

        // ================================================================
        // MÉTODOS FIREBASE: COMPONENTES (PEÇAS)
        // ================================================================

        /// <summary>
        /// Lista todos os componentes (peças) cadastrados no Firestore.
        /// </summary>
        /// <returns>Lista de objetos <see cref="VeiculoComponente"/>.</returns>
        public async Task<List<VeiculoComponente>> ListarVeiculoComponente()
        {
            CollectionReference collection = _firestoreDb.Db.Collection(_collectionComponente);
            QuerySnapshot snapshot = await collection.GetSnapshotAsync();
            List<VeiculoComponente> componentes = new List<VeiculoComponente>();

            foreach (DocumentSnapshot document in snapshot.Documents)
            {
                if (document.Exists)
                {
                    // CORRECAO: Leitura manual para compatibilidade com documentos
                    // que tenham o campo "Id" gravado internamente.
                    document.TryGetValue("PesoKg", out double peso);
                    var componente = new VeiculoComponente
                    {
                        Id = document.Id,
                        fk_Veiculo_Vin = document.TryGetValue("fk_Veiculo_Vin", out string vin) ? vin : null,
                        fk_LoteMateriaPrima_Id = document.TryGetValue("fk_LoteMateriaPrima_Id", out string loteId) ? loteId : null,
                        fk_Fornecedor_Id = document.TryGetValue("fk_Fornecedor_Id", out string fornId) ? fornId : null,
                        NomePeca = document.TryGetValue("NomePeca", out string peca) ? peca : null,
                        PesoKg = peso,
                    };
                    componentes.Add(componente);
                }
            }
            return componentes;
        }

        /// <summary>
        /// Cria um novo componente (peça) no Firestore com validações de negócio.
        /// </summary>
        /// <remarks>
        /// Regras: Peso &gt; 0; se associado a um lote, valida o balanço de massa (não pode exceder o lote).
        /// </remarks>
        /// <param name="componente">Dados do componente.</param>
        /// <returns>Componente criado com ID atribuído.</returns>
        /// <exception cref="ArgumentException">Peso inválido.</exception>
        /// <exception cref="InvalidOperationException">Balanço de massa excedido.</exception>
        public async Task<VeiculoComponente> CriarVeiculoComponente(VeiculoComponente componente)
        {
            // REGRA DE NEGÓCIO: Peso válido
            if (componente.PesoKg <= 0)
                throw new ArgumentException("O peso da peça deve ser maior que zero.");

            // REGRA DE NEGÓCIO: Balanço de Massa do Lote
            if (!string.IsNullOrEmpty(componente.fk_LoteMateriaPrima_Id))
            {
                var lotes = await ListarLoteMateriaPrima();
                var loteOrigem = lotes.FirstOrDefault(l => l.Id == componente.fk_LoteMateriaPrima_Id);

                if (loteOrigem != null)
                {
                    var componentesExistentes = await ListarVeiculoComponente();
                    double pesoJaConsumido = componentesExistentes
                        .Where(c => c.fk_LoteMateriaPrima_Id == loteOrigem.Id)
                        .Sum(c => c.PesoKg);

                    if ((pesoJaConsumido + componente.PesoKg) > loteOrigem.QuantidadeKg)
                    {
                        throw new InvalidOperationException($"Capacidade excedida: O lote {loteOrigem.Id} possui apenas {(loteOrigem.QuantidadeKg - pesoJaConsumido):F2} Kg disponíveis. Tentou associar uma peça de {componente.PesoKg} Kg.");
                    }
                }
            }

            int novoId = await GerarProximoId("contador_componente");
            componente.Id = novoId.ToString();

            // CORRECAO: Grava via dicionario para nao incluir campo "Id" interno
            var dadosComp = new Dictionary<string, object>
            {
                { "fk_Veiculo_Vin",         componente.fk_Veiculo_Vin         ?? "" },
                { "fk_LoteMateriaPrima_Id", componente.fk_LoteMateriaPrima_Id ?? "" },
                { "fk_Fornecedor_Id",       componente.fk_Fornecedor_Id       ?? "" },
                { "NomePeca",               componente.NomePeca               ?? "" },
                { "PesoKg",                 componente.PesoKg },
            };

            DocumentReference docRef = _firestoreDb.Db.Collection(_collectionComponente).Document(componente.Id);
            await docRef.SetAsync(dadosComp);

            // Invalida cache de pegada média
            _cache.Remove("PegadaMediaCache");

            return componente;
        }

        /// <summary>
        /// Exclui um componente (peça) do Firestore.
        /// </summary>
        /// <param name="id">ID do componente.</param>
        /// <exception cref="ArgumentException">ID inválido.</exception>
        public async Task ExcluirVeiculoComponente(string id)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentException("O ID não pode ser nulo ou vazio.");
            DocumentReference docRef = _firestoreDb.Db.Collection(_collectionComponente).Document(id);
            await docRef.DeleteAsync();
        }

        // ================================================================
        // MÉTODOS FIREBASE: VEÍCULOS
        // ================================================================

        /// <summary>
        /// Lista todos os veículos cadastrados no Firestore.
        /// </summary>
        /// <remarks>
        /// O VIN é utilizado como ID do documento.
        /// </remarks>
        /// <returns>Lista de objetos <see cref="Veiculo"/>.</returns>
        public async Task<List<Veiculo>> ListarVeiculo()
        {
            CollectionReference collection = _firestoreDb.Db.Collection(_collectionVeiculo);
            QuerySnapshot snapshot = await collection.GetSnapshotAsync();
            List<Veiculo> veiculos = new List<Veiculo>();

            foreach (DocumentSnapshot document in snapshot.Documents)
            {
                if (document.Exists)
                {
                    var veiculo = document.ConvertTo<Veiculo>();
                    veiculo.Vin = document.Id; // VIN é o ID do documento
                    veiculos.Add(veiculo);
                }
            }
            return veiculos;
        }

        /// <summary>
        /// Busca peças reais no Mercado Livre para um VIN específico e as retorna como lista de componentes.
        /// </summary>
        /// <remarks>
        /// Utiliza a API do Mercado Livre para obter títulos de produtos como nomes de peças.
        /// </remarks>
        /// <param name="vin">VIN do veículo.</param>
        /// <returns>Lista de componentes simulados a partir de anúncios do Mercado Livre.</returns>
        public async Task<List<VeiculoComponente>> GerarComponentesParaVeiculoAsync(string vin)
        {
            var componentes = new List<VeiculoComponente>();

            using (var httpClient = new HttpClient())
            {
                string url = "https://api.mercadolibre.com/sites/MLB/search?q=peca+caminhao+iveco&limit=5";

                try
                {
                    var response = await httpClient.GetAsync(url);

                    if (response.IsSuccessStatusCode)
                    {
                        var jsonResult = await response.Content.ReadAsStringAsync();
                        using var doc = JsonDocument.Parse(jsonResult);

                        var resultados = doc.RootElement.GetProperty("results");

                        foreach (var item in resultados.EnumerateArray())
                        {
                            var novaPeca = new VeiculoComponente
                            {
                                Id = item.GetProperty("id").GetString() + "-" + Guid.NewGuid().ToString().Substring(0, 5),
                                NomePeca = item.GetProperty("title").GetString(),
                                fk_Veiculo_Vin = vin,
                                fk_LoteMateriaPrima_Id = "LOTE-ML-" + DateTime.Now.ToString("yyyyMMdd")
                            };

                            componentes.Add(novaPeca);
                        }
                    }
                    else
                    {
                        throw new Exception($"Erro ao buscar no Mercado Livre. Status: {response.StatusCode}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Erro na integração com o Mercado Livre: " + ex.Message);
                    throw;
                }
            }

            return componentes;
        }

        /// <summary>
        /// Cria um novo veículo no Firestore (VIN como ID).
        /// </summary>
        /// <param name="veiculo">Dados do veículo.</param>
        /// <returns>Veículo criado.</returns>
        public async Task<Veiculo> CriarVeiculo(Veiculo veiculo)
        {
            if (string.IsNullOrEmpty(veiculo.Vin))
                throw new ArgumentException("O veículo deve possuir um VIN válido.");
            DocumentReference docRef = _firestoreDb.Db.Collection(_collectionVeiculo).Document(veiculo.Vin);
            await docRef.SetAsync(veiculo);
            return veiculo;
        }

        /// <summary>
        /// Obtém um veículo específico pelo VIN.
        /// </summary>
        /// <param name="vin">VIN do veículo.</param>
        /// <returns>Veículo encontrado, ou <c>null</c>.</returns>
        public async Task<Veiculo> ObterVeiculoPorVin(string vin)
        {
            var veiculos = await ListarVeiculo();
            return veiculos.FirstOrDefault(v => v.Vin.Equals(vin, StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Exclui um veículo do Firestore.
        /// </summary>
        /// <param name="vin">VIN do veículo.</param>
        public async Task ExcluirVeiculo(string vin)
        {
            if (string.IsNullOrEmpty(vin))
                throw new ArgumentException("O VIN não pode ser nulo ou vazio.");
            DocumentReference docRef = _firestoreDb.Db.Collection(_collectionVeiculo).Document(vin);
            await docRef.DeleteAsync();
        }

        /// <summary>
        /// Atualiza um veículo existente, com regra de auditoria.
        /// </summary>
        /// <remarks>
        /// Não permite alteração após montagem concluída para preservar a auditoria ESG.
        /// </remarks>
        /// <param name="vin">VIN do veículo.</param>
        /// <param name="veiculoAtualizado">Novos dados.</param>
        /// <returns>Veículo atualizado.</returns>
        /// <exception cref="ArgumentException">VIN inválido.</exception>
        /// <exception cref="InvalidOperationException">Veículo já montado (auditoria ESG).</exception>
        public async Task<Veiculo> AtualizarVeiculo(string vin, Veiculo veiculoAtualizado)
        {
            if (string.IsNullOrEmpty(vin))
                throw new ArgumentException("O VIN não pode ser nulo ou vazio.");

            DocumentReference docRef = _firestoreDb.Db.Collection(_collectionVeiculo).Document(vin);
            DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();

            if (!snapshot.Exists)
                return null;

            var veiculoExistente = snapshot.ConvertTo<Veiculo>();

            // REGRA DE NEGÓCIO: Proteção contra fraude em veículos já montados
            if (veiculoExistente.DataMontagem.HasValue)
            {
                throw new InvalidOperationException($"Auditoria Violada: O veículo {vin} já teve a sua montagem concluída a {veiculoExistente.DataMontagem.Value:dd/MM/yyyy}. Os dados não podem ser alterados para preservar a auditoria ESG.");
            }

            veiculoAtualizado.Vin = vin;
            await docRef.SetAsync(veiculoAtualizado, SetOptions.MergeAll);

            return veiculoAtualizado;
        }

        // ================================================================
        // MÉTODO AUXILIAR: GERADOR DE IDS INCREMENTAIS
        // ================================================================

        /// <summary>
        /// Gera um ID sequencial para coleções que não possuem chave natural.
        /// </summary>
        /// <remarks>
        /// Utiliza uma transação atômica no Firestore para garantir concorrência.
        /// </remarks>
        /// <param name="nomeContador">Nome do contador no Firestore.</param>
        /// <returns>Próximo ID disponível.</returns>
        private async Task<int> GerarProximoId(string nomeContador)
        {
            DocumentReference contadorId = _firestoreDb.Db.Collection("contadores").Document(nomeContador);
            return await _firestoreDb.Db.RunTransactionAsync(async transaction =>
            {
                DocumentSnapshot snapshot = await transaction.GetSnapshotAsync(contadorId);
                int idAtual = 0;
                if (snapshot.Exists)
                    snapshot.TryGetValue("ultimoId", out idAtual);

                int proximoId = idAtual + 1;
                Dictionary<string, object> atualizacaoContador = new Dictionary<string, object> { { "ultimoId", proximoId } };
                transaction.Set(contadorId, atualizacaoContador, SetOptions.MergeAll);
                return proximoId;
            });
        }

        // ================================================================
        // MÉTODOS DE AUTENTICAÇÃO (USUÁRIOS)
        // ================================================================

        /// <summary>
        /// Cadastra um novo usuário no Firestore com validação de e-mail único.
        /// </summary>
        /// <param name="novoUsuario">Dados do usuário.</param>
        /// <returns>Usuário criado.</returns>
        /// <exception cref="Exception">E-mail já cadastrado.</exception>
        public async Task<Usuario> CadastrarUsuario(Usuario novoUsuario)
        {
            var usuariosRef = _firestoreDb.Db.Collection("Usuarios");

            // Verifica se email já existe
            var query = await usuariosRef.WhereEqualTo("Email", novoUsuario.Email).GetSnapshotAsync();

            if (query.Documents.Count > 0)
                throw new Exception("Já existe um usuário cadastrado com este e-mail.");

            // Garante valor padrão
            if (string.IsNullOrWhiteSpace(novoUsuario.Acesso))
                novoUsuario.Acesso = "Usuario";

            int novoId = await GerarProximoId("contador_usuario");
            novoUsuario.Id = novoId.ToString();
            novoUsuario.DataCriacao = DateTime.UtcNow;

            DocumentReference docRef = usuariosRef.Document(novoUsuario.Id);
            await docRef.SetAsync(novoUsuario);

            return novoUsuario;
        }

        /// <summary>
        /// Autentica um usuário por e-mail e senha.
        /// </summary>
        /// <remarks>
        /// Realiza comparação manual para evitar gargalos de indexação NoSQL.
        /// </remarks>
        /// <param name="email">E-mail do usuário.</param>
        /// <param name="senha">Senha (texto puro).</param>
        /// <returns>Usuário autenticado, ou <c>null</c>.</returns>
        public async Task<Usuario> FazerLogin(string email, string senha)
        {
            _logger.LogCritical("### LOGIN PARA: {email}", email);

            var usuariosRef = _firestoreDb.Db.Collection("Usuarios");
            var snapshot = await usuariosRef.GetSnapshotAsync();

            _logger.LogCritical("### TOTAL DOCS: {count}", snapshot.Documents.Count);

            foreach (var doc in snapshot.Documents)
            {
                doc.TryGetValue<string>("Email", out var emailSalvo);
                doc.TryGetValue<string>("Senha", out var senhaSalva);

                _logger.LogCritical("### DOC {id} | Email:'{e}' | Senha:'{s}'",
                    doc.Id, emailSalvo, senhaSalva);

                if (string.Equals(emailSalvo, email, StringComparison.OrdinalIgnoreCase)
                    && senhaSalva == senha)
                {
                    _logger.LogCritical("### USUARIO ENCONTRADO");
                    var usuario = doc.ConvertTo<Usuario>();
                    usuario.Id = doc.Id;
                    return usuario;
                }
            }

            _logger.LogCritical("### NENHUM USUARIO ENCONTRADO");
            return null;
        }

        // ================================================================
        // MÉTODOS DE CÁLCULO PARA DASHBOARD E ESG
        // ================================================================

        /// <summary>
        /// Calcula a pegada de carbono média (kg CO₂) considerando lotes (prioritário) ou componentes (fallback).
        /// </summary>
        /// <remarks>
        /// Utiliza cache de 5 minutos para otimização.
        /// </remarks>
        /// <returns>Valor médio da pegada de carbono.</returns>
        public async Task<double> CalcularPegadaMediaAsync()
        {
            const string cacheKey = "PegadaMediaCache";

            if (_cache.TryGetValue(cacheKey, out double cachedValue))
                return cachedValue;

            double resultado = await CalcularPegadaMediaInternoAsync();
            _cache.Set(cacheKey, resultado, TimeSpan.FromMinutes(5));
            return resultado;
        }

        /// <summary>
        /// Cálculo real (sem cache) – chamado internamente.
        /// </summary>
        /// <remarks>
        /// Prioriza lotes; se não houver, usa componentes com fator de emissão padrão (2.5 kg CO2/kg).
        /// </remarks>
        /// <returns>Valor médio calculado, ou 0 em caso de erro ou dados ausentes.</returns>
        private async Task<double> CalcularPegadaMediaInternoAsync()
        {
            try
            {
                // 1. Tenta calcular a partir dos lotes de matéria-prima
                var lotes = await ListarLoteMateriaPrima();
                if (lotes != null && lotes.Count > 0)
                {
                    double somaPegada = 0;
                    foreach (var lote in lotes)
                    {
                        somaPegada += lote.QuantidadeKg * lote.PegadaCarbonoPorKg;
                    }
                    return somaPegada / lotes.Count;
                }

                // 2. Se não há lotes, calcula a partir dos componentes (peças)
                var componentes = await ListarVeiculoComponente();
                if (componentes == null || componentes.Count == 0)
                    return 0;

                const double FatorEmissaoPadrao = 2.5; // kg CO2/kg (média para metais)
                var grupos = componentes.GroupBy(c => c.fk_Veiculo_Vin);
                double somaPegadaPorVeiculo = 0;
                int totalVeiculosComPecas = 0;

                foreach (var grupo in grupos)
                {
                    double pegadaVeiculo = 0;
                    foreach (var comp in grupo)
                    {
                        pegadaVeiculo += comp.PesoKg * FatorEmissaoPadrao;
                    }
                    somaPegadaPorVeiculo += pegadaVeiculo;
                    totalVeiculosComPecas++;
                }

                return totalVeiculosComPecas > 0 ? somaPegadaPorVeiculo / totalVeiculosComPecas : 0;
            }
            catch
            {
                // Em caso de erro, retorna 0 (nunca lança exceção)
                return 0;
            }
        }

        /// <summary>
        /// Obtém dados mensais de emissões para o gráfico YTD.
        /// </summary>
        /// <remarks>
        /// Combina dados de veículos (Processo Fabril) e lotes (Cadeia de Fornecedores).
        /// Se não houver dados, retorna um conjunto de exemplo para demonstração.
        /// </remarks>
        /// <returns>DTO com meses, valores da fábrica e valores da cadeia.</returns>
        public async Task<GraficoEmissoesDto> ObterDadosGraficoAsync()
        {
            var resultado = new GraficoEmissoesDto();

            // 1. Buscar veículos com DataMontagem
            var veiculos = await ListarVeiculo();
            if (veiculos == null || !veiculos.Any())
            {
                return ObterDadosExemplo();
            }

            // 2. Buscar componentes e agrupar por VIN
            var componentes = await ListarVeiculoComponente();
            var dictComponentesPorVin = componentes?
                .GroupBy(c => c.fk_Veiculo_Vin)
                .ToDictionary(g => g.Key, g => g.ToList()) ?? new Dictionary<string, List<VeiculoComponente>>();

            // 3. Calcular emissão por veículo (soma dos componentes)
            const double fatorEmissaoPadrao = 2.5;
            var emissaoPorVeiculo = new Dictionary<string, double>();
            foreach (var v in veiculos)
            {
                double somaPeso = 0;
                if (dictComponentesPorVin.TryGetValue(v.Vin, out var comps))
                {
                    somaPeso = comps.Sum(c => c.PesoKg);
                }
                emissaoPorVeiculo[v.Vin] = somaPeso * fatorEmissaoPadrao;
            }

            // 4. Agrupar por mês/ano com base em DataMontagem
            var veiculosComData = veiculos
                .Where(v => v.DataMontagem.HasValue)
                .Select(v => new
                {
                    v.Vin,
                    MesAno = new DateTime(v.DataMontagem.Value.Year, v.DataMontagem.Value.Month, 1),
                    Emissao = emissaoPorVeiculo.GetValueOrDefault(v.Vin, 0)
                })
                .GroupBy(x => x.MesAno)
                .OrderBy(g => g.Key)
                .Select(g => new
                {
                    Mes = g.Key.ToString("MMM"),
                    Ano = g.Key.Year,
                    TotalEmissao = g.Sum(x => x.Emissao) / 1000 // converte para toneladas
                })
                .ToList();

            // 5. Se não houver dados com data, usar exemplo
            if (!veiculosComData.Any())
            {
                return ObterDadosExemplo();
            }

            // 6. Preencher resultado com dados dos veículos
            resultado.Meses = veiculosComData.Select(x => $"{x.Mes}/{x.Ano}").ToArray();
            resultado.ValoresFabrica = veiculosComData.Select(x => Math.Round(x.TotalEmissao, 1)).ToArray();

            // 7. Adicionar dados da cadeia de fornecedores (baseado em lotes)
            var lotes = await ListarLoteMateriaPrima();
            if (lotes != null && lotes.Any())
            {
                var lotesPorMes = lotes
                    .Where(l => l.DataProducao.HasValue)
                    .GroupBy(l => new DateTime(l.DataProducao.Value.Year, l.DataProducao.Value.Month, 1))
                    .OrderBy(g => g.Key)
                    .Select(g => new
                    {
                        Mes = g.Key.ToString("MMM"),
                        Ano = g.Key.Year,
                        TotalEmissao = g.Sum(l => l.QuantidadeKg * l.PegadaCarbonoPorKg) / 1000
                    })
                    .ToList();

                // Unificar meses de veículos e lotes
                var todosMeses = veiculosComData
                    .Select(x => new { x.Mes, x.Ano })
                    .Union(lotesPorMes.Select(x => new { x.Mes, x.Ano }))
                    .Distinct()
                    .OrderBy(x => x.Ano).ThenBy(x => x.Mes)
                    .ToList();

                resultado.Meses = todosMeses.Select(x => $"{x.Mes}/{x.Ano}").ToArray();
                resultado.ValoresFabrica = todosMeses.Select(m =>
                    veiculosComData.FirstOrDefault(x => x.Mes == m.Mes && x.Ano == m.Ano)?.TotalEmissao ?? 0
                ).ToArray();
                resultado.ValoresCadeia = todosMeses.Select(m =>
                    lotesPorMes.FirstOrDefault(x => x.Mes == m.Mes && x.Ano == m.Ano)?.TotalEmissao ?? 0
                ).ToArray();
            }
            else
            {
                resultado.ValoresCadeia = resultado.Meses.Select(_ => 0.0).ToArray();
            }

            // Garantir que não haja valores negativos
            resultado.ValoresFabrica = resultado.ValoresFabrica.Select(v => v < 0 ? 0 : v).ToArray();
            resultado.ValoresCadeia = resultado.ValoresCadeia.Select(v => v < 0 ? 0 : v).ToArray();

            return resultado;
        }

        /// <summary>
        /// Dados de exemplo para fallback (usado quando não há dados reais no banco).
        /// </summary>
        /// <returns>DTO com dados de exemplo.</returns>
        private GraficoEmissoesDto ObterDadosExemplo()
        {
            return new GraficoEmissoesDto
            {
                Meses = new[] { "Jan/2025", "Fev/2025", "Mar/2025", "Abr/2025", "Mai/2025", "Jun/2025" },
                ValoresFabrica = new double[] { 12.5, 15.2, 14.8, 18.5, 20.1, 22.0 },
                ValoresCadeia = new double[] { 8.0, 9.5, 8.8, 12.0, 14.5, 16.0 }
            };
        }

        /// <summary>
        /// Obtém dados consolidados para a página de Análise ESG.
        /// </summary>
        /// <remarks>
        /// Retorna:
        /// - Distribuição percentual de emissões por escopo (1, 2, 3)
        /// - Ranking dos Top 10 Fornecedores Verdes
        /// </remarks>
        /// <returns>DTO com distribuição de emissões e lista de fornecedores verdes.</returns>
        public async Task<AnalisesESGDto> ObterDadosAnalisesESGAsync()
        {
            var resultado = new AnalisesESGDto();

            // Buscar dados
            var veiculos = await ListarVeiculo();
            var componentes = await ListarVeiculoComponente();
            var fornecedores = await ListarFornecedor();

            const double fatorEmissaoPadrao = 2.5;

            // ============================================================
            // 1. Calcular emissões por fornecedor (baseado nos componentes)
            // ============================================================
            var dictFornecedorEmissao = new Dictionary<string, double>();
            var dictFornecedorPecas = new Dictionary<string, int>();

            foreach (var comp in componentes)
            {
                if (!string.IsNullOrEmpty(comp.fk_Fornecedor_Id))
                {
                    if (!dictFornecedorPecas.ContainsKey(comp.fk_Fornecedor_Id))
                        dictFornecedorPecas[comp.fk_Fornecedor_Id] = 0;
                    dictFornecedorPecas[comp.fk_Fornecedor_Id]++;

                    double emissao = comp.PesoKg * fatorEmissaoPadrao;
                    if (!dictFornecedorEmissao.ContainsKey(comp.fk_Fornecedor_Id))
                        dictFornecedorEmissao[comp.fk_Fornecedor_Id] = 0;
                    dictFornecedorEmissao[comp.fk_Fornecedor_Id] += emissao;
                }
            }

            // ============================================================
            // 2. Distribuição de Emissões (Escopo 1, 2, 3)
            // ============================================================
            double emissaoVeiculos = 0;
            foreach (var v in veiculos)
            {
                double somaPeso = componentes?.Where(c => c.fk_Veiculo_Vin == v.Vin).Sum(c => c.PesoKg) ?? 0;
                emissaoVeiculos += somaPeso * fatorEmissaoPadrao;
            }

            var lotes = await ListarLoteMateriaPrima();
            double emissaoLotes = lotes?.Sum(l => l.QuantidadeKg * l.PegadaCarbonoPorKg) ?? 0;

            double emissaoFornecedores = dictFornecedorEmissao.Values.Sum();

            double total = emissaoVeiculos + emissaoLotes + emissaoFornecedores;
            if (total > 0)
            {
                resultado.DistribuicaoEmissoes = new List<EscopoEmissaoDto>
                {
                    new EscopoEmissaoDto { Escopo = "Escopo 1 (Fábrica)", Porcentagem = Math.Round((emissaoVeiculos / total) * 100, 1) },
                    new EscopoEmissaoDto { Escopo = "Escopo 2 (Energia)", Porcentagem = Math.Round((emissaoLotes / total) * 100, 1) },
                    new EscopoEmissaoDto { Escopo = "Escopo 3 (Fornecedores)", Porcentagem = Math.Round((emissaoFornecedores / total) * 100, 1) }
                };
            }
            else
            {
                resultado.DistribuicaoEmissoes = new List<EscopoEmissaoDto>
                {
                    new EscopoEmissaoDto { Escopo = "Escopo 1 (Fábrica)", Porcentagem = 0 },
                    new EscopoEmissaoDto { Escopo = "Escopo 2 (Energia)", Porcentagem = 0 },
                    new EscopoEmissaoDto { Escopo = "Escopo 3 (Fornecedores)", Porcentagem = 0 }
                };
            }

            // ============================================================
            // 3. Top Fornecedores Verdes (baseado em componentes)
            // ============================================================
            var fornecedoresComDados = new List<FornecedorVerdeDto>();
            if (fornecedores != null)
            {
                foreach (var f in fornecedores)
                {
                    int totalPecas = dictFornecedorPecas.GetValueOrDefault(f.Id, 0);
                    double pegadaMedia = 0;
                    if (totalPecas > 0)
                    {
                        double emissaoTotal = dictFornecedorEmissao.GetValueOrDefault(f.Id, 0);
                        pegadaMedia = emissaoTotal / totalPecas;
                    }

                    double scoreVerde = 0;
                    if (pegadaMedia > 0)
                        scoreVerde = Math.Round((totalPecas * 10) / pegadaMedia, 2);
                    else if (totalPecas > 0)
                        scoreVerde = totalPecas * 5;

                    fornecedoresComDados.Add(new FornecedorVerdeDto
                    {
                        Id = f.Id,
                        Nome = f.Nome,
                        Localizacao = f.Localizacao,
                        TotalPecas = totalPecas,
                        PegadaMedia = Math.Round(pegadaMedia, 2),
                        ScoreVerde = scoreVerde,
                        Certificado = totalPecas == 0 ? "Sem dados" :
                                      (scoreVerde > 50 ? "ISO 14001" : "Pendente")
                    });
                }
            }

            resultado.TopFornecedoresVerdes = fornecedoresComDados
                .OrderByDescending(f => f.ScoreVerde)
                .Take(10)
                .ToList();

            return resultado;
        }
    }
}