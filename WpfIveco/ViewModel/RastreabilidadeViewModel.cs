using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using WpfIveco.Models;
using WpfIveco.ViewModels;

namespace WpfIveco.ViewModel
{
    public class RastreabilidadeViewModel : ViewModelBase
    {
        private readonly HttpClient _httpClient;

        private string _pesquisaVin = "";
        public string PesquisaVin
        {
            get => _pesquisaVin;
            set { _pesquisaVin = value; OnPropertyChanged(); }
        }

        private string _totalVeiculos = "0";
        public string TotalVeiculos
        {
            get => _totalVeiculos;
            set { _totalVeiculos = value; OnPropertyChanged(); }
        }

        private ObservableCollection<VeiculoModel> _listaVeiculos = new();
        public ObservableCollection<VeiculoModel> ListaVeiculos
        {
            get => _listaVeiculos;
            set { _listaVeiculos = value; OnPropertyChanged(); }
        }

        public ICommand PesquisarVinCommand { get; }

        public RastreabilidadeViewModel(HttpClient httpClient)
        {
            App.LogInfo("Construtor", "RASTREAB");
            _httpClient = httpClient;
            PesquisarVinCommand = new RelayCommand(async p => await PesquisarVinAsync());
        }

        public async Task CarregarVeiculosAsync()
        {
            App.LogInfo("CarregarVeiculosAsync iniciado", "RASTREAB");
            try
            {
                var response = await _httpClient.GetAsync("api/dados/veiculos");
                App.LogInfo($"GET Veículos → {(int)response.StatusCode}", "RASTREAB");
                if (!response.IsSuccessStatusCode) return;

                var json = await response.Content.ReadAsStringAsync();
                var veiculos = JsonSerializer.Deserialize<List<VeiculoModel>>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (veiculos != null)
                {
                    ListaVeiculos = new ObservableCollection<VeiculoModel>(veiculos);
                    TotalVeiculos = veiculos.Count.ToString();
                    App.LogInfo($"{veiculos.Count} veículos carregados", "RASTREAB");
                }
            }
            catch
            {
                App.LogError("Erro ao carregar veículos – lista vazia", "RASTREAB");
                ListaVeiculos = new ObservableCollection<VeiculoModel>();
                TotalVeiculos = "0";
            }
        }

        private async Task PesquisarVinAsync()
        {
            App.LogInfo($"Pesquisando VIN: {PesquisaVin}", "RASTREAB");
            if (string.IsNullOrWhiteSpace(PesquisaVin) || PesquisaVin.Length != 17)
            {
                App.LogWarning("VIN inválido", "RASTREAB");
                MessageBox.Show("Introduza um VIN válido com 17 caracteres.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var response = await _httpClient.GetAsync($"api/dados/veiculos/validar-vin/{PesquisaVin}");
                App.LogInfo($"GET validar-vin → {(int)response.StatusCode}", "RASTREAB");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(json);
                    var veiculoJson = doc.RootElement.GetProperty("veiculo").GetRawText();

                    var content = new StringContent(veiculoJson, System.Text.Encoding.UTF8, "application/json");
                    var resSalvar = await _httpClient.PostAsync("api/dados/veiculos", content);
                    App.LogInfo($"POST veiculos → {(int)resSalvar.StatusCode}", "RASTREAB");

                    if (resSalvar.IsSuccessStatusCode)
                    {
                        App.LogInfo("Veículo rastreado e guardado!", "RASTREAB");
                        MessageBox.Show("Veículo IVECO rastreado e guardado no Ledger!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                        PesquisaVin = "";
                        await CarregarVeiculosAsync();
                    }
                    else if (resSalvar.StatusCode == System.Net.HttpStatusCode.Conflict)
                    {
                        App.LogWarning("Veículo já registado", "RASTREAB");
                        MessageBox.Show("Veículo autêntico, mas já estava registado no sistema.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                    else
                    {
                        var erro = await resSalvar.Content.ReadAsStringAsync();
                        App.LogError($"Falha ao salvar: {erro}", "RASTREAB");
                        MessageBox.Show("Não foi possível guardar o veículo.\nTente novamente.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    var erro = await response.Content.ReadAsStringAsync();
                    App.LogError($"Falha na validação: {erro}", "RASTREAB");
                    MessageBox.Show("Este VIN não pertence a um veículo Iveco válido.", "Acesso Negado", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (HttpRequestException)
            {
                App.LogWarning("Modo offline ativado", "RASTREAB");
                MessageBox.Show("Modo Offline ativado. O veículo será guardado localmente e sincronizado quando a rede for restabelecida.",
                    "Aviso de Contingência", MessageBoxButton.OK, MessageBoxImage.Information);
                PesquisaVin = "";
            }
            catch
            {
                App.LogError("Erro inesperado ao pesquisar VIN", "RASTREAB");
                MessageBox.Show("Ocorreu um erro inesperado.\nTente novamente ou contacte o suporte.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}