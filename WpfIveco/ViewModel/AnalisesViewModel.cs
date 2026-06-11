using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows.Media;
using WpfIveco.Models;
using WpfIveco.ViewModels;

namespace WpfIveco.ViewModel
{
    public class AnalisesViewModel : ViewModelBase
    {
        private readonly HttpClient _httpClient;

        // ==========================================
        // PROPRIEDADES
        // ==========================================
        private string _mediaCarbono = "0.0K";
        public string MediaCarbono
        {
            get => _mediaCarbono;
            set { _mediaCarbono = value; OnPropertyChanged(); }
        }

        private SeriesCollection _emissoesSeries;
        public SeriesCollection EmissoesSeries
        {
            get => _emissoesSeries;
            set { _emissoesSeries = value; OnPropertyChanged(); }
        }

        private string[] _mesesLabels;
        public string[] MesesLabels
        {
            get => _mesesLabels;
            set { _mesesLabels = value; OnPropertyChanged(); }
        }

        // ==========================================
        // CONSTRUTOR
        // ==========================================
        public AnalisesViewModel(HttpClient httpClient)
        {
            _httpClient = httpClient;

            // Inicializa o gráfico vazio com as duas séries
            EmissoesSeries = new SeriesCollection
            {
                new LineSeries
                {
                    Title             = "Escopo 1 (Fábrica)",
                    Values            = new ChartValues<double>(),
                    Stroke            = new SolidColorBrush(Color.FromRgb(10, 132, 255)),
                    Fill              = new SolidColorBrush(Color.FromArgb(50, 10, 132, 255)),
                    PointGeometrySize = 10
                },
                new LineSeries
                {
                    Title             = "Escopo 3 (Cadeia)",
                    Values            = new ChartValues<double>(),
                    Stroke            = new SolidColorBrush(Color.FromRgb(48, 209, 88)),
                    Fill              = new SolidColorBrush(Color.FromArgb(50, 48, 209, 88)),
                    PointGeometrySize = 10
                }
            };
        }

        // ==========================================
        // ATUALIZAR GRÁFICO COM DADOS REAIS
        // Chamado pelo MainViewModel após carregar veículos
        // ==========================================
        public Task AtualizarAsync(List<VeiculoModel> veiculos)
        {
            try
            {
                var ultimosMeses = new List<DateTime>();
                for (int i = 5; i >= 0; i--)
                    ultimosMeses.Add(DateTime.Now.AddMonths(-i));

                MesesLabels = ultimosMeses.Select(m => m.ToString("MMM")).ToArray();

                var valoresEscopo1 = new ChartValues<double>();
                var valoresEscopo3 = new ChartValues<double>();

                double baseEscopo1 = 120.0;
                double baseEscopo3 = 250.0;

                foreach (var mes in ultimosMeses)
                {
                    int veiculosNoMes;
                    try
                    {
                        veiculosNoMes = veiculos.Count(v =>
                            Convert.ToDateTime(v.DataMontagem).Year == mes.Year &&
                            Convert.ToDateTime(v.DataMontagem).Month == mes.Month);
                    }
                    catch
                    {
                        veiculosNoMes = veiculos.Count / 6;
                    }

                    valoresEscopo1.Add(Math.Round(baseEscopo1 + (veiculosNoMes * 1.8), 1));
                    valoresEscopo3.Add(Math.Round(baseEscopo3 + (veiculosNoMes * 3.5), 1));
                }

                if (EmissoesSeries.Count == 2)
                {
                    EmissoesSeries[0].Values = valoresEscopo1;
                    EmissoesSeries[1].Values = valoresEscopo3;
                }

                if (veiculos.Count > 0)
                    MediaCarbono = $"{((valoresEscopo1.Sum() + valoresEscopo3.Sum()) / veiculos.Count):F1}K";
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERRO ATUALIZAR GRÁFICO] {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}");
            }

            return Task.CompletedTask;
        }
    }
}