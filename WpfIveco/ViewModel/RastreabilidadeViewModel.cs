using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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

        
        /// <summary>
        /// PROPRIEDADES
        /// </summary>
        
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

        
        /// <summary>
        /// COMANDOS
        /// </summary>
        
        public ICommand PesquisarVinCommand { get; }

        
        /// <summary>
        /// CONSTRUTOR
        /// </summary>
        /// <param name="httpClient"></param>
        
        public RastreabilidadeViewModel(HttpClient httpClient)
        {
            _httpClient = httpClient;
            PesquisarVinCommand = new RelayCommand(async p => await PesquisarVinAsync());
        }

        
        /// <summary>
        /// CARREGAR VEÍCULOS
        /// </summary>
        /// <returns></returns>
        
        public async Task CarregarVeiculosAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/dados/veiculos");

                if (!response.IsSuccessStatusCode)
                {
                    Debug.WriteLine($"[ERRO CARREGAR VEÍCULOS] HTTP {(int)response.StatusCode}");
                    return;
                }

                var json = await response.Content.ReadAsStringAsync();
                var veiculos = JsonSerializer.Deserialize<List<VeiculoModel>>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (veiculos != null)
                {
                    ListaVeiculos = new ObservableCollection<VeiculoModel>(veiculos);
                    TotalVeiculos = veiculos.Count.ToString();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERRO INESPERADO VEÍCULOS] {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}");
            }
        }

        
        /// <summary>
        /// PESQUISAR VIN
        /// </summary>
        /// <returns></returns>
        
        private async Task PesquisarVinAsync()
        {
            if (string.IsNullOrWhiteSpace(PesquisaVin) || PesquisaVin.Length != 17)
            {
                MessageBox.Show("Introduza um VIN válido com 17 caracteres.", "Aviso",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var response = await _httpClient.GetAsync($"api/dados/veiculos/validar-vin/{PesquisaVin}");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    using var doc = System.Text.Json.JsonDocument.Parse(json);
                    var veiculoJson = doc.RootElement.GetProperty("veiculo").GetRawText();

                    var content = new System.Net.Http.StringContent(
                        veiculoJson, System.Text.Encoding.UTF8, "application/json");
                    var resSalvar = await _httpClient.PostAsync("api/dados/veiculos", content);

                    if (resSalvar.IsSuccessStatusCode)
                    {
                        MessageBox.Show("Veículo IVECO rastreado e guardado no Ledger!",
                            "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                        PesquisaVin = "";
                        await CarregarVeiculosAsync();
                    }
                    else if (resSalvar.StatusCode == System.Net.HttpStatusCode.Conflict)
                    {
                        MessageBox.Show("Veículo autêntico, mas já estava registado no sistema.",
                            "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                    else
                    {
                        var erro = await resSalvar.Content.ReadAsStringAsync();
                        Debug.WriteLine($"[ERRO SALVAR VIN] HTTP {(int)resSalvar.StatusCode} -> {erro}");
                        MessageBox.Show("Não foi possível guardar o veículo.\nTente novamente.",
                            "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    var erro = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine($"[ERRO VALIDAR VIN] HTTP {(int)response.StatusCode} -> {erro}");
                    MessageBox.Show("Este VIN não pertence a um veículo Iveco válido.",
                        "Acesso Negado", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"[ERRO CONEXÃO VIN] {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}");

                /// IMPLEMENTAÇÃO OFFLINE-SAFE
                MessageBox.Show("Modo Offline ativado. O veículo será guardado localmente e sincronizado quando a rede for restabelecida.",
                    "Aviso de Contingência", MessageBoxButton.OK, MessageBoxImage.Information);

                /// Aqui chamaria o seu serviço de SQLite local:
                /// await _sqliteService.SalvarVeiculoLocalAsync(new VeiculoModel { Vin = PesquisaVin, is_sintonizado = false });

                PesquisaVin = "";
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERRO INESPERADO VIN] {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}");
                MessageBox.Show("Ocorreu um erro inesperado.\nTente novamente ou contacte o suporte.",
                    "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}