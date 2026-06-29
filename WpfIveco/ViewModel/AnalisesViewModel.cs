using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using WpfIveco.DTO;
using WpfIveco.Models;

namespace WpfIveco.ViewModels
{
    /// <summary>
    /// ViewModel para o Dashboard ESG (Análises).
    /// Força a ligação direta ao Firebase Realtime Database independente de como for instanciada.
    /// </summary>
    public class AnalisesViewModel : INotifyPropertyChanged
    {
        private readonly HttpClient _httpClient;
        private const string FirebaseEndpoint = "https://projetopim2semestre-default-rtdb.firebaseio.com/dashboard_esg.json";

        /// ==========================================
        /// 1. PROPRIEDADES DOS CARDS SUPERIORES
        /// ==========================================
        private int _totalEmissoes;
        public int TotalEmissoes
        {
            get => _totalEmissoes;
            set { _totalEmissoes = value; OnPropertyChanged(); }
        }

        private int _fornecedoresVerdes;
        public int FornecedoresVerdes
        {
            get => _fornecedoresVerdes;
            set { _fornecedoresVerdes = value; OnPropertyChanged(); }
        }

        private int _pecasReaproveitadas;
        public int PecasReaproveitadas
        {
            get => _pecasReaproveitadas;
            set { _pecasReaproveitadas = value; OnPropertyChanged(); }
        }

        private string _economiaGerada = "A carregar...";
        public string EconomiaGerada
        {
            get => _economiaGerada;
            set { _economiaGerada = value; OnPropertyChanged(); }
        }

        /// ==========================================
        /// 2. PROPRIEDADES DOS GRÁFICOS (LIVECHARTS)
        /// ==========================================
        private SeriesCollection _emissoesSeries;
        public SeriesCollection EmissoesSeries
        {
            get => _emissoesSeries;
            set { _emissoesSeries = value; OnPropertyChanged(); }
        }

        private string[] _graficoLabels;
        public string[] GraficoLabels
        {
            get => _graficoLabels;
            set { _graficoLabels = value; OnPropertyChanged(); }
        }

        /// ==========================================
        /// 3. COLEÇÕES PARA AS LISTAS DA UI
        /// ==========================================
        private ObservableCollection<FornecedorSustentavel> _fornecedoresSustentaveis = new();
        public ObservableCollection<FornecedorSustentavel> FornecedoresSustentaveis
        {
            get => _fornecedoresSustentaveis;
            set { _fornecedoresSustentaveis = value; OnPropertyChanged(); }
        }

        /// ==========================================
        /// CONSTRUTORES (À Prova de Falhas)
        /// ==========================================

        /// <summary>
        /// Construtor Padrão (Vazio) - Usado se a View criar a ViewModel diretamente.
        /// </summary>
        public AnalisesViewModel()
        {
            // Cria um cliente HTTP próprio caso não seja injetado nenhum externo
            _httpClient = new HttpClient();
            InicializarDashboard();
        }

        /// <summary>
        /// Construtor com Parâmetros - Usado se o MainViewModel injetar o HttpClient global.
        /// </summary>
        public AnalisesViewModel(HttpClient httpClient)
        {
            _httpClient = httpClient ?? new HttpClient();
            InicializarDashboard();
        }

        private void InicializarDashboard()
        {
            EmissoesSeries = new SeriesCollection();
            /// Dispara a carga assíncrona dos dados do Firebase eliminando os mocks antigos
            _ = CarregarDadosFirebaseAsync();
        }

        /// ==========================================
        /// PROCESSO DE CARGA REAL DO FIREBASE
        /// ==========================================
        public async Task CarregarDadosFirebaseAsync()
        {
            try
            {
                /// Faz o pedido GET na API REST do Firebase
                var response = await _httpClient.GetAsync(FirebaseEndpoint);

                if (response.IsSuccessStatusCode)
                {
                    string jsonString = await response.Content.ReadAsStringAsync();

                    if (string.IsNullOrWhiteSpace(jsonString) || jsonString == "null")
                    {
                        EconomiaGerada = "Sem dados no banco";
                        return;
                    }

                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    FirebaseEsgDto dadosReal = JsonSerializer.Deserialize<FirebaseEsgDto>(jsonString, options);

                    if (dadosReal != null)
                    {
                        /// Atualiza os Cards com os valores reais salvos no Firebase
                        TotalEmissoes = dadosReal.TotalEmissoes;
                        FornecedoresVerdes = dadosReal.FornecedoresVerdes;
                        PecasReaproveitadas = dadosReal.PecasReaproveitadas;
                        EconomiaGerada = dadosReal.EconomiaGerada;

                        /// Limpa a lista antiga e insere os fornecedores do banco
                        FornecedoresSustentaveis.Clear();
                        if (dadosReal.TopFornecedores != null)
                        {
                            foreach (var fornecedor in dadosReal.TopFornecedores)
                            {
                                FornecedoresSustentaveis.Add(fornecedor);
                            }
                        }

                        /// Atualiza os gráficos de forma segura na Thread da UI
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            EmissoesSeries = new SeriesCollection
                            {
                                new PieSeries
                                {
                                    Title = "Peças Reaproveitadas",
                                    Values = new ChartValues<int> { dadosReal.PecasReaproveitadas },
                                    DataLabels = true,
                                    Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#10B981"))
                                },
                                new PieSeries
                                {
                                    Title = "Emissões Evitadas (tCO2e)",
                                    Values = new ChartValues<int> { dadosReal.TotalEmissoes },
                                    DataLabels = true,
                                    Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EF4444"))
                                }
                            };

                            GraficoLabels = new[] { "Métricas de Sustentabilidade" };
                        });
                    }
                }
                else
                {
                    EconomiaGerada = "Erro de rede";
                }
            }
            catch (Exception)
            {
                EconomiaGerada = "Indisponível";
            }
        }

        /// ==========================================
        /// NOTIFICAÇÃO DE ALTERAÇÃO DE PROPRIEDADES
        /// ==========================================
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

   
}