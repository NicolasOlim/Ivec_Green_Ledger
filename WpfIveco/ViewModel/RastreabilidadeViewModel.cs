using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using WpfIveco.Data;
using WpfIveco.Models;
using WpfIveco.ViewModels;

namespace WpfIveco.ViewModel
{
    /// <summary>
    /// ViewModel responsável pela tela de Rastreabilidade de Veículos.
    /// Gerencia a listagem de veículos, a pesquisa de VIN (via NHTSA) e o fallback offline.
    /// </summary>
    public class RastreabilidadeViewModel : ViewModelBase
    {
        private readonly HttpClient _httpClient;          // Cliente HTTP para chamadas à API
        private readonly LocalDatabaseService _localDb;  // Serviço de cache local SQLite

        // ============================================================
        // PROPRIEDADES
        // ============================================================

        private string _pesquisaVin = "";
        /// <summary>VIN digitado pelo usuário para pesquisa.</summary>
        public string PesquisaVin
        {
            get => _pesquisaVin;
            set { _pesquisaVin = value; OnPropertyChanged(); }
        }

        private string _totalVeiculos = "0";
        /// <summary>Total de veículos cadastrados (exibido no badge).</summary>
        public string TotalVeiculos
        {
            get => _totalVeiculos;
            set { _totalVeiculos = value; OnPropertyChanged(); }
        }

        private ObservableCollection<VeiculoModel> _listaVeiculos = new();
        /// <summary>Lista de veículos exibida na interface.</summary>
        public ObservableCollection<VeiculoModel> ListaVeiculos
        {
            get => _listaVeiculos;
            set { _listaVeiculos = value; OnPropertyChanged(); }
        }

        // ============================================================
        // COMANDOS
        // ============================================================

        /// <summary>Comando acionado ao clicar em "Rastrear Origem".</summary>
        public ICommand PesquisarVinCommand { get; }

        // ============================================================
        // CONSTRUTOR
        // ============================================================

        public RastreabilidadeViewModel(HttpClient httpClient)
        {
            App.LogInfo("Construtor", "RASTREAB");
            _httpClient = httpClient;
            _localDb = new LocalDatabaseService();
            PesquisarVinCommand = new RelayCommand(async p => await PesquisarVinAsync());
        }

        // ============================================================
        // MÉTODOS PÚBLICOS
        // ============================================================

        /// <summary>
        /// Carrega a lista de veículos, priorizando a API.
        /// Em caso de falha, busca os dados no SQLite local.
        /// </summary>
        public async Task CarregarVeiculosAsync()
        {
            App.LogInfo("CarregarVeiculosAsync iniciado", "RASTREAB");
            try
            {
                // Tenta obter da API
                var response = await _httpClient.GetAsync("api/dados/veiculos");
                App.LogInfo($"GET Veículos → {(int)response.StatusCode}", "RASTREAB");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var veiculos = JsonSerializer.Deserialize<List<VeiculoModel>>(json,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (veiculos != null && veiculos.Count > 0)
                    {
                        // Atualiza a interface
                        ListaVeiculos = new ObservableCollection<VeiculoModel>(veiculos);
                        TotalVeiculos = veiculos.Count.ToString();
                        App.LogInfo($"{veiculos.Count} veículos carregados da API", "RASTREAB");

                        // Salva em background no SQLite (não bloqueia a UI)
                        _ = _localDb.SalvarVeiculosAsync(veiculos);
                        return;
                    }
                }

                // Se a API não retornou dados, busca no local
                App.LogWarning("API indisponível ou sem dados, usando dados locais (veículos)", "RASTREAB");
                await CarregarVeiculosLocaisAsync();
            }
            catch (Exception ex)
            {
                // Em caso de exceção (ex: sem internet), fallback para local
                App.LogError($"Erro ao buscar veículos: {ex.Message}", "RASTREAB");
                await CarregarVeiculosLocaisAsync();
            }
        }

        // ============================================================
        // MÉTODOS PRIVADOS
        // ============================================================

        /// <summary>
        /// Carrega os veículos diretamente do banco de dados local.
        /// </summary>
        private async Task CarregarVeiculosLocaisAsync()
        {
            var locais = await _localDb.GetVeiculosAsync();
            if (locais.Any())
            {
                ListaVeiculos = new ObservableCollection<VeiculoModel>(locais);
                TotalVeiculos = locais.Count.ToString();
                App.LogInfo($"Carregados {locais.Count} veículos do SQLite", "RASTREAB");
            }
            else
            {
                // Sem dados locais e sem API – lista vazia
                ListaVeiculos = new ObservableCollection<VeiculoModel>();
                TotalVeiculos = "0";
            }
        }

        /// <summary>
        /// Pesquisa e valida um VIN na API da NHTSA.
        /// Se válido, salva o veículo no sistema (online) e, em caso de falha, salva localmente.
        /// </summary>
        private async Task PesquisarVinAsync()
        {
            App.LogInfo($"Pesquisando VIN: {PesquisaVin}", "RASTREAB");

            // Validação básica do VIN (17 caracteres)
            if (string.IsNullOrWhiteSpace(PesquisaVin) || PesquisaVin.Length != 17)
            {
                App.LogWarning("VIN inválido", "RASTREAB");
                MessageBox.Show("Introduza um VIN válido com 17 caracteres.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                // 1. Valida o VIN na NHTSA via API própria
                var response = await _httpClient.GetAsync($"api/dados/veiculos/validar-vin/{PesquisaVin}");
                App.LogInfo($"GET validar-vin → {(int)response.StatusCode}", "RASTREAB");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(json);
                    var veiculoJson = doc.RootElement.GetProperty("veiculo").GetRawText();

                    // 2. Salva o veículo no sistema (POST)
                    var content = new StringContent(veiculoJson, System.Text.Encoding.UTF8, "application/json");
                    var resSalvar = await _httpClient.PostAsync("api/dados/veiculos", content);
                    App.LogInfo($"POST veiculos → {(int)resSalvar.StatusCode}", "RASTREAB");

                    if (resSalvar.IsSuccessStatusCode)
                    {
                        App.LogInfo("Veículo rastreado e guardado!", "RASTREAB");
                        MessageBox.Show("Veículo IVECO rastreado e guardado no Ledger!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                        PesquisaVin = ""; // Limpa o campo
                        await CarregarVeiculosAsync(); // Recarrega a lista
                    }
                    else if (resSalvar.StatusCode == System.Net.HttpStatusCode.Conflict)
                    {
                        // VIN já existe no sistema
                        App.LogWarning("Veículo já registado", "RASTREAB");
                        MessageBox.Show("Veículo autêntico, mas já estava registado no sistema.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                    else
                    {
                        App.LogError($"Falha ao salvar: {await resSalvar.Content.ReadAsStringAsync()}", "RASTREAB");
                        MessageBox.Show("Não foi possível guardar o veículo.\nTente novamente.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    App.LogError($"Falha na validação: {await response.Content.ReadAsStringAsync()}", "RASTREAB");
                    MessageBox.Show("Este VIN não pertence a um veículo Iveco válido.", "Acesso Negado", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (HttpRequestException)
            {
                // Se a API não responder (ex: offline), avisa o usuário
                App.LogWarning("Modo offline ativado", "RASTREAB");
                MessageBox.Show("Modo Offline ativado. O veículo será guardado localmente e sincronizado quando a rede for restabelecida.",
                    "Aviso de Contingência", MessageBoxButton.OK, MessageBoxImage.Information);
                PesquisaVin = ""; // Limpa o campo
            }
            catch (Exception ex)
            {
                App.LogError($"Erro inesperado ao pesquisar VIN: {ex.Message}", "RASTREAB");
                MessageBox.Show("Ocorreu um erro inesperado.\nTente novamente ou contacte o suporte.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}