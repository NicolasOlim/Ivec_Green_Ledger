using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows; // Necessário para o MessageBox
using System.Windows.Threading;
using WpfIveco.Models;

namespace WpfIveco.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly HttpClient _httpClient;
        private readonly DispatcherTimer _timer;

        // Alterei o texto inicial para sabermos se o Binding está a funcionar
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

        public MainViewModel()
        {
            // 1. TRUQUE MÁGICO: Ignorar erros de certificado HTTPS no localhost
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
            };

            _httpClient = new HttpClient(handler);
            _httpClient.BaseAddress = new Uri("https://localhost:44353/");

            _ = CarregarDadosDaApiAsync();

            _timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(10) };
            _timer.Tick += async (sender, args) => await CarregarDadosDaApiAsync();
            _timer.Start();
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
            catch (Exception ex)
            {
                // 2. FORÇAR A MOSTRAR O ERRO NA TELA!
                MessageBox.Show($"Ocorreu um erro ao conectar com a API:\n\n{ex.Message}", "Erro de Comunicação", MessageBoxButton.OK, MessageBoxImage.Error);
                TotalVeiculos = "Falha";
                TotalFornecedores = "Falha";
            }
        }
    }
}