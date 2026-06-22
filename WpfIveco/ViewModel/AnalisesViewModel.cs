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
            Debug.WriteLine("[ANALISES] Construtor padrão – inicializando vazio.");
            InicializarVazio();
        }

        public AnalisesViewModel(HttpClient httpClient) : this()
        {
            _httpClient = httpClient;
            Debug.WriteLine("[ANALISES] Construtor com HttpClient.");
        }

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
            Debug.WriteLine("[ANALISES] AtualizarAsync iniciado.");

            // 1. Gráfico YTD
            try
            {
                Debug.WriteLine("[ANALISES] Buscando dados do gráfico YTD...");
                var response = await _httpClient.GetAsync("api/dados/grafico-emissoes");
                Debug.WriteLine($"[ANALISES] GET YTD → {(int)response.StatusCode}");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine("[ANALISES] JSON YTD recebido.");
                    var dados = JsonSerializer.Deserialize<GraficoEmissoesDto>(json,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (dados != null && dados.Meses != null && dados.Meses.Length > 0)
                    {
                        MesesLabels = dados.Meses;
                        EmissoesSeries[0].Values = new ChartValues<double>(dados.ValoresFabrica);
                        EmissoesSeries[1].Values = new ChartValues<double>(dados.ValoresCadeia);
                        Debug.WriteLine($"[ANALISES] YTD atualizado com {dados.Meses.Length} meses.");
                    }
                    else
                    {
                        Debug.WriteLine("[ANALISES] YTD vazio (sem dados).");
                        MesesLabels = Array.Empty<string>();
                        EmissoesSeries[0].Values = new ChartValues<double>();
                        EmissoesSeries[1].Values = new ChartValues<double>();
                    }
                }
                else
                {
                    Debug.WriteLine($"[ANALISES] Falha YTD: HTTP {response.StatusCode}");
                    MesesLabels = Array.Empty<string>();
                    EmissoesSeries[0].Values = new ChartValues<double>();
                    EmissoesSeries[1].Values = new ChartValues<double>();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ANALISES] ERRO YTD: {ex.GetType().Name} – {ex.Message}");
                Debug.WriteLine(ex.StackTrace);
                MesesLabels = Array.Empty<string>();
                EmissoesSeries[0].Values = new ChartValues<double>();
                EmissoesSeries[1].Values = new ChartValues<double>();
            }

            // 2. Dados ESG
            try
            {
                Debug.WriteLine("[ANALISES] Buscando dados ESG...");
                var responseEsg = await _httpClient.GetAsync("api/dados/analises-esg");
                Debug.WriteLine($"[ANALISES] GET ESG → {(int)responseEsg.StatusCode}");

                if (responseEsg.IsSuccessStatusCode)
                {
                    var jsonEsg = await responseEsg.Content.ReadAsStringAsync();
                    Debug.WriteLine("[ANALISES] JSON ESG recebido.");
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
                        Debug.WriteLine($"[ANALISES] Distribuição atualizada com {series.Count} escopos.");
                    }
                    else
                    {
                        Debug.WriteLine("[ANALISES] Distribuição vazia (sem dados).");
                        DistribuicaoSeries = new SeriesCollection();
                    }

                    if (dadosEsg?.TopFornecedoresVerdes != null && dadosEsg.TopFornecedoresVerdes.Any())
                    {
                        TopFornecedores = dadosEsg.TopFornecedoresVerdes;
                        Debug.WriteLine($"[ANALISES] TopFornecedores atualizado com {TopFornecedores.Count} fornecedores.");
                    }
                    else
                    {
                        Debug.WriteLine("[ANALISES] Nenhum fornecedor verde.");
                        TopFornecedores = new List<FornecedorVerdeDto>();
                    }
                }
                else
                {
                    Debug.WriteLine($"[ANALISES] Falha ESG: HTTP {responseEsg.StatusCode}");
                    DistribuicaoSeries = new SeriesCollection();
                    TopFornecedores = new List<FornecedorVerdeDto>();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ANALISES] ERRO ESG: {ex.GetType().Name} – {ex.Message}");
                Debug.WriteLine(ex.StackTrace);
                DistribuicaoSeries = new SeriesCollection();
                TopFornecedores = new List<FornecedorVerdeDto>();
            }

            Debug.WriteLine("[ANALISES] AtualizarAsync concluído.");
        }
    }
}