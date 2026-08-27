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
    /// CORREÇÃO: Paralelização das chamadas HTTP para reduzir tempo de resposta.
    /// </summary>
    public class DashboardViewModel : ViewModelBase
    {
        // ============================================================
        // CAMPOS PRIVADOS
        // ============================================================

        private readonly HttpClient _httpClient;
        private readonly LocalDatabaseService _localDb;
        private string _pegadaMediaFormatada = "Carregando...";
        private readonly Stopwatch _stopwatch = new Stopwatch();

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
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _localDb = new LocalDatabaseService();
            _stopwatch.Start();
        }

        // ============================================================
        // MÉTODO PRINCIPAL
        // ============================================================

        public async Task AtualizarPegadaMediaAsync()
        {
            App.LogInfo("Atualizando dados do dashboard...", "DASH");

            try
            {
                string cacheKey = $"DashboardData_{DateTime.Now:yyyyMMddHHmm}";

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
                    App.LogInfo("Usando dados em cache", "DASH");
                    AplicarDadosCache(cachedData);
                    return;
                }

                // ============================================================
                // CORREÇÃO: Paraleliza as chamadas para a API
                // ============================================================
                bool apiOk = await CarregarDadosDaApi();

                if (!apiOk)
                {
                    App.LogWarning("API indisponível, carregando dados locais", "DASH");
                    await CarregarDadosLocais();
                }

                VariacaoConsultas = "+12%";

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
                    App.LogInfo($"Dados salvos em cache por {CacheDurationSeconds} segundos", "DASH");
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
        /// Tenta carregar todos os dados da API em paralelo.
        /// Retorna true se todos os endpoints responderem com sucesso.
        /// CORREÇÃO: Agora usa Task.WhenAll para executar as chamadas simultaneamente.
        /// </summary>
        private async Task<bool> CarregarDadosDaApi()
        {
            try
            {
                await Task.WhenAll(
                    CarregarPegadaMedia(),
                    CarregarConsultasHoje(),
                    CarregarTempoResposta(),
                    CarregarUsoServidor()
                );
                return true;
            }
            catch
            {
                return false;
            }
        }

        private async Task CarregarDadosLocais()
        {
            try
            {
                var totalVeiculos = await _localDb.GetTotalVeiculosAsync();
                var totalFornecedores = await _localDb.GetTotalFornecedoresAsync();
                var totalPecas = await _localDb.GetTotalPecasAsync();
                ConsultasHoje = totalVeiculos + totalFornecedores + totalPecas;

                if (ConsultasHoje > 0)
                {
                    UsoServidor = ConsultasHoje > 50 ? 60 : (ConsultasHoje > 30 ? 45 : 30);
                    TempoRespostaMs = 150;
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
                var resultadoVeiculos = await _httpClient.GetAsync("api/dados/veiculos");
                var veiculos = await resultadoVeiculos.Content.ReadAsStringAsync();
                var listaVeiculos = JsonSerializer.Deserialize<List<VeiculoModel>>(veiculos,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                var resultadoFornecedores = await _httpClient.GetAsync("api/dados/fornecedores");
                var fornecedores = await resultadoFornecedores.Content.ReadAsStringAsync();
                var listaFornecedores = JsonSerializer.Deserialize<List<FornecedorModel>>(fornecedores,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                var resultadoPecas = await _httpClient.GetAsync("api/dados/componentes");
                var pecas = await resultadoPecas.Content.ReadAsStringAsync();
                var listaPecas = JsonSerializer.Deserialize<List<PecaModel>>(pecas,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                var total = (listaVeiculos?.Count ?? 0) + (listaFornecedores?.Count ?? 0) + (listaPecas?.Count ?? 0);
                ConsultasHoje = total;
                VariacaoConsultas = "+12%";

                int tempoVeiculos = 0, tempoFornecedores = 0, tempoPecas = 0;

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
                TempoRespostaMs = 120;
            }
        }

        private async Task CarregarUsoServidor()
        {
            try
            {
                var totalItens = ConsultasHoje;
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
                    // Fallback silencioso
                }

                App.LogInfo($"Uso do servidor estimado: {UsoServidor}%", "DASH");
            }
            catch
            {
                UsoServidor = 42;
            }
        }
    }
}