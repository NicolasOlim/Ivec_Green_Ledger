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

        // ================================================================
        // MÉTODOS EXTERNOS - INTEGRAÇÕES COM APIs TERCEIRAS
        // ================================================================

        public async Task<Fornecedor> BuscarFornecedorPorCnpjAsync(string cnpj)
        {
            try
            {
                var cnpjLimpo = new string(cnpj.Where(char.IsDigit).ToArray());
                var url = $"https://brasilapi.com.br/api/cnpj/v1/{cnpjLimpo}";

                using var client = new HttpClient();
                client.DefaultRequestHeaders.Add("User-Agent", "IvecoApp/1.0");

                var response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var data = JsonSerializer.Deserialize<BrasilApiCnpjResponse>(json, options);

                    if (data != null)
                    {
                        string nomeEmpresa = !string.IsNullOrWhiteSpace(data.NomeFantasia)
                            ? data.NomeFantasia
                            : data.RazaoSocial;

                        string moradaCompleta = $"{data.Logradouro}, {data.Numero} - {data.Bairro}, {data.Municipio} - {data.Uf}";

                        return new Fornecedor
                        {
                            Id = string.Empty,
                            Nome = nomeEmpresa,
                            Localizacao = moradaCompleta,
                            Cnpj = cnpjLimpo,
                            CategoriaEsg = "Não avaliado"
                        };
                    }
                }

                Console.WriteLine($"[FALHA BRASIL API]: HTTP {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERRO EXCEÇÃO BRASILAPI]: {ex.Message}");
                return null;
            }
        }

        public async Task<Veiculo> BuscarEValidarVinIvecoAsync(string vin)
        {
            try
            {
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
                        var marca = data.Results.FirstOrDefault(r => r.Variable == "Make")?.Value;

                        if (string.IsNullOrEmpty(marca) || !marca.ToUpper().Contains("IVECO"))
                        {
                            throw new Exception($"VIN inválido para este sistema. A marca detetada foi: {marca ?? "Desconhecida"}. Apenas veículos IVECO são permitidos.");
                        }

                        var modelo = data.Results.FirstOrDefault(r => r.Variable == "Model")?.Value;

                        return new Veiculo
                        {
                            Vin = vinLimpo,
                            Modelo = string.IsNullOrWhiteSpace(modelo) ? "Iveco Não Especificado" : modelo,
                            DataMontagem = DateTime.UtcNow
                        };
                    }
                }
                return null;
            }
            catch
            {
                throw;
            }
        }

        // ================================================================
        // MÉTODOS FIREBASE: FORNECEDORES
        // ================================================================

        public async Task<List<Fornecedor>> ListarFornecedor()
        {
            CollectionReference collection = _firestoreDb.Db.Collection(_collectionFornecedor);
            QuerySnapshot snapshot = await collection.GetSnapshotAsync();
            List<Fornecedor> fornecedores = new List<Fornecedor>();

            foreach (DocumentSnapshot document in snapshot.Documents)
            {
                if (document.Exists)
                {
                    var fornecedor = new Fornecedor
                    {
                        Id = document.Id,
                        Nome = document.TryGetValue("Nome", out string nome) ? nome : null,
                        Localizacao = document.TryGetValue("Localizacao", out string localizacao) ? localizacao : null,
                        Cnpj = document.TryGetValue("Cnpj", out string cnpj) ? cnpj : null,
                        CategoriaEsg = document.TryGetValue("CategoriaEsg", out string cat) ? cat : "Não avaliado"
                    };
                    fornecedores.Add(fornecedor);
                }
            }
            return fornecedores;
        }

        public async Task<Fornecedor> CriarFornecedor(Fornecedor fornecedor)
        {
            int novoId = await GerarProximoId("contador_fornecedor");
            fornecedor.Id = novoId.ToString();

            if (string.IsNullOrWhiteSpace(fornecedor.CategoriaEsg))
                fornecedor.CategoriaEsg = "Não avaliado";

            var dados = new Dictionary<string, object>
            {
                { "Nome",           fornecedor.Nome          ?? "" },
                { "Localizacao",    fornecedor.Localizacao   ?? "" },
                { "Cnpj",           fornecedor.Cnpj          ?? "" },
                { "CategoriaEsg",   fornecedor.CategoriaEsg  ?? "Não avaliado" }
            };

            DocumentReference docRef = _firestoreDb.Db.Collection(_collectionFornecedor).Document(fornecedor.Id);
            await docRef.SetAsync(dados);
            return fornecedor;
        }

        public async Task ExcluirFornecedor(string id)
        {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentException("O ID do fornecedor não pode ser nulo ou vazio.");

            var lotesAtivos = await ListarLoteMateriaPrima();
            bool possuiLotes = lotesAtivos.Any(l => l.fk_Fornecedor_Id == id);

            if (possuiLotes)
            {
                throw new InvalidOperationException("Operação Bloqueada: Este fornecedor possui lotes de matéria-prima associados. A exclusão comprometeria a rastreabilidade do Escopo 3.");
            }

            DocumentReference docRef = _firestoreDb.Db.Collection(_collectionFornecedor).Document(id);
            await docRef.DeleteAsync();
        }

        // ================================================================
        // MÉTODOS FIREBASE: LOTES DE MATÉRIA-PRIMA
        // ================================================================

        public async Task<List<LoteMateriaPrima>> ListarLoteMateriaPrima()
        {
            CollectionReference collection = _firestoreDb.Db.Collection(_collectionLote);
            QuerySnapshot snapshot = await collection.GetSnapshotAsync();
            List<LoteMateriaPrima> lotes = new List<LoteMateriaPrima>();

            foreach (DocumentSnapshot document in snapshot.Documents)
            {
                if (document.Exists)
                {
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

        public async Task<LoteMateriaPrima> CriarLoteMateriaPrima(LoteMateriaPrima lote)
        {
            if (lote.QuantidadeKg <= 0)
                throw new ArgumentException("A quantidade de matéria-prima (Kg) deve ser maior que zero.");

            if (lote.PegadaCarbonoPorKg < 0)
                throw new ArgumentException("O fator de Pegada de Carbono não pode ser um número negativo.");

            if (lote.DataProducao.HasValue && lote.DataProducao.Value > DateTime.UtcNow)
                throw new ArgumentException("Violação Temporal: A data de produção do lote não pode estar no futuro.");

            int novoId = await GerarProximoId("contador_lote");
            lote.Id = novoId.ToString();

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

            _cache.Remove("PegadaMediaCache");

            return lote;
        }

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

        public async Task<List<VeiculoComponente>> ListarVeiculoComponente()
        {
            CollectionReference collection = _firestoreDb.Db.Collection(_collectionComponente);
            QuerySnapshot snapshot = await collection.GetSnapshotAsync();
            List<VeiculoComponente> componentes = new List<VeiculoComponente>();

            foreach (DocumentSnapshot document in snapshot.Documents)
            {
                if (document.Exists)
                {
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

        // ================================================================
        // MÉTODO CORRIGIDO: CriarVeiculoComponente
        // ================================================================
        public async Task<VeiculoComponente> CriarVeiculoComponente(VeiculoComponente componente)
        {
            // REGRA DE NEGÓCIO: Peso válido
            if (componente.PesoKg <= 0)
                throw new ArgumentException("O peso da peça deve ser maior que zero.");

            // REGRA DE NEGÓCIO: Balanço de Massa do Lote
            // AGORA TRATA string.Empty COMO "SEM LOTE"
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

            // Geração do ID com fallback em caso de falha do contador
            int novoId;
            try
            {
                novoId = await GerarProximoId("contador_componente");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha ao gerar ID incremental para componente. Usando fallback com GUID.");
                novoId = (int)(DateTime.UtcNow.Ticks % 1000000) + new Random().Next(1, 999);
            }

            componente.Id = novoId.ToString();

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

        public async Task<Veiculo> CriarVeiculo(Veiculo veiculo)
        {
            if (string.IsNullOrEmpty(veiculo.Vin))
                throw new ArgumentException("O veículo deve possuir um VIN válido.");
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
            if (string.IsNullOrEmpty(vin))
                throw new ArgumentException("O VIN não pode ser nulo ou vazio.");
            DocumentReference docRef = _firestoreDb.Db.Collection(_collectionVeiculo).Document(vin);
            await docRef.DeleteAsync();
        }

        public async Task<Veiculo> AtualizarVeiculo(string vin, Veiculo veiculoAtualizado)
        {
            if (string.IsNullOrEmpty(vin))
                throw new ArgumentException("O VIN não pode ser nulo ou vazio.");

            DocumentReference docRef = _firestoreDb.Db.Collection(_collectionVeiculo).Document(vin);
            DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();

            if (!snapshot.Exists)
                return null;

            var veiculoExistente = snapshot.ConvertTo<Veiculo>();

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

        public async Task<Usuario> CadastrarUsuario(Usuario novoUsuario)
        {
            var usuariosRef = _firestoreDb.Db.Collection("Usuarios");

            var query = await usuariosRef.WhereEqualTo("Email", novoUsuario.Email).GetSnapshotAsync();

            if (query.Documents.Count > 0)
                throw new Exception("Já existe um usuário cadastrado com este e-mail.");

            if (string.IsNullOrWhiteSpace(novoUsuario.Acesso))
                novoUsuario.Acesso = "Usuario";

            int novoId = await GerarProximoId("contador_usuario");
            novoUsuario.Id = novoId.ToString();
            novoUsuario.DataCriacao = DateTime.UtcNow;

            DocumentReference docRef = usuariosRef.Document(novoUsuario.Id);
            await docRef.SetAsync(novoUsuario);

            return novoUsuario;
        }

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

        public async Task<double> CalcularPegadaMediaAsync()
        {
            const string cacheKey = "PegadaMediaCache";

            if (_cache.TryGetValue(cacheKey, out double cachedValue))
                return cachedValue;

            double resultado = await CalcularPegadaMediaInternoAsync();
            _cache.Set(cacheKey, resultado, TimeSpan.FromMinutes(5));
            return resultado;
        }

        private async Task<double> CalcularPegadaMediaInternoAsync()
        {
            try
            {
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

                var componentes = await ListarVeiculoComponente();
                if (componentes == null || componentes.Count == 0)
                    return 0;

                const double FatorEmissaoPadrao = 2.5;
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
                return 0;
            }
        }

        public async Task<GraficoEmissoesDto> ObterDadosGraficoAsync()
        {
            var resultado = new GraficoEmissoesDto();

            var veiculos = await ListarVeiculo();
            if (veiculos == null || !veiculos.Any())
            {
                return ObterDadosExemplo();
            }

            var componentes = await ListarVeiculoComponente();
            var dictComponentesPorVin = componentes?
                .GroupBy(c => c.fk_Veiculo_Vin)
                .ToDictionary(g => g.Key, g => g.ToList()) ?? new Dictionary<string, List<VeiculoComponente>>();

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
                    TotalEmissao = g.Sum(x => x.Emissao) / 1000
                })
                .ToList();

            if (!veiculosComData.Any())
            {
                return ObterDadosExemplo();
            }

            resultado.Meses = veiculosComData.Select(x => $"{x.Mes}/{x.Ano}").ToArray();
            resultado.ValoresFabrica = veiculosComData.Select(x => Math.Round(x.TotalEmissao, 1)).ToArray();

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

            resultado.ValoresFabrica = resultado.ValoresFabrica.Select(v => v < 0 ? 0 : v).ToArray();
            resultado.ValoresCadeia = resultado.ValoresCadeia.Select(v => v < 0 ? 0 : v).ToArray();

            return resultado;
        }

        private GraficoEmissoesDto ObterDadosExemplo()
        {
            return new GraficoEmissoesDto
            {
                Meses = new[] { "Jan/2025", "Fev/2025", "Mar/2025", "Abr/2025", "Mai/2025", "Jun/2025" },
                ValoresFabrica = new double[] { 12.5, 15.2, 14.8, 18.5, 20.1, 22.0 },
                ValoresCadeia = new double[] { 8.0, 9.5, 8.8, 12.0, 14.5, 16.0 }
            };
        }

        public async Task<AnalisesESGDto> ObterDadosAnalisesESGAsync()
        {
            var resultado = new AnalisesESGDto();

            var veiculos = await ListarVeiculo();
            var componentes = await ListarVeiculoComponente();
            var fornecedores = await ListarFornecedor();

            const double fatorEmissaoPadrao = 2.5;

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

        public async Task<double> CalcularTotalEmissoesAsync()
        {
            double total = 0;
            const double fatorEmissaoPadrao = 2.5;

            var componentes = await ListarVeiculoComponente();
            if (componentes != null)
            {
                foreach (var comp in componentes)
                {
                    total += comp.PesoKg * fatorEmissaoPadrao;
                }
                _logger.LogInformation($"Total de emissões dos veículos: {total} kg CO₂", "DadosService");
            }

            var lotes = await ListarLoteMateriaPrima();
            if (lotes != null)
            {
                foreach (var lote in lotes)
                {
                    total += lote.QuantidadeKg * lote.PegadaCarbonoPorKg;
                }
                _logger.LogInformation($"Total de emissões dos lotes: {lotes.Sum(l => l.QuantidadeKg * l.PegadaCarbonoPorKg)} kg CO₂", "DadosService");
            }

            _logger.LogInformation($"Total geral de emissões: {total} kg CO₂ ({total / 1000:F2} ton)", "DadosService");
            return total;
        }

        public async Task<double> ObterPrecoCarbonoAsync()
        {
            try
            {
                using var client = new HttpClient();
                client.Timeout = TimeSpan.FromSeconds(10);
                client.DefaultRequestHeaders.Add("User-Agent", "IvecoGreenLedger/1.0");

                var url = "https://api.worldbank.org/v2/country/all/indicator/EN.CLC.CRBT.ZS?format=json";
                var response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(json);

                    var precoUSD = doc.RootElement[1][0]
                                         .GetProperty("value")
                                         .GetDouble();

                    var precoBRL = precoUSD * 5.5;
                    var resultado = Math.Round(precoBRL, 2);

                    _logger.LogInformation($"Preço do carbono (World Bank): USD {precoUSD} → R$ {resultado}/ton");
                    return resultado;
                }
                else
                {
                    _logger.LogWarning($"Falha ao obter preço do World Bank. Status: {response.StatusCode}. Usando fallback.");
                    return 150.0;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Erro na consulta de preço do carbono: {ex.Message}. Usando fallback.");
                return 150.0;
            }
        }
    }
}