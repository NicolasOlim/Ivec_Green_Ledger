using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
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

        // Gráficos – as coleções são inicializadas uma vez e reutilizadas
        private SeriesCollection _graficoPizzaSeries = new SeriesCollection();
        public SeriesCollection GraficoPizzaSeries
        {
            get => _graficoPizzaSeries;
            set { _graficoPizzaSeries = value; OnPropertyChanged(); }
        }

        private SeriesCollection _graficoBarrasSeries = new SeriesCollection();
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

        // DataGrid e Ranking – coleções reutilizadas
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
                    CarregarAnalisesEsgAsync(),
                    CarregarTotalEmissoesAsync() 
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

                    PecasReaproveitadas = (int)(pegadaMedia * 0.05);
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
                        await Application.Current.Dispatcher.InvokeAsync(() =>
                        {
                            // Limpa a coleção existente
                            GraficoBarrasSeries.Clear();

                            var valoresFabrica = dados.ValoresFabrica ?? Array.Empty<double>();
                            var valoresCadeia = dados.ValoresCadeia ?? Array.Empty<double>();
                            MesesLabels = dados.Meses ?? new[] { "Jan", "Fev", "Mar", "Abr", "Mai", "Jun" };

                            // Adiciona as duas séries
                            GraficoBarrasSeries.Add(new ColumnSeries
                            {
                                Title = "Processo Fabril",
                                Values = new ChartValues<double>(valoresFabrica),
                                Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0A5B43")),
                                MaxColumnWidth = 30
                            });

                            GraficoBarrasSeries.Add(new ColumnSeries
                            {
                                Title = "Cadeia de Fornecedores",
                                Values = new ChartValues<double>(valoresCadeia),
                                Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4BAC50")),
                                MaxColumnWidth = 30
                            });

                            // Força notificação
                            OnPropertyChanged(nameof(GraficoBarrasSeries));
                            OnPropertyChanged(nameof(MesesLabels));

                            App.LogInfo($"Gráfico de barras atualizado com {MesesLabels.Length} meses", "ANALISES");
                        });
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
                        await Application.Current.Dispatcher.InvokeAsync(() =>
                        {
                            // --- GRÁFICO DE PIZZA ---
                            GraficoPizzaSeries.Clear();
                            if (dados.DistribuicaoEmissoes != null && dados.DistribuicaoEmissoes.Count > 0)
                            {
                                var coresPizza = new[] { "#0A5B43", "#1B7055", "#4BAC50", "#A7F3D0" };
                                for (int i = 0; i < dados.DistribuicaoEmissoes.Count; i++)
                                {
                                    var escopo = dados.DistribuicaoEmissoes[i];
                                    var cor = coresPizza[i % coresPizza.Length];
                                    GraficoPizzaSeries.Add(new PieSeries
                                    {
                                        Title = escopo.Escopo,
                                        Values = new ChartValues<double> { escopo.Porcentagem },
                                        DataLabels = true,
                                        Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(cor))
                                    });
                                }
                            }
                            else
                            {
                                GraficoPizzaSeries.Add(new PieSeries
                                {
                                    Title = "Sem dados",
                                    Values = new ChartValues<double> { 100 },
                                    DataLabels = true,
                                    Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E5E7EB"))
                                });
                            }
                            OnPropertyChanged(nameof(GraficoPizzaSeries));

                            // --- CONTAGEM DE CERTIFICADOS ---
                            int certificados = 0;
                            if (dados.TopFornecedoresVerdes != null)
                            {
                                certificados = dados.TopFornecedoresVerdes
                                    .Count(f => (f.ScoreVerde * 100) > 50);
                            }
                            FornecedoresVerdes = certificados;

                            // --- FORNECEDORES COM DADOS ---
                            var fornecedoresComDados = dados.TopFornecedoresVerdes?
                                .Where(f => f.TotalPecas > 0)
                                .OrderByDescending(f => f.ScoreVerde)
                                .ToList() ?? new List<FornecedorVerdeDto>();

                            // --- RANKING ---
                            TopFornecedores.Clear();
                            var coresRanking = new[] { "#F59E0B", "#9CA3AF", "#B45309", "#10B981", "#10B981", "#3B82F6", "#8B5CF6", "#EC4899", "#F97316", "#14B8A6" };
                            for (int i = 0; i < fornecedoresComDados.Count && i < 10; i++)
                            {
                                var f = fornecedoresComDados[i];
                                string categoriaExibida = (f.ScoreVerde * 100) > 50 ? "Certificado" : (f.Certificado ?? "Pendente");
                                TopFornecedores.Add(new FornecedorSustentavel
                                {
                                    Posicao = i + 1,
                                    Nome = f.Nome,
                                    Categoria = categoriaExibida,
                                    PontuacaoESG = (int)(f.ScoreVerde * 100),
                                    CorDestaque = coresRanking[i % coresRanking.Length]
                                });
                            }
                            OnPropertyChanged(nameof(TopFornecedores));

                            // --- TABELA ---
                            UltimasAvaliacoes.Clear();
                            foreach (var f in fornecedoresComDados)
                            {
                                string statusExibido = (f.ScoreVerde * 100) > 50 ? "Certificado" : (f.Certificado ?? "Pendente");
                                UltimasAvaliacoes.Add(new AvaliacaoFornecedor
                                {
                                    Fornecedor = f.Nome,
                                    Material = $"{f.TotalPecas} peças fornecidas",
                                    PegadaCarbono = Math.Round(f.PegadaMedia, 2),
                                    DataAvaliacao = DateTime.Now,
                                    Status = statusExibido
                                });
                            }
                            OnPropertyChanged(nameof(UltimasAvaliacoes));

                            App.LogInfo($"ESG atualizado: pizza={GraficoPizzaSeries.Count}, top={TopFornecedores.Count}, tabela={UltimasAvaliacoes.Count}", "ANALISES");
                        });
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

            // Placeholder do gráfico de pizza
            GraficoPizzaSeries.Clear();
            GraficoPizzaSeries.Add(new PieSeries
            {
                Title = "A carregar...",
                Values = new ChartValues<double> { 100 },
                Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E5E7EB"))
            });

            // Placeholder do gráfico de barras
            GraficoBarrasSeries.Clear();
            GraficoBarrasSeries.Add(new ColumnSeries
            {
                Title = "A carregar...",
                Values = new ChartValues<double> { 0, 0, 0, 0, 0, 0 },
                Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#D1FAE5")),
                MaxColumnWidth = 30
            });

            UltimasAvaliacoes.Clear();
            TopFornecedores.Clear();
        }

        /// <summary>
        /// Busca o total real de emissões (soma de veículos e lotes) do endpoint /total-emissoes.
        /// </summary>
        private async Task CarregarTotalEmissoesAsync()
        {
            App.LogInfo("GET total-emissoes...", "ANALISES");
            try
            {
                var response = await _httpClient.GetAsync("api/dados/total-emissoes");
                App.LogInfo($"GET total-emissoes → {(int)response.StatusCode}", "ANALISES");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(json);
                    var total = doc.RootElement.GetProperty("totalEmissoes").GetDouble();

                    // Converte kg para toneladas e arredonda para inteiro
                    TotalEmissoes = (int)Math.Round(total / 1000);
                    App.LogInfo($"Total de emissões reais: {TotalEmissoes} ton ({total} kg)", "ANALISES");
                }
                else
                {
                    var erro = await response.Content.ReadAsStringAsync();
                    App.LogError($"Falha total-emissoes: HTTP {response.StatusCode} - {erro}", "ANALISES");
                }
            }
            catch (Exception ex)
            {
                App.LogError($"Erro em CarregarTotalEmissoesAsync: {ex.Message}", "ANALISES");
            }
        }

    }
}