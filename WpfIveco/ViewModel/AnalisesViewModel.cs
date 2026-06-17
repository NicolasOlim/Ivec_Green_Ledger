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

        public AnalisesViewModel()
        {
            InicializarDadosExemplo();
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

        public async Task AtualizarAsync(List<VeiculoModel> veiculos)
        {
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
                        return;
                    }
                }

                // Fallback
                InicializarDadosExemplo();
            }
            catch
            {
                InicializarDadosExemplo();
            }
        }

        public class GraficoEmissoesDto
        {
            public string[] Meses { get; set; }
            public double[] ValoresFabrica { get; set; }
            public double[] ValoresCadeia { get; set; }
        }
    }
}