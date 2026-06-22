using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Media;
using WpfIveco.DTO;
using WpfIveco.Models;

namespace WpfIveco.ViewModels
{
    public class AnalisesViewModel : ViewModelBase
    {
        private readonly HttpClient _httpClient;

        // Gráfico YTD
        private SeriesCollection _emissoesSeries;
        private string[] _mesesLabels;

        public SeriesCollection EmissoesSeries
        {
            get => _emissoesSeries;
            set { _emissoesSeries = value; OnPropertyChanged(); }
        }

        public string[] MesesLabels
        {
            get => _mesesLabels;
            set { _mesesLabels = value; OnPropertyChanged(); }
        }

        public Func<double, string> Formatter => value => $"{value:N1} t";

        // Gráfico de pizza
        private SeriesCollection _distribuicaoSeries;
        public SeriesCollection DistribuicaoSeries
        {
            get => _distribuicaoSeries;
            set { _distribuicaoSeries = value; OnPropertyChanged(); }
        }

        // Top Fornecedores
        private List<FornecedorVerdeDto> _topFornecedores;
        public List<FornecedorVerdeDto> TopFornecedores
        {
            get => _topFornecedores;
            set { _topFornecedores = value; OnPropertyChanged(); }
        }

        public AnalisesViewModel()
        {
            // Inicializa com coleções vazias (sem dados de exemplo)
            InicializarVazio();
        }

        public AnalisesViewModel(HttpClient httpClient) : this()
        {
            _httpClient = httpClient;
        }

        /// <summary>
        /// Inicializa os gráficos com coleções vazias (sem dados de exemplo).
        /// </summary>
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

        public async Task AtualizarAsync()
        {
            // 1. Gráfico YTD – dados reais
            try
            {
                var response = await _httpClient.GetAsync("api/dados/grafico-emissoes");
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
                    }
                    else
                    {
                        // Dados reais vieram vazios – mantém coleções vazias
                        MesesLabels = Array.Empty<string>();
                        EmissoesSeries[0].Values = new ChartValues<double>();
                        EmissoesSeries[1].Values = new ChartValues<double>();
                        Debug.WriteLine("[Atualizar] YTD vazio (sem dados reais).");
                    }
                }
                else
                {
                    Debug.WriteLine($"[Atualizar] YTD HTTP {(int)response.StatusCode} – sem dados.");
                    // Mantém vazio
                    MesesLabels = Array.Empty<string>();
                    EmissoesSeries[0].Values = new ChartValues<double>();
                    EmissoesSeries[1].Values = new ChartValues<double>();
                }
            }
            catch
            {
                Debug.WriteLine("[Atualizar] YTD erro – mantendo vazio.");
                MesesLabels = Array.Empty<string>();
                EmissoesSeries[0].Values = new ChartValues<double>();
                EmissoesSeries[1].Values = new ChartValues<double>();
            }

            // 2. Dados ESG – reais
            try
            {
                var responseEsg = await _httpClient.GetAsync("api/dados/analises-esg");
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
                    }
                    else
                    {
                        // Sem dados reais – coleção vazia
                        DistribuicaoSeries = new SeriesCollection();
                        Debug.WriteLine("[Atualizar] Distribuição vazia (sem dados reais).");
                    }

                    if (dadosEsg?.TopFornecedoresVerdes != null && dadosEsg.TopFornecedoresVerdes.Any())
                    {
                        TopFornecedores = dadosEsg.TopFornecedoresVerdes;
                    }
                    else
                    {
                        TopFornecedores = new List<FornecedorVerdeDto>();
                        Debug.WriteLine("[Atualizar] Nenhum fornecedor verde (sem dados reais).");
                    }
                }
                else
                {
                    Debug.WriteLine($"[Atualizar] ESG HTTP {(int)responseEsg.StatusCode} – sem dados.");
                    DistribuicaoSeries = new SeriesCollection();
                    TopFornecedores = new List<FornecedorVerdeDto>();
                }
            }
            catch
            {
                Debug.WriteLine("[Atualizar] ESG erro – mantendo vazio.");
                DistribuicaoSeries = new SeriesCollection();
                TopFornecedores = new List<FornecedorVerdeDto>();
            }
        }
    }
}