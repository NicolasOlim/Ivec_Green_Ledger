using LiveCharts;
using LiveCharts.Wpf;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Media;
using WpfIveco.Models;

namespace WpfIveco.ViewModels
{

    public class AnalisesViewModel : INotifyPropertyChanged

    {
        private readonly HttpClient _httpClient;

        // ==========================================
        // 1. PROPRIEDADES DOS CARDS SUPERIORES
        // ==========================================
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

        private string _economiaGerada;
        public string EconomiaGerada
        {
            get => _economiaGerada;
            set { _economiaGerada = value; OnPropertyChanged(); }
        }

        // ==========================================
        // 2. PROPRIEDADES DOS GRÁFICOS (LiveCharts)
        // ==========================================
        public SeriesCollection GraficoPizzaSeries { get; set; }
        public SeriesCollection GraficoBarrasSeries { get; set; }
        public string[] MesesLabels { get; set; }

        // ==========================================
        // 3. COLEÇÕES DAS TABELAS E LISTAS
        // ==========================================
        public ObservableCollection<AvaliacaoFornecedor> UltimasAvaliacoes { get; set; }
        public ObservableCollection<FornecedorSustentavel> TopFornecedores { get; set; }

        // ==========================================
        // CONSTRUTORES (CORRIGIDOS)
        // ==========================================

        // Construtor padrão
        public AnalisesViewModel()
        {
            CarregarDadosFalsos();
        }

        // Construtor com 1 argumento (o que o MainViewModel estava sentindo falta!)
        public AnalisesViewModel(HttpClient httpClient) : this()
        {
            _httpClient = httpClient;
        }

        // ==========================================
        // MÉTODOS (CORRIGIDOS)
        // ==========================================

        // O método que o MainViewModel tenta chamar para atualizar os dados
        public async Task AtualizarAsync()
        {
            // Por enquanto, vamos apenas simular um tempo de carregamento de API (200ms)
            // e carregar os dados falsos. Futuramente você pode colocar suas requisições
            // _httpClient.GetAsync(...) aqui dentro novamente!
            await Task.Delay(200);
            CarregarDadosFalsos();
        }

        private void CarregarDadosFalsos()
        {
            TotalEmissoes = 14250;
            FornecedoresVerdes = 48;
            PecasReaproveitadas = 12400;
            EconomiaGerada = "R$ 1.2M";

            GraficoPizzaSeries = new SeriesCollection
            {
                new PieSeries { Title = "Logística", Values = new ChartValues<double> { 45 }, Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0A5B43")) },
                new PieSeries { Title = "Produção", Values = new ChartValues<double> { 35 }, Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1B7055")) },
                new PieSeries { Title = "Fornecedores", Values = new ChartValues<double> { 15 }, Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4BAC50")) },
                new PieSeries { Title = "Administrativo", Values = new ChartValues<double> { 5 }, Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#A7F3D0")) }
            };

            GraficoBarrasSeries = new SeriesCollection
            {
                new ColumnSeries
                {
                    Title = "Redução (ton)",
                    Values = new ChartValues<double> { 120, 150, 210, 180, 290, 320 },
                    Fill = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#1B7055")),
                    MaxColumnWidth = 30
                }
            };
            MesesLabels = new[] { "Jan", "Fev", "Mar", "Abr", "Mai", "Jun" };

            UltimasAvaliacoes = new ObservableCollection<AvaliacaoFornecedor>
            {
                new AvaliacaoFornecedor { Fornecedor = "BOSCH Sistemas", Material = "Injeção Eletrônica", PegadaCarbono = 12.5, DataAvaliacao = DateTime.Now.AddDays(-2), Status = "ISO 14001" },
                new AvaliacaoFornecedor { Fornecedor = "Pirelli Pneus", Material = "Borracha Sintética", PegadaCarbono = 45.2, DataAvaliacao = DateTime.Now.AddDays(-5), Status = "ISO 14001" },
                new AvaliacaoFornecedor { Fornecedor = "Aço Frio S/A", Material = "Chapas Metálicas", PegadaCarbono = 110.0, DataAvaliacao = DateTime.Now.AddDays(-10), Status = "Pendente" },
                new AvaliacaoFornecedor { Fornecedor = "Vidros Auto", Material = "Para-brisas", PegadaCarbono = 22.1, DataAvaliacao = DateTime.Now.AddDays(-12), Status = "Em Análise" }
            };

            TopFornecedores = new ObservableCollection<FornecedorSustentavel>
            {
                new FornecedorSustentavel { Posicao = 1, Nome = "EcoParts Ltda", Categoria = "Selo Ouro - Emissão Zero", PontuacaoESG = 98, CorDestaque = "#F59E0B" },
                new FornecedorSustentavel { Posicao = 2, Nome = "BOSCH Sistemas", Categoria = "Selo Prata - Circular", PontuacaoESG = 92, CorDestaque = "#9CA3AF" },
                new FornecedorSustentavel { Posicao = 3, Nome = "TechGreen", Categoria = "Selo Bronze - Limpa", PontuacaoESG = 88, CorDestaque = "#B45309" },
                new FornecedorSustentavel { Posicao = 4, Nome = "Pirelli Pneus", Categoria = "Certificado ISO 14001", PontuacaoESG = 82, CorDestaque = "#10B981" },
                new FornecedorSustentavel { Posicao = 5, Nome = "Alumínios Sustentáveis", Categoria = "Certificado ISO 14001", PontuacaoESG = 79, CorDestaque = "#10B981" }
            };
        }

        // ==========================================
        // IMPLEMENTAÇÃO DO INOTIFYPROPERTYCHANGED
        // ==========================================
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }


   
}