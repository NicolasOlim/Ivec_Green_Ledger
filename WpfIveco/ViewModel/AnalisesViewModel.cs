using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Media;
using WpfIveco.Models;

namespace WpfIveco.ViewModels
{
    public class AnalisesViewModel : ViewModelBase
    {
        private readonly HttpClient _httpClient;

        // Dados do gráfico de barras (Já existentes)
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

        // Dados do gráfico de pizza (Distribuição de Emissões)
        private SeriesCollection _distribuicaoSeries;
        public SeriesCollection DistribuicaoSeries
        {
            get => _distribuicaoSeries;
            set { _distribuicaoSeries = value; OnPropertyChanged(); }
        }

        // Top Fornecedores Verdes
        private List<FornecedorVerdeDto> _topFornecedores;
        public List<FornecedorVerdeDto> TopFornecedores
        {
            get => _topFornecedores;
            set { _topFornecedores = value; OnPropertyChanged(); }
        }

        public AnalisesViewModel()
        {
            InicializarDadosExemplo();
            InicializarDadosESGExemplo();
        }

        public AnalisesViewModel(HttpClient httpClient) : this()
        {
            _httpClient = httpClient;
        }

        private void InicializarDadosExemplo()
        {
            MesesLabels = new[] { "Jan/2025", "Fev/2025", "Mar/2025", "Abr/2025", "Mai/2025", "Jun/2025" };
            EmissoesSeries = new SeriesCollection
            {
                new ColumnSeries
                {
                    Title = "Processo Fabril Iveco",
                    Values = new ChartValues<double> { 12.5, 15.2, 14.8, 18.5, 20.1, 22.0 },
                    Fill = new SolidColorBrush(Color.FromRgb(0, 120, 200)),
                    DataLabels = true,
                    LabelPoint = point => $"{point.Y:N1} t"
                },
                new ColumnSeries
                {
                    Title = "Cadeia de Fornecedores",
                    Values = new ChartValues<double> { 8.0, 9.5, 8.8, 12.0, 14.5, 16.0 },
                    Fill = new SolidColorBrush(Color.FromRgb(255, 150, 50)),
                    DataLabels = true,
                    LabelPoint = point => $"{point.Y:N1} t"
                }
            };
        }

        private void InicializarDadosESGExemplo()
        {
            DistribuicaoSeries = new SeriesCollection
    {
        new PieSeries
        {
            Title = "Escopo 1 (Fábrica)",
            Values = new ChartValues<double> { 0 },
            Fill = new SolidColorBrush(Color.FromRgb(0, 120, 200)),
            DataLabels = true,
            LabelPoint = point => $"{point.Y}%"
        },
        new PieSeries
        {
            Title = "Escopo 2 (Energia)",
            Values = new ChartValues<double> { 0 },
            Fill = new SolidColorBrush(Color.FromRgb(100, 200, 100)),
            DataLabels = true,
            LabelPoint = point => $"{point.Y}%"
        },
        new PieSeries
        {
            Title = "Escopo 3 (Fornecedores)",
            Values = new ChartValues<double> { 0 },
            Fill = new SolidColorBrush(Color.FromRgb(255, 150, 50)),
            DataLabels = true,
            LabelPoint = point => $"{point.Y}%"
        }
    };

            TopFornecedores = new List<FornecedorVerdeDto>();
        }

        public async Task AtualizarAsync(List<VeiculoModel> veiculos)
        {
            // ============================================================
            // 1. ATUALIZAR GRÁFICO DE BARRAS (YTD)
            // ============================================================
            try
            {
                var response = await _httpClient.GetAsync("api/dados/grafico-emissoes");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var dados = JsonSerializer.Deserialize<GraficoEmissoesDto>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    if (dados != null && dados.Meses != null && dados.Meses.Length > 0)
                    {
                        MesesLabels = dados.Meses;
                        EmissoesSeries[0].Values = new ChartValues<double>(dados.ValoresFabrica);
                        EmissoesSeries[1].Values = new ChartValues<double>(dados.ValoresCadeia);
                    }
                }
            }
            catch { /* mantém dados de exemplo */ }

            // ============================================================
            // 2. ATUALIZAR GRÁFICO DE PIZZA (DISTRIBUIÇÃO DE EMISSÕES)
            // ============================================================
            try
            {
                var responseEsg = await _httpClient.GetAsync("api/dados/analises-esg");
                if (responseEsg.IsSuccessStatusCode)
                {
                    var jsonEsg = await responseEsg.Content.ReadAsStringAsync();
                    var dadosEsg = JsonSerializer.Deserialize<AnalisesESGDto>(jsonEsg, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (dadosEsg != null && dadosEsg.DistribuicaoEmissoes != null && dadosEsg.DistribuicaoEmissoes.Any())
                    {
                        // Atualiza gráfico de pizza com dados reais (mesmo que sejam 0%)
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
                        // Se a API retornar lista vazia, usar dados de exemplo
                        InicializarDadosESGExemplo();
                    }

                    // Atualiza top fornecedores
                    if (dadosEsg?.TopFornecedoresVerdes != null && dadosEsg.TopFornecedoresVerdes.Any())
                    {
                        TopFornecedores = dadosEsg.TopFornecedoresVerdes;
                    }
                    else
                    {
                        // Se não houver fornecedores, manter lista vazia (ou dados de exemplo)
                        TopFornecedores = new List<FornecedorVerdeDto>();
                        // Opcional: se quiser mostrar exemplos, use InicializarDadosESGExemplo();
                    }
                }
            }
            catch
            {
                // Em caso de falha, mantém dados de exemplo
                InicializarDadosESGExemplo();
            }
        }

        public class GraficoEmissoesDto
        {
            public string[] Meses { get; set; }
            public double[] ValoresFabrica { get; set; }
            public double[] ValoresCadeia { get; set; }
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