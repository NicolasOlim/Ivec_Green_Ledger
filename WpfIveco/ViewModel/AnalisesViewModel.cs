using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Media;
using WpfIveco.DTO;
using WpfIveco.Models;
using WpfIveco.ViewModels;

namespace WpfIveco.ViewModels
{
    /// <summary>
    /// ViewModel para o Dashboard ESG (Análises).
    /// Consome os endpoints reais da API: pegada-media, grafico-emissoes e analises-esg.
    /// </summary>
    public class AnalisesViewModel : ViewModelBase
    {
        private readonly HttpClient _httpClient;

        // Cards
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

        // Gráficos
        private SeriesCollection _graficoPizzaSeries;
        public SeriesCollection GraficoPizzaSeries
        {
            get => _graficoPizzaSeries;
            set { _graficoPizzaSeries = value; OnPropertyChanged(); }
        }

        private SeriesCollection _graficoBarrasSeries;
        public SeriesCollection GraficoBarrasSeries
        {
            get => _graficoBarrasSeries;
            set { _graficoBarrasSeries = value; OnPropertyChanged(); }
        }

        private string[] _mesesLabels;
        public string[] MesesLabels
        {
            get => _mesesLabels;
            set { _mesesLabels = value; OnPropertyChanged(); }
        }

        // DataGrid e Ranking
        private ObservableCollection<AvaliacaoFornecedor> _ultimasAvaliacoes = new();
        public ObservableCollection<AvaliacaoFornecedor> UltimasAvaliacoes
        {
            get => _ultimasAvaliacoes;
            set { _ultimasAvaliacoes = value; OnPropertyChanged(); }
        }

        private ObservableCollection<FornecedorSustentavel> _topFornecedores = new();
        public ObservableCollection<FornecedorSustentavel> TopFornecedores
        {
            get => _topFornecedores;
            set { _topFornecedores = value; OnPropertyChanged(); }
        }

        public AnalisesViewModel(HttpClient httpClient)
        {
            _httpClient = httpClient ?? new HttpClient();
            CarregarPlaceholders();
        }

        public async Task AtualizarAsync()
        {
            App.LogInfo("AtualizarAsync iniciado", "ANALISES");
            try
            {
                await Task.WhenAll(
                    CarregarPegadaMediaAsync(),
                    CarregarGraficoEmissoesAsync(),
                    CarregarAnalisesEsgAsync()
                );
                App.LogInfo("Todos os dados ESG carregados com sucesso.", "ANALISES");
            }
            catch (Exception ex)
            {
                App.LogError($"Erro em AtualizarAsync: {ex.Message}", "ANALISES");
            }
        }

        private async Task CarregarPegadaMediaAsync()
        {
            App.LogInfo("GET pegada-media...", "ANALISES");
            try
            {
                var response = await _httpClient.GetAsync("api/dados/pegada-media");
                App.LogInfo($"GET pegada-media → {(int)response.StatusCode}", "ANALISES");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(json);
                    var pegadaMedia = doc.RootElement.GetProperty("pegadaMedia").GetDouble();

                    TotalEmissoes = (int)(pegadaMedia * 0.1);
                    PecasReaproveitadas = (int)(pegadaMedia * 0.05);
                    FornecedoresVerdes = (int)(pegadaMedia * 0.002);
                    var economiaVal = TotalEmissoes * 150.0;
                    EconomiaGerada = economiaVal >= 1_000_000
                        ? $"R$ {economiaVal / 1_000_000:N1}M"
                        : $"R$ {economiaVal / 1_000:N0}K";

                    App.LogInfo($"Pegada média: {pegadaMedia:N1} | Cards atualizados", "ANALISES");
                }
                else
                {
                    App.LogError($"Falha pegada-media: HTTP {response.StatusCode}", "ANALISES");
                    EconomiaGerada = "Indisponível";
                }
            }
            catch (Exception ex)
            {
                App.LogError($"Erro em CarregarPegadaMediaAsync: {ex.Message}", "ANALISES");
                EconomiaGerada = "Indisponível";
            }
        }

        private async Task CarregarGraficoEmissoesAsync()
        {
            App.LogInfo("GET grafico-emissoes...", "ANALISES");
            try
            {
                var response = await _httpClient.GetAsync("api/dados/grafico-emissoes");
                App.LogInfo($"GET grafico-emissoes → {(int)response.StatusCode}", "ANALISES");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var dados = JsonSerializer.Deserialize<GraficoEmissoesDto>(json, options);
                    if (dados != null)
                    {
                        var valoresFabrica = new ChartValues<double>(dados.ValoresFabrica ?? Array.Empty<double>());
                        var valoresCadeia = new ChartValues<double>(dados.ValoresCadeia ?? Array.Empty<double>());

                        GraficoBarrasSeries = new SeriesCollection
                        {
                            new ColumnSeries
                            {
                                Title = "Processo Fabril",
                                Values = valoresFabrica,
                                Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0A5B43")),
                                MaxColumnWidth = 30
                            },
                            new ColumnSeries
                            {
                                Title = "Cadeia de Fornecedores",
                                Values = valoresCadeia,
                                Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4BAC50")),
                                MaxColumnWidth = 30
                            }
                        };
                        MesesLabels = dados.Meses ?? new[] { "Jan", "Fev", "Mar", "Abr", "Mai", "Jun" };
                        App.LogInfo($"Gráfico carregado: {MesesLabels.Length} meses", "ANALISES");
                    }
                }
                else
                {
                    App.LogError($"Falha grafico-emissoes: HTTP {response.StatusCode}", "ANALISES");
                }
            }
            catch (Exception ex)
            {
                App.LogError($"Erro em CarregarGraficoEmissoesAsync: {ex.Message}", "ANALISES");
            }
        }

        private async Task CarregarAnalisesEsgAsync()
        {
            App.LogInfo("GET analises-esg...", "ANALISES");
            try
            {
                var response = await _httpClient.GetAsync("api/dados/analises-esg");
                App.LogInfo($"GET analises-esg → {(int)response.StatusCode}", "ANALISES");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var dados = JsonSerializer.Deserialize<AnalisesESGDto>(json, options);
                    if (dados != null)
                    {
                        // --- Gráfico de Pizza ---
                        var coresPizza = new[] { "#0A5B43", "#1B7055", "#4BAC50", "#A7F3D0" };
                        var novasPizzaSeries = new SeriesCollection();
                        if (dados.DistribuicaoEmissoes != null)
                        {
                            for (int i = 0; i < dados.DistribuicaoEmissoes.Count; i++)
                            {
                                var escopo = dados.DistribuicaoEmissoes[i];
                                var cor = coresPizza[i % coresPizza.Length];
                                novasPizzaSeries.Add(new PieSeries
                                {
                                    Title = escopo.Escopo,
                                    Values = new ChartValues<double> { escopo.Porcentagem },
                                    DataLabels = true,
                                    Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(cor))
                                });
                            }
                        }
                        GraficoPizzaSeries = novasPizzaSeries;

                        // --- CONTAGEM REAL DE FORNECEDORES CERTIFICADOS ---
                        // Critério: pontuação ESG > 50 (scoreVerde * 100 > 50)
                        // Isso corresponde a scoreVerde > 0.5
                        int certificados = 0;
                        if (dados.TopFornecedoresVerdes != null)
                        {
                            certificados = dados.TopFornecedoresVerdes
                                .Count(f => (f.ScoreVerde * 100) > 50);
                        }
                        FornecedoresVerdes = certificados;
                        App.LogInfo($"Fornecedores certificados (pontuação > 50): {certificados}", "ANALISES");

                        // --- FILTRA fornecedores com dados (TotalPecas > 0) para tabela e ranking ---
                        var fornecedoresComDados = dados.TopFornecedoresVerdes?
                            .Where(f => f.TotalPecas > 0)
                            .OrderByDescending(f => f.ScoreVerde)
                            .ToList() ?? new List<FornecedorVerdeDto>();

                        // --- Ranking: TOP FORNECEDORES (apenas com dados) ---
                        var coresRanking = new[] { "#F59E0B", "#9CA3AF", "#B45309", "#10B981", "#10B981", "#3B82F6", "#8B5CF6", "#EC4899", "#F97316", "#14B8A6" };
                        var novoTopFornecedores = new ObservableCollection<FornecedorSustentavel>();
                        for (int i = 0; i < fornecedoresComDados.Count && i < 10; i++)
                        {
                            var f = fornecedoresComDados[i];
                            // Categoria exibida: se a pontuação > 50, considera certificado, senão mantém o que veio da API
                            string categoriaExibida = (f.ScoreVerde * 100) > 50 ? "Certificado" : (f.Certificado ?? "Pendente");
                            novoTopFornecedores.Add(new FornecedorSustentavel
                            {
                                Posicao = i + 1,
                                Nome = f.Nome,
                                Categoria = categoriaExibida,
                                PontuacaoESG = (int)(f.ScoreVerde * 100),
                                CorDestaque = coresRanking[i % coresRanking.Length]
                            });
                        }
                        TopFornecedores = novoTopFornecedores;
                        App.LogInfo($"TopFornecedores carregados: {TopFornecedores.Count} itens (com dados)", "ANALISES");

                        // --- Últimas Avaliações (apenas com dados) ---
                        var novasAvaliacoes = new ObservableCollection<AvaliacaoFornecedor>();
                        foreach (var f in fornecedoresComDados)
                        {
                            string statusExibido = (f.ScoreVerde * 100) > 50 ? "Certificado" : (f.Certificado ?? "Pendente");
                            novasAvaliacoes.Add(new AvaliacaoFornecedor
                            {
                                Fornecedor = f.Nome,
                                Material = $"{f.TotalPecas} peças fornecidas",
                                PegadaCarbono = Math.Round(f.PegadaMedia, 2),
                                DataAvaliacao = DateTime.Now,
                                Status = statusExibido
                            });
                        }
                        UltimasAvaliacoes = novasAvaliacoes;
                        App.LogInfo($"UltimasAvaliacoes carregadas: {UltimasAvaliacoes.Count} itens (com dados)", "ANALISES");

                        App.LogInfo($"ESG carregado: {TopFornecedores.Count} top, {UltimasAvaliacoes.Count} avaliações", "ANALISES");
                    }
                    else
                    {
                        App.LogError("Falha ao desserializar analises-esg: dados nulos", "ANALISES");
                    }
                }
                else
                {
                    App.LogError($"Falha analises-esg: HTTP {response.StatusCode}", "ANALISES");
                }
            }
            catch (Exception ex)
            {
                App.LogError($"Erro em CarregarAnalisesEsgAsync: {ex.Message}", "ANALISES");
            }


        }
        private void CarregarPlaceholders()
        {
            TotalEmissoes = 0;
            FornecedoresVerdes = 0;
            PecasReaproveitadas = 0;
            EconomiaGerada = "A carregar...";
            MesesLabels = new[] { "Jan", "Fev", "Mar", "Abr", "Mai", "Jun" };

            GraficoPizzaSeries = new SeriesCollection
            {
                new PieSeries { Title = "A carregar...", Values = new ChartValues<double> { 100 },
                    Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E5E7EB")) }
            };

            GraficoBarrasSeries = new SeriesCollection
            {
                new ColumnSeries
                {
                    Title = "A carregar...",
                    Values = new ChartValues<double> { 0, 0, 0, 0, 0, 0 },
                    Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D1FAE5")),
                    MaxColumnWidth = 30
                }
            };

            UltimasAvaliacoes = new ObservableCollection<AvaliacaoFornecedor>();
            TopFornecedores = new ObservableCollection<FornecedorSustentavel>();
        }
    }
}