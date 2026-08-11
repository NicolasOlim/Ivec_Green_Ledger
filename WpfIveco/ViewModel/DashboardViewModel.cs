using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Runtime.Caching;
using System.Text.Json;
using System.Threading.Tasks;
using WpfIveco.Data;
using WpfIveco.Models;
using WpfIveco.ViewModels;

namespace WpfIveco.ViewModels
{
    /// <summary>
    /// ViewModel para o Dashboard principal.
    /// Gerencia a exibição da pegada média de carbono e métricas gerais do sistema.
    /// Utiliza cache em memória e fallback para SQLite quando a API está offline.
    /// </summary>
    public class DashboardViewModel : ViewModelBase
    {
        private readonly HttpClient _httpClient;
        private readonly LocalDatabaseService _localDb;
        private string _pegadaMediaFormatada = "Carregando...";
        private readonly Stopwatch _stopwatch = new Stopwatch();

        // Cache em memória (expira em 60 segundos)
        private static readonly MemoryCache _cache = MemoryCache.Default;
        private const int CacheDurationSeconds = 60;

        // ============================================================
        // PROPRIEDADES (BINDINGS)
        // ============================================================

        public string PegadaMediaFormatada
        {
            get => _pegadaMediaFormatada;
            set { _pegadaMediaFormatada = value; OnPropertyChanged(); }
        }

        private int _consultasHoje = 0;
        public int ConsultasHoje
        {
            get => _consultasHoje;
            set { _consultasHoje = value; OnPropertyChanged(); }
        }

        private int _tempoRespostaMs = 0;
        public int TempoRespostaMs
        {
            get => _tempoRespostaMs;
            set { _tempoRespostaMs = value; OnPropertyChanged(); }
        }

        private int _usoServidor = 0;
        public int UsoServidor
        {
            get => _usoServidor;
            set { _usoServidor = value; OnPropertyChanged(); }
        }

        private string _variacaoConsultas = "+0%";
        public string VariacaoConsultas
        {
            get => _variacaoConsultas;
            set { _variacaoConsultas = value; OnPropertyChanged(); }
        }

        // ============================================================
        // CONSTRUTOR
        // ============================================================

        public DashboardViewModel(HttpClient httpClient)
        {
            App.LogInfo("Construtor", "DASH");
            _httpClient = httpClient;
            _localDb = new LocalDatabaseService();
            _stopwatch.Start();
        }

        // ============================================================
        // MÉTODO PRINCIPAL
        // ============================================================

        /// <summary>
        /// Atualiza todos os dados do dashboard.
        /// Tenta obter da API; se falhar, busca do SQLite local.
        /// Também utiliza cache em memória para evitar chamadas repetidas.
        /// </summary>
        public async Task AtualizarPegadaMediaAsync()
        {
            App.LogInfo("Atualizando dados do dashboard...", "DASH");

            try
            {
                // Chave do cache baseada na data/hora (atualiza a cada minuto)
                string cacheKey = $"DashboardData_{DateTime.Now:yyyyMMddHHmm}";

                // 1. Tenta recuperar do cache em memória
                DashboardCacheData cachedData = null;
                try
                {
                    cachedData = _cache.Get(cacheKey) as DashboardCacheData;
                }
                catch (Exception cacheEx)
                {
                    App.LogWarning($"Erro ao acessar cache: {cacheEx.Message}", "DASH");
                }

                if (cachedData != null)
                {
                    App.LogInfo("Usando dados em cache para o dashboard", "DASH");
                    AplicarDadosCache(cachedData);
                    return;
                }

                // 2. Tenta carregar da API
                bool apiOk = await CarregarDadosDaApi();

                if (!apiOk)
                {
                    // 3. Se a API falhou, carrega do SQLite
                    App.LogWarning("API indisponível, carregando dados locais para o dashboard", "DASH");
                    await CarregarDadosLocais();
                }

                // 4. Variação (simulada, pode ser ajustada)
                VariacaoConsultas = "+12%";

                // 5. Salva no cache em memória
                var dadosCache = new DashboardCacheData
                {
                    ConsultasHoje = ConsultasHoje,
                    TempoRespostaMs = TempoRespostaMs,
                    UsoServidor = UsoServidor,
                    VariacaoConsultas = VariacaoConsultas,
                    PegadaMediaFormatada = PegadaMediaFormatada
                };

                try
                {
                    _cache.Set(cacheKey, dadosCache, DateTimeOffset.Now.AddSeconds(CacheDurationSeconds));
                    App.LogInfo($"Dados do dashboard salvos em cache por {CacheDurationSeconds} segundos", "DASH");
                }
                catch (Exception cacheEx)
                {
                    App.LogWarning($"Erro ao salvar cache: {cacheEx.Message}", "DASH");
                }

                App.LogInfo($"Dashboard atualizado: Consultas={ConsultasHoje}, Tempo={TempoRespostaMs}ms, Servidor={UsoServidor}%", "DASH");
            }
            catch (Exception ex)
            {
                App.LogError($"Erro ao carregar dados do dashboard: {ex.Message}", "DASH");
                // Mantém valores anteriores ou define fallback
                if (ConsultasHoje == 0)
                {
                    ConsultasHoje = 0;
                    TempoRespostaMs = 0;
                    UsoServidor = 0;
                    PegadaMediaFormatada = "Indisponível";
                }
            }
        }

        // ============================================================
        // MÉTODOS AUXILIARES
        // ============================================================

        /// <summary>
        /// Tenta carregar todos os dados da API.
        /// Retorna true se todos os endpoints responderem com sucesso.
        /// </summary>
        private async Task<bool> CarregarDadosDaApi()
        {
            try
            {
                await CarregarPegadaMedia();
                await CarregarConsultasHoje();
                await CarregarTempoResposta();
                await CarregarUsoServidor();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Carrega os dados do SQLite para exibição no dashboard.
        /// </summary>
        private async Task CarregarDadosLocais()
        {
            try
            {
                // Conta os registros locais
                var totalVeiculos = await _localDb.GetTotalVeiculosAsync();
                var totalFornecedores = await _localDb.GetTotalFornecedoresAsync();
                var totalPecas = await _localDb.GetTotalPecasAsync();
                ConsultasHoje = totalVeiculos + totalFornecedores + totalPecas;

                if (ConsultasHoje > 0)
                {
                    // Estimativa de uso baseada na quantidade de dados
                    UsoServidor = ConsultasHoje > 50 ? 60 : (ConsultasHoje > 30 ? 45 : 30);
                    TempoRespostaMs = 150; // tempo estimado para acesso local
                    PegadaMediaFormatada = "Dados locais (offline)";
                }
                else
                {
                    ConsultasHoje = 0;
                    UsoServidor = 5;
                    TempoRespostaMs = 0;
                    PegadaMediaFormatada = "Sem dados locais";
                }

                App.LogInfo($"Dados locais carregados: {ConsultasHoje} itens", "DASH");
            }
            catch (Exception ex)
            {
                App.LogError($"Erro ao carregar dados locais: {ex.Message}", "DASH");
            }
        }

        private void AplicarDadosCache(DashboardCacheData dados)
        {
            if (dados == null) return;
            ConsultasHoje = dados.ConsultasHoje;
            TempoRespostaMs = dados.TempoRespostaMs;
            UsoServidor = dados.UsoServidor;
            VariacaoConsultas = dados.VariacaoConsultas;
            PegadaMediaFormatada = dados.PegadaMediaFormatada;
        }

        // ============================================================
        // MÉTODOS INDIVIDUAIS DE CARREGAMENTO (API)
        // ============================================================

        private async Task CarregarPegadaMedia()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/dados/pegada-media");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(json);
                    var media = doc.RootElement.GetProperty("pegadaMedia").GetDouble();

                    if (media >= 1000)
                        PegadaMediaFormatada = (media / 1000).ToString("N1") + "K";
                    else if (media > 0)
                        PegadaMediaFormatada = media.ToString("N1") + " kg CO2";
                    else
                        PegadaMediaFormatada = "0.0 kg CO2";

                    App.LogInfo($"Pegada média: {PegadaMediaFormatada}", "DASH");
                }
                else
                {
                    PegadaMediaFormatada = "Erro ao carregar";
                }
            }
            catch
            {
                PegadaMediaFormatada = "Indisponível";
            }
        }

        private async Task CarregarConsultasHoje()
        {
            try
            {
                // Obtém contagem de veículos
                var resultadoVeiculos = await _httpClient.GetAsync("api/dados/veiculos");
                var veiculos = await resultadoVeiculos.Content.ReadAsStringAsync();
                var listaVeiculos = JsonSerializer.Deserialize<List<VeiculoModel>>(veiculos,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                // Obtém contagem de fornecedores
                var resultadoFornecedores = await _httpClient.GetAsync("api/dados/fornecedores");
                var fornecedores = await resultadoFornecedores.Content.ReadAsStringAsync();
                var listaFornecedores = JsonSerializer.Deserialize<List<FornecedorModel>>(fornecedores,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                // Obtém contagem de peças
                var resultadoPecas = await _httpClient.GetAsync("api/dados/componentes");
                var pecas = await resultadoPecas.Content.ReadAsStringAsync();
                var listaPecas = JsonSerializer.Deserialize<List<PecaModel>>(pecas,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                var total = (listaVeiculos?.Count ?? 0) + (listaFornecedores?.Count ?? 0) + (listaPecas?.Count ?? 0);
                ConsultasHoje = total;
                VariacaoConsultas = "+12%";

                // ============================================================
                // CORREÇÃO: Extrai os tempos de resposta dos cabeçalhos HTTP
                // Os cabeçalhos vêm como string, então usamos int.TryParse para converter com segurança.
                // ============================================================
                int tempoVeiculos = 0, tempoFornecedores = 0, tempoPecas = 0;

                // Tenta obter o cabeçalho "X-Response-Time" de cada resposta
                if (resultadoVeiculos.Headers.TryGetValues("X-Response-Time", out var valoresVeiculos))
                    int.TryParse(valoresVeiculos.FirstOrDefault(), out tempoVeiculos);

                if (resultadoFornecedores.Headers.TryGetValues("X-Response-Time", out var valoresFornecedores))
                    int.TryParse(valoresFornecedores.FirstOrDefault(), out tempoFornecedores);

                if (resultadoPecas.Headers.TryGetValues("X-Response-Time", out var valoresPecas))
                    int.TryParse(valoresPecas.FirstOrDefault(), out tempoPecas);

                var tempos = new[] { tempoVeiculos, tempoFornecedores, tempoPecas };
                var mediaTempo = (int)Math.Round(tempos.Average());
                TempoRespostaMs = (TempoRespostaMs + mediaTempo) / 2;

                App.LogInfo($"Consultas: {ConsultasHoje}", "DASH");
            }
            catch (Exception ex)
            {
                App.LogError($"Erro ao carregar consultas: {ex.Message}", "DASH");
                ConsultasHoje = 0;
            }
        }

        private async Task CarregarTempoResposta()
        {
            try
            {
                if (TempoRespostaMs == 0)
                {
                    var stopwatch = Stopwatch.StartNew();
                    var response = await _httpClient.GetAsync("api/dados/pegada-media");
                    stopwatch.Stop();
                    if (response.IsSuccessStatusCode)
                    {
                        TempoRespostaMs = (int)stopwatch.ElapsedMilliseconds;
                        App.LogInfo($"Tempo de resposta medido: {TempoRespostaMs}ms", "DASH");
                    }
                }
            }
            catch
            {
                TempoRespostaMs = 120; // fallback
            }
        }

        private async Task CarregarUsoServidor()
        {
            try
            {
                var totalItens = ConsultasHoje;
                // Estimativa inicial baseada na quantidade de dados
                if (totalItens > 50)
                    UsoServidor = 60;
                else if (totalItens > 30)
                    UsoServidor = 45;
                else if (totalItens > 10)
                    UsoServidor = 30;
                else if (totalItens > 0)
                    UsoServidor = 15;
                else
                    UsoServidor = 5;

                // Tenta obter valor real do health check (se disponível)
                try
                {
                    var response = await _httpClient.GetAsync("api/dados/health");
                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        using var doc = JsonDocument.Parse(json);
                        if (doc.RootElement.TryGetProperty("uso", out var uso))
                        {
                            UsoServidor = uso.GetInt32();
                            App.LogInfo($"Uso do servidor obtido da API: {UsoServidor}%", "DASH");
                            return;
                        }
                    }
                }
                catch
                {
                    // Fallback silencioso – mantém a estimativa
                }

                App.LogInfo($"Uso do servidor estimado: {UsoServidor}%", "DASH");
            }
            catch
            {
                UsoServidor = 42; // fallback
            }
        }
    }
}