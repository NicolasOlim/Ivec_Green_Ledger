using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Media;
using WpfIveco.DTO;
using WpfIveco.Models;

namespace WpfIveco.ViewModels
{
    /// <summary>
    /// ViewModel para a página de Análise ESG.
    /// Gerencia gráficos YTD, distribuição de emissões e ranking de fornecedores verdes.
    /// </summary>
    public class AnalisesViewModel : ViewModelBase
    {
        private readonly HttpClient _httpClient;

        // ============================================================
        // GRÁFICO YTD (Emissões por mês)
        // ============================================================

        private SeriesCollection _emissoesSeries;
        private string[] _mesesLabels;

        /// <summary>Séries do gráfico YTD (Processo Fabril e Cadeia de Fornecedores).</summary>
        public SeriesCollection EmissoesSeries
        {
            get => _emissoesSeries;
            set { _emissoesSeries = value; OnPropertyChanged(); }
        }

        /// <summary>Rótulos dos meses no gráfico YTD.</summary>
        public string[] MesesLabels
        {
            get => _mesesLabels;
            set { _mesesLabels = value; OnPropertyChanged(); }
        }

        /// <summary>Formatador do eixo Y para exibir valores em toneladas.</summary>
        public Func<double, string> Formatter => value => $"{value:N1} t";

        // ============================================================
        // GRÁFICO DE PIZZA (Distribuição de Emissões por Escopo)
        // ============================================================

        private SeriesCollection _distribuicaoSeries;

        /// <summary>Séries do gráfico de pizza (Escopo 1, 2, 3).</summary>
        public SeriesCollection DistribuicaoSeries
        {
            get => _distribuicaoSeries;
            set { _distribuicaoSeries = value; OnPropertyChanged(); }
        }

        // ============================================================
        // RANKING DE FORNECEDORES VERDES
        // ============================================================

        private List<FornecedorVerdeDto> _topFornecedores;

        /// <summary>Lista dos top 10 fornecedores verdes.</summary>
        public List<FornecedorVerdeDto> TopFornecedores
        {
            get => _topFornecedores;
            set { _topFornecedores = value; OnPropertyChanged(); }
        }

        // ============================================================
        // CONSTRUTORES
        // ============================================================

        /// <summary>Construtor padrão, inicializa com dados vazios.</summary>
        public AnalisesViewModel()
        {
            App.LogInfo("Construtor padrão – inicializando vazio", "ANALISES");
            InicializarVazio();
        }

        /// <summary>Construtor com HttpClient para comunicação com a API.</summary>
        public AnalisesViewModel(HttpClient httpClient) : this()
        {
            _httpClient = httpClient;
            App.LogInfo("Construtor com HttpClient", "ANALISES");
        }

        // ============================================================
        // MÉTODOS PRIVADOS
        // ============================================================

        /// <summary>Inicializa os gráficos com coleções vazias (sem dados de exemplo).</summary>
        private void InicializarVazio()
        {
            MesesLabels = Array.Empty<string>();
            EmissoesSeries = new SeriesCollection
            {
                new ColumnSeries
                {
                    Title = "Processo Fabril Iveco",
                    Values = new ChartValues<double>(),
                    Fill = new SolidColorBrush(Color.FromRgb(0, 120, 200)),
                    DataLabels = true,
                    LabelPoint = point => $"{point.Y:N1} t"
                },
                new ColumnSeries
                {
                    Title = "Cadeia de Fornecedores",
                    Values = new ChartValues<double>(),
                    Fill = new SolidColorBrush(Color.FromRgb(255, 150, 50)),
                    DataLabels = true,
                    LabelPoint = point => $"{point.Y:N1} t"
                }
            };
            DistribuicaoSeries = new SeriesCollection();
            TopFornecedores = new List<FornecedorVerdeDto>();
        }

        // ============================================================
        // MÉTODO PÚBLICO DE ATUALIZAÇÃO
        // ============================================================

        /// <summary>
        /// Atualiza todos os dados da página: gráfico YTD e dados ESG.
        /// Consome as APIs /grafico-emissoes e /analises-esg.
        /// </summary>
        public async Task AtualizarAsync()
        {
            App.LogInfo("AtualizarAsync iniciado", "ANALISES");

            // 1. Gráfico YTD
            try
            {
                App.LogInfo("Buscando dados do gráfico YTD...", "ANALISES");
                var response = await _httpClient.GetAsync("api/dados/grafico-emissoes");
                App.LogInfo($"GET YTD → {(int)response.StatusCode}", "ANALISES");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var dados = JsonSerializer.Deserialize<GraficoEmissoesDto>(json,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (dados != null && dados.Meses != null && dados.Meses.Length > 0)
                    {
                        MesesLabels = dados.Meses;
                        EmissoesSeries[0].Values = new ChartValues<double>(dados.ValoresFabrica);
                        EmissoesSeries[1].Values = new ChartValues<double>(dados.ValoresCadeia);
                        App.LogInfo($"YTD atualizado com {dados.Meses.Length} meses", "ANALISES");
                    }
                    else
                    {
                        App.LogWarning("YTD vazio (sem dados).", "ANALISES");
                        MesesLabels = Array.Empty<string>();
                        EmissoesSeries[0].Values = new ChartValues<double>();
                        EmissoesSeries[1].Values = new ChartValues<double>();
                    }
                }
                else
                {
                    App.LogError($"Falha YTD: HTTP {response.StatusCode}", "ANALISES");
                    MesesLabels = Array.Empty<string>();
                    EmissoesSeries[0].Values = new ChartValues<double>();
                    EmissoesSeries[1].Values = new ChartValues<double>();
                }
            }
            catch
            {
                App.LogError("Erro ao carregar gráfico YTD – usando valores vazios", "ANALISES");
                MesesLabels = Array.Empty<string>();
                EmissoesSeries[0].Values = new ChartValues<double>();
                EmissoesSeries[1].Values = new ChartValues<double>();
            }

            // 2. Dados ESG
            try
            {
                App.LogInfo("Buscando dados ESG...", "ANALISES");
                var responseEsg = await _httpClient.GetAsync("api/dados/analises-esg");
                App.LogInfo($"GET ESG → {(int)responseEsg.StatusCode}", "ANALISES");

                if (responseEsg.IsSuccessStatusCode)
                {
                    var jsonEsg = await responseEsg.Content.ReadAsStringAsync();
                    var dadosEsg = JsonSerializer.Deserialize<AnalisesESGDto>(jsonEsg,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (dadosEsg != null && dadosEsg.DistribuicaoEmissoes != null && dadosEsg.DistribuicaoEmissoes.Any())
                    {
                        var series = new SeriesCollection();
                        foreach (var item in dadosEsg.DistribuicaoEmissoes)
                        {
                            var cor = item.Escopo.Contains("Escopo 1") ? Color.FromRgb(0, 120, 200) :
                                      item.Escopo.Contains("Escopo 2") ? Color.FromRgb(100, 200, 100) :
                                      Color.FromRgb(255, 150, 50);
                            series.Add(new PieSeries
                            {
                                Title = item.Escopo,
                                Values = new ChartValues<double> { item.Porcentagem },
                                Fill = new SolidColorBrush(cor),
                                DataLabels = true,
                                LabelPoint = point => $"{point.Y}%"
                            });
                        }
                        DistribuicaoSeries = series;
                        App.LogInfo($"Distribuição atualizada com {series.Count} escopos", "ANALISES");
                    }
                    else
                    {
                        App.LogWarning("Distribuição vazia (sem dados).", "ANALISES");
                        DistribuicaoSeries = new SeriesCollection();
                    }

                    if (dadosEsg?.TopFornecedoresVerdes != null && dadosEsg.TopFornecedoresVerdes.Any())
                    {
                        TopFornecedores = dadosEsg.TopFornecedoresVerdes;
                        App.LogInfo($"TopFornecedores atualizado com {TopFornecedores.Count} fornecedores", "ANALISES");
                    }
                    else
                    {
                        App.LogWarning("Nenhum fornecedor verde.", "ANALISES");
                        TopFornecedores = new List<FornecedorVerdeDto>();
                    }
                }
                else
                {
                    App.LogError($"Falha ESG: HTTP {responseEsg.StatusCode}", "ANALISES");
                    DistribuicaoSeries = new SeriesCollection();
                    TopFornecedores = new List<FornecedorVerdeDto>();
                }
            }
            catch
            {
                App.LogError("Erro ao carregar dados ESG – usando valores vazios", "ANALISES");
                DistribuicaoSeries = new SeriesCollection();
                TopFornecedores = new List<FornecedorVerdeDto>();
            }

            App.LogInfo("AtualizarAsync concluído", "ANALISES");
        }
    }
}