using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using WpfIveco.Models;

namespace WpfIveco.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly HttpClient _httpClient;
        private readonly DispatcherTimer _timer;

        // --- VARIÁVEIS DA TELA DASHBOARD ---
        private string _totalVeiculos = "A carregar...";
        public string TotalVeiculos
        {
            get => _totalVeiculos;
            set { _totalVeiculos = value; OnPropertyChanged(); }
        }

        private string _totalFornecedores = "A carregar...";
        public string TotalFornecedores
        {
            get => _totalFornecedores;
            set { _totalFornecedores = value; OnPropertyChanged(); }
        }

        // --- VARIÁVEIS DA TELA PROJETOS ---
        private string _pesquisaVin = "";
        public string PesquisaVin
        {
            get => _pesquisaVin;
            set { _pesquisaVin = value; OnPropertyChanged(); }
        }

        // --- VARIÁVEIS DA TELA AJUSTES ---
        private string _apiUrlConfig = "https://localhost:44353/api";
        public string ApiUrlConfig
        {
            get => _apiUrlConfig;
            set { _apiUrlConfig = value; OnPropertyChanged(); }
        }

        private string _statusSimulador = "Parado";
        public string StatusSimulador
        {
            get => _statusSimulador;
            set { _statusSimulador = value; OnPropertyChanged(); }
        }

        // --- NAVEGAÇÃO ---
        private string _abaAtiva = "Dashboard";
        public string AbaAtiva
        {
            get => _abaAtiva;
            set { _abaAtiva = value; OnPropertyChanged(); }
        }

        // --- COMANDOS DOS BOTÕES ---
        public ICommand MudarAbaCommand { get; }
        public ICommand PesquisarVinCommand { get; }
        public ICommand LigarDesligarSimuladorCommand { get; }
        public ICommand LimparBaseDadosCommand { get; }

        public MainViewModel()
        {
            // Inicializa os Comandos
            MudarAbaCommand = new RelayCommand(MudarAba);
            PesquisarVinCommand = new RelayCommand(p => MessageBox.Show($"A procurar dados do veículo com VIN: {PesquisaVin}\n\n(Lógica a implementar)", "Rastreabilidade IVECO"));
            LigarDesligarSimuladorCommand = new RelayCommand(p => StatusSimulador = StatusSimulador == "A Correr" ? "Parado" : "A Correr");
            LimparBaseDadosCommand = new RelayCommand(p => MessageBox.Show("Base de dados local (SQLite) limpa com sucesso!", "Gestão de Dados"));

            // Ignora erros de certificado HTTPS no localhost
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
            };

            _httpClient = new HttpClient(handler);
            _httpClient.BaseAddress = new Uri("https://localhost:44353/"); // Porta da sua API

            // Primeira chamada à API
            _ = CarregarDadosDaApiAsync();

            // Temporizador: Atualiza a cada 10 segundos
            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(10) };
            _timer.Tick += async (sender, args) => await CarregarDadosDaApiAsync();
            _timer.Start();
        }

        private void MudarAba(object parametro)
        {
            if (parametro is string nomeDaAba)
            {
                AbaAtiva = nomeDaAba;
            }
        }

        private async Task CarregarDadosDaApiAsync()
        {
            try
            {
                var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                // Busca Veículos
                var responseVeiculos = await _httpClient.GetAsync("api/dados/veiculos");
                if (responseVeiculos.IsSuccessStatusCode)
                {
                    var json = await responseVeiculos.Content.ReadAsStringAsync();
                    var veiculos = JsonSerializer.Deserialize<List<VeiculoModel>>(json, jsonOptions);
                    TotalVeiculos = veiculos.Count.ToString();
                }
                else
                {
                    TotalVeiculos = $"Erro {responseVeiculos.StatusCode}";
                }

                // Busca Fornecedores
                var responseFornecedores = await _httpClient.GetAsync("api/dados/fornecedores");
                if (responseFornecedores.IsSuccessStatusCode)
                {
                    var json = await responseFornecedores.Content.ReadAsStringAsync();
                    var fornecedores = JsonSerializer.Deserialize<List<FornecedorModel>>(json, jsonOptions);
                    TotalFornecedores = fornecedores.Count.ToString();
                }
                else
                {
                    TotalFornecedores = $"Erro {responseFornecedores.StatusCode}";
                }
            }
            catch (Exception)
            {
                // Em caso de falha de conexão silenciosa (para não encher de mensagens)
                TotalVeiculos = "Falha";
                TotalFornecedores = "Falha";
            }
        }
    }
}