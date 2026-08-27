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
    /// CORREÇÃO: Fallback para dados vazios, tratamento de exceções na economia, logs detalhados.
    /// </summary>
    public class AnalisesViewModel : ViewModelBase
    {
        // ============================================================
        // CAMPOS PRIVADOS
        // ============================================================

        private readonly HttpClient _httpClient;

        // ============================================================
        // CARDS
        // ============================================================

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

        // ============================================================
        // GRÁFICOS
        // ============================================================

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

        // ============================================================
        // DATAGRID E RANKING
        // ============================================================

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

        // ============================================================
        // CONSTRUTOR
        // ============================================================

        public AnalisesViewModel(HttpClient httpClient)
        {
            _httpClient = httpClient ?? new HttpClient();
            CarregarPlaceholders();
        }

        // ============================================================
        // MÉTODO PRINCIPAL
        // ============================================================

        public async Task AtualizarAsync()
        {
            App.LogInfo("AtualizarAsync iniciado", "ANALISES");
            try
            {
                await CarregarTotalEmissoesAsync();
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
                // Mantém placeholders em caso de erro total
            }
        }

        // ============================================================
        // MÉTODOS DE CARREGAMENTO
        // ============================================================

        private async Task CarregarPegadaMediaAsync()
        {
            App.LogInfo("GET pegada-media...", "ANALISES");
            try
            {
                var response = await _httpClient.GetAsync("api/dados/pegada-media");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(json);
                    var pegadaMedia = doc.RootElement.GetProperty("pegadaMedia").GetDouble();

                    PecasReaproveitadas = (int)(pegadaMedia * 0.05);
                    // A economia é calculada separadamente
                    App.LogInfo($"Pegada média: {pegadaMedia:N1}", "ANALISES");
                }
                else
                {
                    App.LogError($"Falha pegada-media: HTTP {response.StatusCode}", "ANALISES");
                    // Fallback: mantém valores existentes
                }
            }
            catch (Exception ex)
            {
                App.LogError($"Erro em CarregarPegadaMediaAsync: {ex.Message}", "ANALISES");
            }
        }

        private async Task CarregarGraficoEmissoesAsync()
        {
            App.LogInfo("CarregarGraficoEmissoesAsync iniciado", "ANALISES");
            try
            {
                var response = await _httpClient.GetAsync("api/dados/grafico-emissoes");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var dados = JsonSerializer.Deserialize<GraficoEmissoesDto>(json, options);

                    if (dados != null && dados.ValoresFabrica != null && dados.ValoresFabrica.Length > 0)
                    {
                        await Application.Current.Dispatcher.InvokeAsync(() =>
                        {
                            GraficoBarrasSeries.Clear();
                            var corFabrica = (Color)ColorConverter.ConvertFromString("#0A5B43");
                            var corCadeia = (Color)ColorConverter.ConvertFromString("#4BAC50");

                            GraficoBarrasSeries.Add(new ColumnSeries
                            {
                                Title = "Processo Fabril",
                                Values = new ChartValues<double>(dados.ValoresFabrica),
                                Fill = new SolidColorBrush(corFabrica),
                                MaxColumnWidth = 30
                            });

                            GraficoBarrasSeries.Add(new ColumnSeries
                            {
                                Title = "Cadeia de Fornecedores",
                                Values = new ChartValues<double>(dados.ValoresCadeia ?? new double[] { 0, 0, 0, 0, 0, 0 }),
                                Fill = new SolidColorBrush(corCadeia),
                                MaxColumnWidth = 30
                            });

                            MesesLabels = dados.Meses ?? new[] { "Jan", "Fev", "Mar", "Abr", "Mai", "Jun" };
                            OnPropertyChanged(nameof(GraficoBarrasSeries));
                            OnPropertyChanged(nameof(MesesLabels));
                            App.LogInfo("Gráfico de barras atualizado.", "ANALISES");
                        });
                    }
                    else
                    {
                        App.LogWarning("Dados do gráfico vazios, usando placeholders.", "ANALISES");
                    }
                }
                else
                {
                    App.LogError($"Falha grafico-emissoes: {response.StatusCode}", "ANALISES");
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
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var dados = JsonSerializer.Deserialize<AnalisesESGDto>(json, options);

                    if (dados != null)
                    {
                        await Application.Current.Dispatcher.InvokeAsync(() =>
                        {
                            // Atualiza pizza
                            GraficoPizzaSeries.Clear();
                            if (dados.DistribuicaoEmissoes != null && dados.DistribuicaoEmissoes.Count > 0)
                            {
                                var coresPizza = new[] { "#0A5B43", "#1B7055", "#4BAC50", "#A7F3D0" };
                                for (int i = 0; i < dados.DistribuicaoEmissoes.Count; i++)
                                {
                                    var escopo = dados.DistribuicaoEmissoes[i];
                                    GraficoPizzaSeries.Add(new PieSeries
                                    {
                                        Title = escopo.Escopo,
                                        Values = new ChartValues<double> { escopo.Porcentagem },
                                        DataLabels = true,
                                        Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString(coresPizza[i % coresPizza.Length]))
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

                            // Fornecedores verdes
                            int certificados = dados.TopFornecedoresVerdes?.Count(f => (f.ScoreVerde * 100) > 50) ?? 0;
                            FornecedoresVerdes = certificados;

                            // Ranking
                            TopFornecedores.Clear();
                            var fornecedoresOrdenados = dados.TopFornecedoresVerdes?
                                .Where(f => f.TotalPecas > 0)
                                .OrderByDescending(f => f.ScoreVerde)
                                .Take(10)
                                .ToList() ?? new List<FornecedorVerdeDto>();

                            var coresRanking = new[] { "#F59E0B", "#9CA3AF", "#B45309", "#10B981", "#10B981", "#3B82F6", "#8B5CF6", "#EC4899", "#F97316", "#14B8A6" };
                            for (int i = 0; i < fornecedoresOrdenados.Count; i++)
                            {
                                var f = fornecedoresOrdenados[i];
                                TopFornecedores.Add(new FornecedorSustentavel
                                {
                                    Posicao = i + 1,
                                    Nome = f.Nome,
                                    Categoria = (f.ScoreVerde * 100) > 50 ? "Certificado" : "Pendente",
                                    PontuacaoESG = (int)(f.ScoreVerde * 100),
                                    CorDestaque = coresRanking[i % coresRanking.Length]
                                });
                            }
                            OnPropertyChanged(nameof(TopFornecedores));

                            // Tabela
                            UltimasAvaliacoes.Clear();
                            foreach (var f in fornecedoresOrdenados)
                            {
                                UltimasAvaliacoes.Add(new AvaliacaoFornecedor
                                {
                                    Fornecedor = f.Nome,
                                    Material = $"{f.TotalPecas} peças fornecidas",
                                    PegadaCarbono = Math.Round(f.PegadaMedia, 2),
                                    DataAvaliacao = DateTime.Now,
                                    Status = (f.ScoreVerde * 100) > 50 ? "ISO 14001" : "Pendente"
                                });
                            }
                            OnPropertyChanged(nameof(UltimasAvaliacoes));

                            App.LogInfo($"ESG atualizado: Top={TopFornecedores.Count}, Tabela={UltimasAvaliacoes.Count}", "ANALISES");
                        });
                    }
                }
                else
                {
                    App.LogError($"Falha analises-esg: {response.StatusCode}", "ANALISES");
                }
            }
            catch (Exception ex)
            {
                App.LogError($"Erro em CarregarAnalisesEsgAsync: {ex.Message}", "ANALISES");
            }
        }

        // ============================================================
        // TOTAL DE EMISSÕES E ECONOMIA (com fallback)
        // ============================================================

        private async Task CarregarTotalEmissoesAsync()
        {
            App.LogInfo("CarregarTotalEmissoesAsync iniciado", "ANALISES");
            try
            {
                var response = await _httpClient.GetAsync("api/dados/total-emissoes");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(json);
                    var total = doc.RootElement.GetProperty("totalEmissoes").GetDouble();
                    TotalEmissoes = (int)Math.Round(total / 1000);
                    App.LogInfo($"Total de emissões: {TotalEmissoes} ton", "ANALISES");
                    await CalcularEconomiaAsync(TotalEmissoes);
                }
                else
                {
                    App.LogWarning("Falha total-emissoes, estimando por pegada média.", "ANALISES");
                    await EstimarEconomiaPorPegadaMedia();
                }
            }
            catch (Exception ex)
            {
                App.LogError($"Erro em CarregarTotalEmissoesAsync: {ex.Message}", "ANALISES");
                await EstimarEconomiaPorPegadaMedia();
            }
        }

        private async Task EstimarEconomiaPorPegadaMedia()
        {
            App.LogInfo("Estimando economia via pegada média...", "ANALISES");
            try
            {
                var response = await _httpClient.GetAsync("api/dados/pegada-media");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(json);
                    var pegadaMedia = doc.RootElement.GetProperty("pegadaMedia").GetDouble();
                    int estimado = (int)(pegadaMedia * 0.1);
                    TotalEmissoes = estimado > 0 ? estimado : 2; // fallback mínimo
                    App.LogInfo($"Total estimado: {TotalEmissoes} ton", "ANALISES");
                    await CalcularEconomiaAsync(TotalEmissoes);
                }
                else
                {
                    TotalEmissoes = 2;
                    await CalcularEconomiaAsync(2);
                }
            }
            catch
            {
                TotalEmissoes = 2;
                await CalcularEconomiaAsync(2);
            }
        }

        private async Task CalcularEconomiaAsync(double totalTon)
        {
            try
            {
                // Tenta obter preço real
                double precoPorTon = 150.0;
                try
                {
                    var response = await _httpClient.GetAsync("api/dados/preco-carbono");
                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        using var doc = JsonDocument.Parse(json);
                        precoPorTon = doc.RootElement.GetProperty("preco").GetDouble();
                    }
                }
                catch (Exception ex)
                {
                    App.LogWarning($"Falha ao obter preço do carbono: {ex.Message}. Usando fallback R$150,00.", "ANALISES");
                }

                var economiaVal = totalTon * precoPorTon;
                if (economiaVal < 0) economiaVal = 0;

                string economiaFormatada;
                if (economiaVal >= 1_000_000)
                    economiaFormatada = $"R$ {economiaVal / 1_000_000:N1}M";
                else if (economiaVal >= 1_000)
                    economiaFormatada = $"R$ {economiaVal / 1_000:N1}K";
                else
                    economiaFormatada = $"R$ {economiaVal:N0}";

                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    EconomiaGerada = economiaFormatada;
                    App.LogInfo($"Economia atualizada: {EconomiaGerada}", "ANALISES");
                });
            }
            catch (Exception ex)
            {
                App.LogError($"Erro ao calcular economia: {ex.Message}", "ANALISES");
                await Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    EconomiaGerada = "Indisponível";
                });
            }
        }

        // ============================================================
        // PLACEHOLDERS (INICIALIZAÇÃO)
        // ============================================================

        private void CarregarPlaceholders()
        {
            TotalEmissoes = 0;
            FornecedoresVerdes = 0;
            PecasReaproveitadas = 0;
            EconomiaGerada = "A carregar...";
            MesesLabels = new[] { "Jan", "Fev", "Mar", "Abr", "Mai", "Jun" };

            GraficoPizzaSeries.Clear();
            GraficoPizzaSeries.Add(new PieSeries
            {
                Title = "A carregar...",
                Values = new ChartValues<double> { 100 },
                Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#E5E7EB"))
            });

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
    }
}