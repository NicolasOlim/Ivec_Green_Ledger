using ApiIveco.Data;
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
    public class DadosService
    {
        private readonly ILogger<DadosService> _logger;
        private readonly FireBaseData _firestoreDb;
        private readonly IMemoryCache _cache;

        private readonly string _collectionFornecedor = "fornecedores";
        private readonly string _collectionLote = "lotes_materia_prima";
        private readonly string _collectionVeiculo = "veiculos";
        private readonly string _collectionComponente = "veiculo_componentes";

        public DadosService(ILogger<DadosService> logger, FireBaseData firestoreDb, IMemoryCache memoryCache)
        {
            _logger = logger;
            _firestoreDb = firestoreDb;
            _cache = memoryCache;
        }

        
        /// <summary>
        /// MÉTODO EXTERNO: BUSCAR NA BRASILAPI (CNPJ)
        /// </summary>
        /// <param name="cnpj"></param>
        /// <returns></returns>
        
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

                /// Se o código chegar aqui, imprime o erro real no terminal da API
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
        /// MÉTODO EXTERNO: DESCODIFICAR VIN (NHTSA) COM VALIDAÇÃO IVECO
        /// </summary>
        /// <param name="vin"></param>
        /// <returns></returns>
        
        public async Task<Veiculo> BuscarEValidarVinIvecoAsync(string vin)
        {
            try
            {
                /// Limpa o VIN para garantir que não tem espaços
                var vinLimpo = vin.Trim().ToUpper();

                /// URL oficial da API do Governo Americano
                var url = $"https://vpic.nhtsa.dot.gov/api/vehicles/decodevin/{vinLimpo}?format=json";

                using var client = new HttpClient();
                var response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var data = JsonSerializer.Deserialize<NhtsaResponse>(json);

                    if (data != null && data.Results != null)
                    {
                        /// 1. Procura a Marca (Make)
                        var marca = data.Results.FirstOrDefault(r => r.Variable == "Make")?.Value;

                        /// VALIDAÇÃO CRÍTICA: Se não for Iveco, rejeita imediatamente!
                        if (string.IsNullOrEmpty(marca) || !marca.ToUpper().Contains("IVECO"))
                        {
                            throw new Exception($"VIN inválido para este sistema. A marca detetada foi: {marca ?? "Desconhecida"}. Apenas veículos IVECO são permitidos.");
                        }

                        /// Se for Iveco, extraímos o Modelo 
                        var modelo = data.Results.FirstOrDefault(r => r.Variable == "Model")?.Value;

                        /// Retorna os dados formatados (Agora preenche tudo e não dá erro de validação!)
                        return new Veiculo
                        {
                            Vin = vinLimpo,
                            /// Se a API americana não souber o modelo exato, colocamos um genérico
                            Modelo = string.IsNullOrWhiteSpace(modelo) ? "Iveco Não Especificado" : modelo,
                            DataMontagem = DateTime.UtcNow // Regista a data e hora atual
                        };
                    }
                }
                return null;
            }
            catch (Exception ex)
            {
                /// Deixa o erro subir para o Controller para mostrar a mensagem ao utilizador
                throw;
            }
        }

        
        /// <summary>
        /// MÉTODOS FIREBASE: FORNECEDORES
        /// </summary>
        /// <returns></returns>
       
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

        
        /// <summary>
        /// MÉTODOS FIREBASE: LOTES MATÉRIA-PRIMA
        /// </summary>
        /// <returns></returns>
        
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

            // Invalida cache
            _cache.Remove("PegadaMediaCache");

            return lote;
        }

        public async Task ExcluirLoteMateriaPrima(string id)
        {
            if (string.IsNullOrEmpty(id)) throw new ArgumentException("O ID não pode ser nulo ou vazio.");
            DocumentReference docRef = _firestoreDb.Db.Collection(_collectionLote).Document(id);
            await docRef.DeleteAsync();
            _cache.Remove("PegadaMediaCache");
        }

        
        /// <summary>
        /// MÉTODOS FIREBASE: VEÍCULO COMPONENTES
        /// </summary>
        /// <returns></returns>
        
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

            // Invalida cache
            _cache.Remove("PegadaMediaCache");

            return componente;
        }

        public async Task ExcluirVeiculoComponente(string id)
        {
            if (string.IsNullOrEmpty(id)) throw new ArgumentException("O ID não pode ser nulo ou vazio.");
            DocumentReference docRef = _firestoreDb.Db.Collection(_collectionComponente).Document(id);
            await docRef.DeleteAsync();
            _cache.Remove("PegadaMediaCache");
        }

        
        /// <summary>
        /// MÉTODOS FIREBASE: VEÍCULOS
        /// </summary>
        /// <returns></returns>
        
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
                /// Pesquisa 5 peças reais de caminhão Iveco no Mercado Livre (Brasil)
                string url = "https://api.mercadolibre.com/sites/MLB/search?q=peca+caminhao+iveco&limit=5";

                try
                {
                    /// Faz a requisição à API do Mercado Livre
                    var response = await httpClient.GetAsync(url);

                    if (response.IsSuccessStatusCode)
                    {
                        var jsonResult = await response.Content.ReadAsStringAsync();
                        using var doc = JsonDocument.Parse(jsonResult);

                        /// Entra na lista de resultados da pesquisa
                        var resultados = doc.RootElement.GetProperty("results");

                        foreach (var item in resultados.EnumerateArray())
                        {
                            /// Cria a peça baseada nos dados REAIS do anúncio
                            var novaPeca = new VeiculoComponente
                            {
                                /// Gera um ID único para o nosso banco juntando o ID do anúncio com um Guid
                                Id = item.GetProperty("id").GetString() + "-" + Guid.NewGuid().ToString().Substring(0, 5),

                                /// Pega o Título real do anúncio no Mercado Livre
                                NomePeca = item.GetProperty("title").GetString(),

                                fk_Veiculo_Vin = vin,

                                /// Associa a um lote fictício para manter a integridade da sua arquitetura
                                fk_LoteMateriaPrima_Id = "LOTE-ML-" + DateTime.Now.ToString("yyyyMMdd")
                            };

                            componentes.Add(novaPeca);

                            /// AQUI: Se você tem um método para salvar direto no Firebase, chame-o!
                            /// await CriarVeiculoComponente(novaPeca); 
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
            _cache.Remove("PegadaMediaCache");
        }

        
        /// <summary>
        /// MÉTODO AUXILIAR: GERADOR DE IDS NO FIREBASE
        /// </summary>
        /// <param name="nomeContador"></param>
        /// <returns></returns>
        
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

        
        /// <summary>
        /// MÉTODOS DE AUTENTICAÇÃO (USUÁRIOS)
        /// </summary>
        /// <param name="novoUsuario"></param>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        
        public async Task<Usuario> CadastrarUsuario(Usuario novoUsuario)
        {
            var usuariosRef = _firestoreDb.Db.Collection("Usuarios");

            /// Verifica se email já existe
            var query = await usuariosRef
                .WhereEqualTo("Email", novoUsuario.Email)
                .GetSnapshotAsync();

            if (query.Documents.Count > 0)
                throw new Exception("Já existe um usuário cadastrado com este e-mail.");

            /// Garante valor padrão
            if (string.IsNullOrWhiteSpace(novoUsuario.Acesso))
                novoUsuario.Acesso = "Usuario";

            /// Gera ID incremental
            int novoId = await GerarProximoId("contador_usuario");
            novoUsuario.Id = novoId.ToString();
            novoUsuario.DataCriacao = DateTime.UtcNow;

            /// Salva — Id NÃO será campo do documento pois não tem [FirestoreProperty]
            DocumentReference docRef = usuariosRef.Document(novoUsuario.Id);
            await docRef.SetAsync(novoUsuario);

            return novoUsuario;
        }

        /// <summary>
        /// Em ApiIveco/Service/DadosService.cs
        /// </summary>
        /// <param name="vin"></param>
        /// <param name="veiculoAtualizado"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>

        public async Task<Veiculo> AtualizarVeiculo(string vin, Veiculo veiculoAtualizado)
        {
            if (string.IsNullOrEmpty(vin)) throw new ArgumentException("O VIN não pode ser nulo ou vazio.");

            DocumentReference docRef = _firestoreDb.Db.Collection(_collectionVeiculo).Document(vin);
            DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();

            /// Verifica se o veículo existe antes de atualizar
            if (!snapshot.Exists) return null;

            /// Mantém a integridade da chave primária (o VIN original da URL)
            veiculoAtualizado.Vin = vin;

            /// MergeAll mescla os dados novos com os existentes (faz o papel de um PUT/PATCH)
            await docRef.SetAsync(veiculoAtualizado, SetOptions.MergeAll);

            return veiculoAtualizado;
        }

        public async Task<Usuario> FazerLogin(string email, string senha)
        {
            _logger.LogCritical("### LOGIN PARA: {email}", email);

            var usuariosRef = _firestoreDb.Db.Collection("Usuarios");

            /// Busca todos e compara manualmente — evita problemas de query
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

        /// <summary>
        /// Calcula a pegada de carbono média (kg CO2) a partir dos lotes de matéria-prima.
        /// </summary>
        public async Task<double> CalcularPegadaMediaAsync()
        {
            const string cacheKey = "PegadaMediaCache";

            // Tenta obter do cache
            if (_cache.TryGetValue(cacheKey, out double cachedValue))
            {
                return cachedValue;
            }

            // Se não estiver em cache, calcula
            double resultado = await CalcularPegadaMediaInternoAsync();

            // Armazena em cache por 5 minutos
            _cache.Set(cacheKey, resultado, TimeSpan.FromMinutes(5));

            return resultado;
        }

        /// <summary>
        /// Cálculo real (sem cache) – chamado internamente
        /// </summary>
        private async Task<double> CalcularPegadaMediaInternoAsync()
        {
            // 1. Tenta calcular a partir dos lotes de matéria-prima
            var lotes = await ListarLoteMateriaPrima();
            if (lotes != null && lotes.Count > 0)
            {
                double somaPegada = 0;
                foreach (var lote in lotes)
                {
                    // Pegada total do lote = QuantidadeKg * PegadaCarbonoPorKg
                    somaPegada += lote.QuantidadeKg * lote.PegadaCarbonoPorKg;
                }
                return somaPegada / lotes.Count;
            }

            // 2. Se não há lotes, calcula a partir dos componentes (peças)
            var componentes = await ListarVeiculoComponente();
            if (componentes == null || componentes.Count == 0)
                return 0;

            // Fator de emissão padrão (kg CO2 por kg de peça) – valor típico para metais
            const double FatorEmissaoPadrao = 2.5;

            // Agrupa componentes por veículo
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

            // Média por veículo (apenas veículos que têm peças)
            return totalVeiculosComPecas > 0 ? somaPegadaPorVeiculo / totalVeiculosComPecas : 0;
        }

        /// <summary>
        /// Retorna dados de emissões por mês para o gráfico YTD.
        /// </summary>
        public async Task<GraficoEmissoesDto> ObterDadosGraficoAsync()
        {
            var resultado = new GraficoEmissoesDto();

            // 1. Buscar veículos com DataMontagem
            var veiculos = await ListarVeiculo();
            if (veiculos == null || !veiculos.Any())
            {
                // Fallback: dados de exemplo
                return ObterDadosExemplo();
            }

            // 2. Buscar componentes
            var componentes = await ListarVeiculoComponente();
            var dictComponentesPorVin = componentes?
                .GroupBy(c => c.fk_Veiculo_Vin)
                .ToDictionary(g => g.Key, g => g.ToList()) ?? new Dictionary<string, List<VeiculoComponente>>();

            // 3. Calcular emissão por veículo (soma dos componentes)
            const double fatorEmissaoPadrao = 2.5; // kg CO2/kg
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

            // 4. Agrupar por mês/ano com base em DataMontagem (apenas veículos com data)
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

            // 6. Preencher resultado
            resultado.Meses = veiculosComData.Select(x => $"{x.Mes}/{x.Ano}").ToArray();
            resultado.ValoresFabrica = veiculosComData.Select(x => Math.Round(x.TotalEmissao, 1)).ToArray();

            // 7. Cadeia de Fornecedores (baseado em lotes)
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
                        TotalEmissao = g.Sum(l => l.QuantidadeKg * l.PegadaCarbonoPorKg) / 1000 // toneladas
                    })
                    .ToList();

                // Unificar os meses (usar todos os meses de veículos e lotes)
                var todosMeses = veiculosComData
                    .Select(x => new { x.Mes, x.Ano })
                    .Union(lotesPorMes.Select(x => new { x.Mes, x.Ano }))
                    .Distinct()
                    .OrderBy(x => x.Ano).ThenBy(x => x.Mes)
                    .ToList();

                // Recalcular séries para ter os mesmos meses
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
                // Se não houver lotes, usar zeros
                resultado.ValoresCadeia = resultado.Meses.Select(_ => 0.0).ToArray();
            }

            // Garantir que não haja valores negativos
            resultado.ValoresFabrica = resultado.ValoresFabrica.Select(v => v < 0 ? 0 : v).ToArray();
            resultado.ValoresCadeia = resultado.ValoresCadeia.Select(v => v < 0 ? 0 : v).ToArray();

            return resultado;
        }

        /// <summary>
        /// Dados de exemplo (fallback)
        /// </summary>
        private GraficoEmissoesDto ObterDadosExemplo()
        {
            return new GraficoEmissoesDto
            {
                Meses = new[] { "Jan/2025", "Fev/2025", "Mar/2025", "Abr/2025", "Mai/2025", "Jun/2025" },
                ValoresFabrica = new double[] { 12.5, 15.2, 14.8, 18.5, 20.1, 22.0 },
                ValoresCadeia = new double[] { 8.0, 9.5, 8.8, 12.0, 14.5, 16.0 }
            };
        }

        public class GraficoEmissoesDto
        {
            public string[] Meses { get; set; } = Array.Empty<string>();
            public double[] ValoresFabrica { get; set; } = Array.Empty<double>();
            public double[] ValoresCadeia { get; set; } = Array.Empty<double>();
        }



        /// <summary>
        /// Retorna dados para a página Análise ESG: distribuição de emissões por escopo e top fornecedores verdes.
        /// Agora baseado em componentes (peças) associados a fornecedores.
        /// </summary>
        public async Task<AnalisesESGDto> ObterDadosAnalisesESGAsync()
        {
            var resultado = new AnalisesESGDto();

            // Buscar dados
            var veiculos = await ListarVeiculo();
            var componentes = await ListarVeiculoComponente();
            var fornecedores = await ListarFornecedor();

            const double fatorEmissaoPadrao = 2.5; // kg CO2/kg

            // ============================================================
            // 1. Calcular emissões por fornecedor (baseado nos componentes)
            // ============================================================
            var dictFornecedorEmissao = new Dictionary<string, double>();
            var dictFornecedorPecas = new Dictionary<string, int>();

            foreach (var comp in componentes)
            {
                if (!string.IsNullOrEmpty(comp.fk_Fornecedor_Id))
                {
                    // Soma o peso das peças por fornecedor
                    if (!dictFornecedorPecas.ContainsKey(comp.fk_Fornecedor_Id))
                        dictFornecedorPecas[comp.fk_Fornecedor_Id] = 0;
                    dictFornecedorPecas[comp.fk_Fornecedor_Id]++;

                    // Soma a emissão (PesoKg * fator) por fornecedor
                    double emissao = comp.PesoKg * fatorEmissaoPadrao;
                    if (!dictFornecedorEmissao.ContainsKey(comp.fk_Fornecedor_Id))
                        dictFornecedorEmissao[comp.fk_Fornecedor_Id] = 0;
                    dictFornecedorEmissao[comp.fk_Fornecedor_Id] += emissao;
                }
            }

            // ============================================================
            // 2. Distribuição de Emissões (Escopo 1, 2, 3)
            // ============================================================
            // Escopo 1 – Veículos (soma das emissões de todos os componentes, independente de fornecedor)
            double emissaoVeiculos = 0;
            foreach (var v in veiculos)
            {
                double somaPeso = componentes?.Where(c => c.fk_Veiculo_Vin == v.Vin).Sum(c => c.PesoKg) ?? 0;
                emissaoVeiculos += somaPeso * fatorEmissaoPadrao;
            }

            // Escopo 2 – Lotes (mantido, mas você pode remover se não usar lotes)
            var lotes = await ListarLoteMateriaPrima();
            double emissaoLotes = lotes?.Sum(l => l.QuantidadeKg * l.PegadaCarbonoPorKg) ?? 0;

            // Escopo 3 – Fornecedores (soma das emissões de componentes agrupadas por fornecedor)
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
                        pegadaMedia = emissaoTotal / totalPecas; // média por peça
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


        public class AnalisesESGDto
        {
            public List<EscopoEmissaoDto> DistribuicaoEmissoes { get; set; }
            public List<FornecedorVerdeDto> TopFornecedoresVerdes { get; set; }
        }

        public class EscopoEmissaoDto
        {
            public string Escopo { get; set; }
            public double Porcentagem { get; set; }
        }

        public class FornecedorVerdeDto
        {
            public string Id { get; set; }
            public string Nome { get; set; }
            public string Localizacao { get; set; }
            public int TotalPecas { get; set; }
            public double PegadaMedia { get; set; }
            public double ScoreVerde { get; set; }
            public string Certificado { get; set; }
        }
    }
 }