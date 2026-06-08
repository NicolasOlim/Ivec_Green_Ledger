using ApiIveco.Data;
using ApiIveco.Models;
using Google.Cloud.Firestore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace ApiIveco.Service
{
    public class DadosService
    {
        private readonly ILogger<DadosService> _logger;
        private readonly FireBaseData _firestoreDb;

        private readonly string _collectionFornecedor = "fornecedores";
        private readonly string _collectionLote = "lotes_materia_prima";
        private readonly string _collectionVeiculo = "veiculos";
        private readonly string _collectionComponente = "veiculo_componentes";

        public DadosService(ILogger<DadosService> logger, FireBaseData firestoreDb)
        {
            _logger = logger;
            _firestoreDb = firestoreDb;
        }

        // ==========================================
        // MÉTODO EXTERNO: BUSCAR NA BRASILAPI (CNPJ)
        // ==========================================
        public async Task<Fornecedor> BuscarFornecedorPorCnpjAsync(string cnpj)
        {
            try
            {
                var cnpjLimpo = new string(cnpj.Where(char.IsDigit).ToArray());
                var url = $"https://brasilapi.com.br/api/cnpj/v1/{cnpjLimpo}";

                using var client = new HttpClient();
                // BLINDAGEM: Diz à Receita Federal que somos um software legítimo para não sermos bloqueados
                client.DefaultRequestHeaders.Add("User-Agent", "IvecoApp/1.0");

                var response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var data = JsonSerializer.Deserialize<BrasilApiCnpjResponse>(json, options);

                    if (data != null)
                    {
                        // Atualizado com os novos nomes da Model
                        string nomeEmpresa = !string.IsNullOrWhiteSpace(data.NomeFantasia)
                            ? data.NomeFantasia
                            : data.RazaoSocial;

                        string moradaCompleta = $"{data.Logradouro}, {data.Numero} - {data.Bairro}, {data.Municipio} - {data.Uf}";

                        return new Fornecedor
                        {
                            Id = string.Empty,
                            Nome = nomeEmpresa,
                            Localizacao = moradaCompleta,
                            Cnpj = cnpjLimpo
                        };
                    }
                }

                // Se o código chegar aqui, imprime o erro real no terminal da API
                Console.WriteLine($"[FALHA BRASIL API]: HTTP {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERRO EXCEÇÃO BRASILAPI]: {ex.Message}");
                return null;
            }
        }

        // ==========================================
        // MÉTODO EXTERNO: DESCODIFICAR VIN (NHTSA) COM VALIDAÇÃO IVECO
        // ==========================================
        public async Task<Veiculo> BuscarEValidarVinIvecoAsync(string vin)
        {
            try
            {
                // Limpa o VIN para garantir que não tem espaços
                var vinLimpo = vin.Trim().ToUpper();

                // URL oficial da API do Governo Americano
                var url = $"https://vpic.nhtsa.dot.gov/api/vehicles/decodevin/{vinLimpo}?format=json";

                using var client = new HttpClient();
                var response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var data = JsonSerializer.Deserialize<NhtsaResponse>(json);

                    if (data != null && data.Results != null)
                    {
                        // 1. Procura a Marca (Make)
                        var marca = data.Results.FirstOrDefault(r => r.Variable == "Make")?.Value;

                        // VALIDAÇÃO CRÍTICA: Se não for Iveco, rejeita imediatamente!
                        if (string.IsNullOrEmpty(marca) || !marca.ToUpper().Contains("IVECO"))
                        {
                            throw new Exception($"VIN inválido para este sistema. A marca detetada foi: {marca ?? "Desconhecida"}. Apenas veículos IVECO são permitidos.");
                        }

                        // Se for Iveco, extraímos o Modelo 
                        var modelo = data.Results.FirstOrDefault(r => r.Variable == "Model")?.Value;

                        // Retorna os dados formatados (Agora preenche tudo e não dá erro de validação!)
                        return new Veiculo
                        {
                            Vin = vinLimpo,
                            // Se a API americana não souber o modelo exato, colocamos um genérico
                            Modelo = string.IsNullOrWhiteSpace(modelo) ? "Iveco Não Especificado" : modelo,
                            DataMontagem = DateTime.UtcNow // Regista a data e hora atual
                        };
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                // Deixa o erro subir para o Controller para mostrar a mensagem ao utilizador
                throw;
            }
        }

        // ==========================================
        // MÉTODOS FIREBASE: FORNECEDORES
        // ==========================================
        public async Task<List<Fornecedor>> ListarFornecedor()
        {
            CollectionReference collection = _firestoreDb.Db.Collection(_collectionFornecedor);
            QuerySnapshot snapshot = await collection.GetSnapshotAsync();
            List<Fornecedor> fornecedores = new List<Fornecedor>();

            foreach (DocumentSnapshot document in snapshot.Documents)
            {
                if (document.Exists)
                {
                    var fornecedor = document.ConvertTo<Fornecedor>();
                    fornecedor.Id = document.Id;
                    fornecedores.Add(fornecedor);
                }
            }
            return fornecedores;
        }

        public async Task<Fornecedor> CriarFornecedor(Fornecedor fornecedor)
        {
            int novoId = await GerarProximoId("contador_fornecedor");
            fornecedor.Id = novoId.ToString();

            DocumentReference docRef = _firestoreDb.Db.Collection(_collectionFornecedor).Document(fornecedor.Id);
            await docRef.SetAsync(fornecedor);
            return fornecedor;
        }

        public async Task ExcluirFornecedor(string id)
        {
            if (string.IsNullOrEmpty(id)) throw new ArgumentException("O ID do fornecedor não pode ser nulo ou vazio.");
            DocumentReference docRef = _firestoreDb.Db.Collection(_collectionFornecedor).Document(id);
            await docRef.DeleteAsync();
        }

        // ==========================================
        // MÉTODOS FIREBASE: LOTES MATÉRIA-PRIMA
        // ==========================================
        public async Task<List<LoteMateriaPrima>> ListarLoteMateriaPrima()
        {
            CollectionReference collection = _firestoreDb.Db.Collection(_collectionLote);
            QuerySnapshot snapshot = await collection.GetSnapshotAsync();
            List<LoteMateriaPrima> lotes = new List<LoteMateriaPrima>();
            foreach (DocumentSnapshot document in snapshot.Documents)
            {
                if (document.Exists)
                {
                    var lote = document.ConvertTo<LoteMateriaPrima>();
                    lote.Id = document.Id;
                    lotes.Add(lote);
                }
            }
            return lotes;
        }

        public async Task<LoteMateriaPrima> CriarLoteMateriaPrima(LoteMateriaPrima lote)
        {
            int novoId = await GerarProximoId("contador_lote");
            lote.Id = novoId.ToString();
            DocumentReference docRef = _firestoreDb.Db.Collection(_collectionLote).Document(lote.Id);
            await docRef.SetAsync(lote);
            return lote;
        }

        public async Task ExcluirLoteMateriaPrima(string id)
        {
            if (string.IsNullOrEmpty(id)) throw new ArgumentException("O ID não pode ser nulo ou vazio.");
            DocumentReference docRef = _firestoreDb.Db.Collection(_collectionLote).Document(id);
            await docRef.DeleteAsync();
        }

        // ==========================================
        // MÉTODOS FIREBASE: VEÍCULO COMPONENTES
        // ==========================================
        public async Task<List<VeiculoComponente>> ListarVeiculoComponente()
        {
            CollectionReference collection = _firestoreDb.Db.Collection(_collectionComponente);
            QuerySnapshot snapshot = await collection.GetSnapshotAsync();
            List<VeiculoComponente> componentes = new List<VeiculoComponente>();
            foreach (DocumentSnapshot document in snapshot.Documents)
            {
                if (document.Exists)
                {
                    var componente = document.ConvertTo<VeiculoComponente>();
                    componente.Id = document.Id;
                    componentes.Add(componente);
                }
            }
            return componentes;
        }

        public async Task<VeiculoComponente> CriarVeiculoComponente(VeiculoComponente componente)
        {
            int novoId = await GerarProximoId("contador_componente");
            componente.Id = novoId.ToString();
            DocumentReference docRef = _firestoreDb.Db.Collection(_collectionComponente).Document(componente.Id);
            await docRef.SetAsync(componente);
            return componente;
        }

        public async Task ExcluirVeiculoComponente(string id)
        {
            if (string.IsNullOrEmpty(id)) throw new ArgumentException("O ID não pode ser nulo ou vazio.");
            DocumentReference docRef = _firestoreDb.Db.Collection(_collectionComponente).Document(id);
            await docRef.DeleteAsync();
        }

        // ==========================================
        // MÉTODOS FIREBASE: VEÍCULOS
        // ==========================================
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
                    veiculo.Vin = document.Id;
                    veiculos.Add(veiculo);
                }
            }
            return veiculos;
        }

        /// <summary>
        /// Busca peças reais no Mercado Livre para um determinado VIN e associa ao veículo.
        /// </summary>
        public async Task<List<VeiculoComponente>> GerarComponentesParaVeiculoAsync(string vin)
        {
            var componentes = new List<VeiculoComponente>();

            using (var httpClient = new HttpClient())
            {
                // Pesquisa 5 peças reais de caminhão Iveco no Mercado Livre (Brasil)
                string url = "https://api.mercadolibre.com/sites/MLB/search?q=peca+caminhao+iveco&limit=5";

                try
                {
                    // Faz a requisição à API do Mercado Livre
                    var response = await httpClient.GetAsync(url);

                    if (response.IsSuccessStatusCode)
                    {
                        var jsonResult = await response.Content.ReadAsStringAsync();
                        using var doc = JsonDocument.Parse(jsonResult);

                        // Entra na lista de resultados da pesquisa
                        var resultados = doc.RootElement.GetProperty("results");

                        foreach (var item in resultados.EnumerateArray())
                        {
                            // Cria a peça baseada nos dados REAIS do anúncio
                            var novaPeca = new VeiculoComponente
                            {
                                // Gera um ID único para o nosso banco juntando o ID do anúncio com um Guid
                                Id = item.GetProperty("id").GetString() + "-" + Guid.NewGuid().ToString().Substring(0, 5),

                                // Pega o Título real do anúncio no Mercado Livre
                                NomePeca = item.GetProperty("title").GetString(),

                                fk_Veiculo_Vin = vin,

                                // Associa a um lote fictício para manter a integridade da sua arquitetura
                                fk_LoteMateriaPrima_Id = "LOTE-ML-" + DateTime.Now.ToString("yyyyMMdd")
                            };

                            componentes.Add(novaPeca);

                            // AQUI: Se você tem um método para salvar direto no Firebase, chame-o!
                            // await CriarVeiculoComponente(novaPeca); 
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
        public async Task<Veiculo> CriarVeiculo(Veiculo veiculo)
        {
            if (string.IsNullOrEmpty(veiculo.Vin)) throw new ArgumentException("O veículo deve possuir um VIN válido.");
            DocumentReference docRef = _firestoreDb.Db.Collection(_collectionVeiculo).Document(veiculo.Vin);
            await docRef.SetAsync(veiculo);
            return veiculo;
        }

        public async Task<Veiculo> ObterVeiculoPorVin(string vin)
        {
            var veiculos = await ListarVeiculo();
            return veiculos.FirstOrDefault(v => v.Vin.Equals(vin, StringComparison.OrdinalIgnoreCase));
        }

        public async Task ExcluirVeiculo(string vin)
        {
            if (string.IsNullOrEmpty(vin)) throw new ArgumentException("O VIN não pode ser nulo ou vazio.");
            DocumentReference docRef = _firestoreDb.Db.Collection(_collectionVeiculo).Document(vin);
            await docRef.DeleteAsync();
        }

        // ==========================================
        // MÉTODO AUXILIAR: GERADOR DE IDS NO FIREBASE
        // ==========================================
        private async Task<int> GerarProximoId(string nomeContador)
        {
            DocumentReference contadorId = _firestoreDb.Db.Collection("contadores").Document(nomeContador);
            return await _firestoreDb.Db.RunTransactionAsync(async transaction =>
            {
                DocumentSnapshot snapshot = await transaction.GetSnapshotAsync(contadorId);
                int idAtual = 0;
                if (snapshot.Exists) snapshot.TryGetValue("ultimoId", out idAtual);

                int proximoId = idAtual + 1;
                Dictionary<string, object> atualizacaoContador = new Dictionary<string, object> { { "ultimoId", proximoId } };
                transaction.Set(contadorId, atualizacaoContador, SetOptions.MergeAll);
                return proximoId;
            });
        }

        // ==========================================
        // MÉTODOS DE AUTENTICAÇÃO (USUÁRIOS)
        // ==========================================
        public async Task<Usuario> CadastrarUsuario(Usuario novoUsuario)
        {
            var usuariosRef = _firestoreDb.Db.Collection("Usuarios");

            // Verifica se email já existe
            var query = await usuariosRef
                .WhereEqualTo("Email", novoUsuario.Email)
                .GetSnapshotAsync();

            if (query.Documents.Count > 0)
                throw new Exception("Já existe um usuário cadastrado com este e-mail.");

            // Garante valor padrão
            if (string.IsNullOrWhiteSpace(novoUsuario.Acesso))
                novoUsuario.Acesso = "Usuario";

            // Gera ID incremental
            int novoId = await GerarProximoId("contador_usuario");
            novoUsuario.Id = novoId.ToString();
            novoUsuario.DataCriacao = DateTime.UtcNow;

            // Salva — Id NÃO será campo do documento pois não tem [FirestoreProperty]
            DocumentReference docRef = usuariosRef.Document(novoUsuario.Id);
            await docRef.SetAsync(novoUsuario);

            return novoUsuario;
        }

        // Em ApiIveco/Service/DadosService.cs

        public async Task<Veiculo> AtualizarVeiculo(string vin, Veiculo veiculoAtualizado)
        {
            if (string.IsNullOrEmpty(vin)) throw new ArgumentException("O VIN não pode ser nulo ou vazio.");

            DocumentReference docRef = _firestoreDb.Db.Collection(_collectionVeiculo).Document(vin);
            DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();

            // Verifica se o veículo existe antes de atualizar
            if (!snapshot.Exists) return null;

            // Mantém a integridade da chave primária (o VIN original da URL)
            veiculoAtualizado.Vin = vin;

            // MergeAll mescla os dados novos com os existentes (faz o papel de um PUT/PATCH)
            await docRef.SetAsync(veiculoAtualizado, SetOptions.MergeAll);

            return veiculoAtualizado;
        }

        public async Task<Usuario> FazerLogin(string email, string senha)
        {
            _logger.LogCritical("### LOGIN PARA: {email}", email);

            var usuariosRef = _firestoreDb.Db.Collection("Usuarios");

            // Busca todos e compara manualmente — evita problemas de query
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
    }
 }