using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Threading;
using WpfIveco.Models;

namespace WpfIveco.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly HttpClient _httpClient;
        private readonly DispatcherTimer _timer;

        // Variáveis observáveis. Quando elas mudam, a tela muda junto.
        private string _totalVeiculos = "...";
        public string TotalVeiculos
        {
            get => _totalVeiculos;
            set { _totalVeiculos = value; OnPropertyChanged(); }
        }

        private string _totalFornecedores = "...";
        public string TotalFornecedores
        {
            get => _totalFornecedores;
            set { _totalFornecedores = value; OnPropertyChanged(); }
        }

        public MainViewModel()
        {
            _httpClient = new HttpClient();
            // ATENÇÃO: Troque pela porta correta da sua API
            _httpClient.BaseAddress = new Uri("https://localhost:7193/");

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

                // Veículos
                var responseVeiculos = await _httpClient.GetAsync("api/dados/veiculos");
                if (responseVeiculos.IsSuccessStatusCode)
                {
                    var json = await responseVeiculos.Content.ReadAsStringAsync();
                    var veiculos = JsonSerializer.Deserialize<List<VeiculoModel>>(json, jsonOptions);

                    TotalVeiculos = veiculos.Count.ToString(); // Isso atualiza o XAML automaticamente
                }

                // Fornecedores
                var responseFornecedores = await _httpClient.GetAsync("api/dados/fornecedores");
                if (responseFornecedores.IsSuccessStatusCode)
                {
                    var json = await responseFornecedores.Content.ReadAsStringAsync();
                    var fornecedores = JsonSerializer.Deserialize<List<FornecedorModel>>(json, jsonOptions);

                    TotalFornecedores = fornecedores.Count.ToString(); // Isso atualiza o XAML automaticamente
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Falha na API: {ex.Message}");
                TotalVeiculos = "Erro";
                TotalFornecedores = "Erro";
            }
        }
    }
}