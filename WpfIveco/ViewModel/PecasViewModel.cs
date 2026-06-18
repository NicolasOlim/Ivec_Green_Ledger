using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using WpfIveco.Models;
using WpfIveco.ViewModels;

namespace WpfIveco.ViewModel
{
    public class PecasViewModel : ViewModelBase
    {
        private readonly HttpClient _httpClient;

        /// <summary>
        /// PROPRIEDADES
        /// </summary>

        // Lista de VINs para o ComboBox
        private ObservableCollection<string> _listaVins = new();
        public ObservableCollection<string> ListaVins
        {
            get => _listaVins;
            set
            {
                _listaVins = value;
                OnPropertyChanged(nameof(ListaVins));
                Debug.WriteLine($"[ListaVins] Atualizada com {_listaVins?.Count ?? 0} itens");
            }
        }

        // VIN selecionado no ComboBox
        private string _vinSelecionado = "";
        public string VinSelecionado
        {
            get => _vinSelecionado;
            set
            {
                _vinSelecionado = value;
                OnPropertyChanged(nameof(VinSelecionado));
                Debug.WriteLine($"[VinSelecionado] = {value}");
            }
        }

        private string _novaPecaNome = "";
        public string NovaPecaNome
        {
            get => _novaPecaNome;
            set { _novaPecaNome = value; OnPropertyChanged(); }
        }

        private double _novaPecaPesoKg = 0;
        public double NovaPecaPesoKg
        {
            get => _novaPecaPesoKg;
            set { _novaPecaPesoKg = value; OnPropertyChanged(); }
        }

        private ObservableCollection<PecaModel> _listaPecas = new();
        public ObservableCollection<PecaModel> ListaPecas
        {
            get => _listaPecas;
            set { _listaPecas = value; OnPropertyChanged(); }
        }

        // Lista de Fornecedores para o ComboBox
        private ObservableCollection<FornecedorModel> _listaFornecedores = new();
        public ObservableCollection<FornecedorModel> ListaFornecedores
        {
            get => _listaFornecedores;
            set { _listaFornecedores = value; OnPropertyChanged(); }
        }

        // Fornecedor selecionado no ComboBox
        private FornecedorModel _fornecedorSelecionado;
        public FornecedorModel FornecedorSelecionado
        {
            get => _fornecedorSelecionado;
            set
            {
                _fornecedorSelecionado = value;
                OnPropertyChanged(nameof(FornecedorSelecionado));
                Debug.WriteLine($"[FornecedorSelecionado] = {value?.Nome ?? "null"} (ID: {value?.Id ?? "null"})");
            }
        }

        /// <summary>
        /// COMANDOS
        /// </summary>
        public ICommand AdicionarPecaManualCommand { get; }

        /// <summary>
        /// CONSTRUTOR
        /// </summary>
        public PecasViewModel(HttpClient httpClient)
        {
            _httpClient = httpClient;
            AdicionarPecaManualCommand = new RelayCommand(async p => await AdicionarPecaAsync());
        }

        /// <summary>
        /// CARREGAR VINS DISPONÍVEIS
        /// </summary>
        public async Task CarregarVinsAsync()
        {
            try
            {
                Debug.WriteLine("[CARREGAR VINS] Iniciando...");
                var response = await _httpClient.GetAsync("api/dados/veiculos");
                Debug.WriteLine($"[CARREGAR VINS] Status: {response.StatusCode}");

                if (!response.IsSuccessStatusCode)
                {
                    Debug.WriteLine($"[ERRO CARREGAR VINS] HTTP {(int)response.StatusCode}");
                    return;
                }

                var json = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"[CARREGAR VINS] JSON recebido (primeiros 200 chars): {json.Substring(0, Math.Min(200, json.Length))}");

                var veiculos = JsonSerializer.Deserialize<List<VeiculoModel>>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                Debug.WriteLine($"[CARREGAR VINS] Total de veículos: {veiculos?.Count ?? 0}");

                if (veiculos != null && veiculos.Any())
                {
                    var vinList = veiculos
                        .Select(v => v.Vin)
                        .Where(vin => !string.IsNullOrEmpty(vin))
                        .Distinct()
                        .ToList();

                    Debug.WriteLine($"[CARREGAR VINS] Lista de VINs: {string.Join(", ", vinList)}");

                    // Recria a coleção
                    ListaVins = new ObservableCollection<string>(vinList);

                    // Define o primeiro como selecionado
                    if (ListaVins.Any())
                        VinSelecionado = ListaVins.First();
                }
                else
                {
                    Debug.WriteLine("[CARREGAR VINS] Nenhum veículo encontrado.");
                    ListaVins = new ObservableCollection<string>();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERRO INESPERADO VINS] {ex.GetType().Name}: {ex.Message}");
                MessageBox.Show($"Erro ao carregar VINs: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// CARREGAR FORNECEDORES PARA O COMBOBOX
        /// </summary>
        public async Task CarregarFornecedoresAsync()
        {
            try
            {
                Debug.WriteLine("[CARREGAR FORNECEDORES] Iniciando...");
                var response = await _httpClient.GetAsync("api/dados/fornecedores");
                Debug.WriteLine($"[CARREGAR FORNECEDORES] Status: {response.StatusCode}");

                if (!response.IsSuccessStatusCode)
                {
                    Debug.WriteLine($"[ERRO CARREGAR FORNECEDORES] HTTP {(int)response.StatusCode}");
                    return;
                }

                var json = await response.Content.ReadAsStringAsync();
                Debug.WriteLine($"[CARREGAR FORNECEDORES] JSON recebido (primeiros 200 chars): {json.Substring(0, Math.Min(200, json.Length))}");

                var fornecedores = JsonSerializer.Deserialize<List<FornecedorModel>>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                Debug.WriteLine($"[CARREGAR FORNECEDORES] Total de fornecedores: {fornecedores?.Count ?? 0}");

                if (fornecedores != null && fornecedores.Any())
                {
                    ListaFornecedores = new ObservableCollection<FornecedorModel>(fornecedores);

                    // Define o primeiro como selecionado
                    if (ListaFornecedores.Any())
                        FornecedorSelecionado = ListaFornecedores.First();
                }
                else
                {
                    Debug.WriteLine("[CARREGAR FORNECEDORES] Nenhum fornecedor encontrado.");
                    ListaFornecedores = new ObservableCollection<FornecedorModel>();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERRO INESPERADO FORNECEDORES] {ex.GetType().Name}: {ex.Message}");
                MessageBox.Show($"Erro ao carregar fornecedores: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// CARREGAR PEÇAS
        /// </summary>
        public async Task CarregarPecasAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/dados/componentes");
                if (!response.IsSuccessStatusCode)
                {
                    Debug.WriteLine($"[ERRO CARREGAR PEÇAS] HTTP {(int)response.StatusCode}");
                    return;
                }

                var json = await response.Content.ReadAsStringAsync();
                var componentesApi = JsonSerializer.Deserialize<List<VeiculoComponenteApi>>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (componentesApi != null)
                {
                    var listaMapeada = componentesApi
                        .Select(c => new PecaModel
                        {
                            NomePeca = c.NomePeca,
                            VinAssociado = c.Fk_Veiculo_Vin,
                            PesoKg = c.PesoKg,
                            FornecedorId = c.Fk_Fornecedor_Id // <-- ADICIONADO
                        })
                        .Reverse()
                        .ToList();

                    ListaPecas = new ObservableCollection<PecaModel>(listaMapeada);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERRO INESPERADO PEÇAS] {ex.GetType().Name}: {ex.Message}");
            }
        }

        /// <summary>
        /// ADICIONAR PEÇA MANUAL
        /// </summary>
        private async Task AdicionarPecaAsync()
        {
            // VALIDAÇÕES
            if (string.IsNullOrWhiteSpace(VinSelecionado))
            {
                MessageBox.Show("Selecione um veículo VIN válido.", "Aviso",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(NovaPecaNome))
            {
                MessageBox.Show("Preencha o nome da peça.", "Aviso",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (NovaPecaPesoKg <= 0)
            {
                MessageBox.Show("Informe um peso válido (maior que 0 kg).", "Aviso",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (FornecedorSelecionado == null || string.IsNullOrWhiteSpace(FornecedorSelecionado.Id))
            {
                MessageBox.Show("Selecione um fornecedor válido.", "Aviso",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // PREPARA O OBJETO A ENVIAR (INCLUINDO PESO E FORNECEDOR)
            var novaPeca = new
            {
                Id = Guid.NewGuid().ToString().Substring(0, 8),
                NomePeca = NovaPecaNome,
                Fk_Veiculo_Vin = VinSelecionado,
                Fk_LoteMateriaPrima_Id = "LOTE-MANUAL-" + DateTime.Now.ToString("yyyyMMdd"),
                PesoKg = NovaPecaPesoKg,
                Fk_Fornecedor_Id = FornecedorSelecionado.Id // <-- CAMPO ADICIONADO
            };

            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/dados/componentes", novaPeca);

                if (response.IsSuccessStatusCode)
                {
                    // ADICIONA À LISTA LOCAL (COM O PESO E FORNECEDOR)
                    ListaPecas.Insert(0, new PecaModel
                    {
                        NomePeca = NovaPecaNome,
                        VinAssociado = VinSelecionado,
                        PesoKg = NovaPecaPesoKg,
                        FornecedorId = FornecedorSelecionado.Id
                    });

                    // LIMPA OS CAMPOS
                    NovaPecaNome = "";
                    NovaPecaPesoKg = 0;

                    MessageBox.Show("Peça registada com sucesso!", "Sucesso",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    var erro = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine($"[ERRO ADICIONAR PEÇA] HTTP {(int)response.StatusCode} -> {erro}");
                    MessageBox.Show("Não foi possível registar a peça.\nTente novamente.",
                        "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"[ERRO CONEXÃO PEÇA] {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}");
                MessageBox.Show("Não foi possível conectar ao servidor.\nVerifique a sua ligação.",
                    "Erro de Ligação", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERRO INESPERADO PEÇA] {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}");
                MessageBox.Show("Ocorreu um erro inesperado.\nTente novamente ou contacte o suporte.",
                    "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}