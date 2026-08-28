using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using WpfIveco.Data;
using WpfIveco.DTO;
using WpfIveco.Models;
using WpfIveco.ViewModels;

namespace WpfIveco.ViewModel
{
    public class PecasViewModel : ViewModelBase
    {
        private readonly HttpClient _httpClient;
        private readonly LocalDatabaseService _localDb;

        private ObservableCollection<string> _listaVins = new();
        public ObservableCollection<string> ListaVins
        {
            get => _listaVins;
            set { _listaVins = value; OnPropertyChanged(nameof(ListaVins)); }
        }

        private string _vinSelecionado = "";
        public string VinSelecionado
        {
            get => _vinSelecionado;
            set { _vinSelecionado = value; OnPropertyChanged(nameof(VinSelecionado)); }
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

        private ObservableCollection<FornecedorModel> _listaFornecedores = new();
        public ObservableCollection<FornecedorModel> ListaFornecedores
        {
            get => _listaFornecedores;
            set { _listaFornecedores = value; OnPropertyChanged(); }
        }

        private FornecedorModel _fornecedorSelecionado;
        public FornecedorModel FornecedorSelecionado
        {
            get => _fornecedorSelecionado;
            set { _fornecedorSelecionado = value; OnPropertyChanged(nameof(FornecedorSelecionado)); }
        }

        public ICommand AdicionarPecaManualCommand { get; }

        public PecasViewModel(HttpClient httpClient)
        {
            App.LogInfo("Construtor", "PECAS");
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _localDb = new LocalDatabaseService();
            AdicionarPecaManualCommand = new RelayCommand(async p => await AdicionarPecaAsync());
        }

        public async Task CarregarVinsAsync()
        {
            App.LogInfo("CarregarVinsAsync iniciado", "PECAS");
            try
            {
                var response = await _httpClient.GetAsync("api/dados/veiculos");
                App.LogInfo($"GET VINs → {(int)response.StatusCode}", "PECAS");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var veiculos = JsonSerializer.Deserialize<List<VeiculoModel>>(json,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (veiculos != null && veiculos.Any())
                    {
                        var vinList = veiculos.Select(v => v.Vin).Where(vin => !string.IsNullOrEmpty(vin)).Distinct().ToList();
                        ListaVins = new ObservableCollection<string>(vinList);
                        if (ListaVins.Any()) VinSelecionado = ListaVins.First();
                        App.LogInfo($"{vinList.Count} VINs carregados da API", "PECAS");
                        return;
                    }
                }

                App.LogWarning("API indisponível, usando VINs locais", "PECAS");
                await CarregarVinsLocaisAsync();
            }
            catch
            {
                App.LogError("Erro ao carregar VINs – carregando locais", "PECAS");
                await CarregarVinsLocaisAsync();
            }
        }

        public async Task CarregarFornecedoresAsync()
        {
            App.LogInfo("CarregarFornecedoresAsync iniciado", "PECAS");
            try
            {
                var response = await _httpClient.GetAsync("api/dados/fornecedores");
                App.LogInfo($"GET Fornecedores → {(int)response.StatusCode}", "PECAS");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var fornecedores = JsonSerializer.Deserialize<List<FornecedorModel>>(json,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (fornecedores != null && fornecedores.Any())
                    {
                        ListaFornecedores = new ObservableCollection<FornecedorModel>(fornecedores);
                        if (ListaFornecedores.Any()) FornecedorSelecionado = ListaFornecedores.First();
                        App.LogInfo($"{fornecedores.Count} fornecedores carregados da API", "PECAS");
                        return;
                    }
                }

                App.LogWarning("API indisponível, usando fornecedores locais", "PECAS");
                await CarregarFornecedoresLocaisAsync();
            }
            catch
            {
                App.LogError("Erro ao carregar fornecedores – carregando locais", "PECAS");
                await CarregarFornecedoresLocaisAsync();
            }
        }

        public async Task CarregarPecasAsync()
        {
            App.LogInfo("CarregarPecasAsync iniciado", "PECAS");
            try
            {
                var response = await _httpClient.GetAsync("api/dados/componentes");
                App.LogInfo($"GET Peças → {(int)response.StatusCode}", "PECAS");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var componentesApi = JsonSerializer.Deserialize<List<VeiculoComponenteApiDto>>(json,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (componentesApi != null && componentesApi.Any())
                    {
                        var listaMapeada = componentesApi
                            .Select(c => new PecaModel
                            {
                                NomePeca = c.NomePeca,
                                VinAssociado = c.Fk_Veiculo_Vin,
                                PesoKg = c.PesoKg,
                                FornecedorId = c.Fk_Fornecedor_Id
                            })
                            .Reverse()
                            .ToList();

                        ListaPecas = new ObservableCollection<PecaModel>(listaMapeada);
                        App.LogInfo($"{listaMapeada.Count} peças carregadas da API", "PECAS");
                        _ = _localDb.SalvarPecasAsync(listaMapeada);
                        return;
                    }
                }

                App.LogWarning("API indisponível, usando peças locais", "PECAS");
                await CarregarPecasLocaisAsync();
            }
            catch
            {
                App.LogError("Erro ao carregar peças – carregando locais", "PECAS");
                await CarregarPecasLocaisAsync();
            }
        }

        private async Task CarregarVinsLocaisAsync()
        {
            var veiculos = await _localDb.GetVeiculosAsync();
            if (veiculos.Any())
            {
                var vinList = veiculos.Select(v => v.Vin).Where(vin => !string.IsNullOrEmpty(vin)).Distinct().ToList();
                ListaVins = new ObservableCollection<string>(vinList);
                if (ListaVins.Any()) VinSelecionado = ListaVins.First();
                App.LogInfo($"{vinList.Count} VINs carregados do SQLite", "PECAS");
            }
            else
            {
                ListaVins = new ObservableCollection<string>();
            }
        }

        private async Task CarregarFornecedoresLocaisAsync()
        {
            var fornecedores = await _localDb.GetFornecedoresAsync();
            if (fornecedores.Any())
            {
                ListaFornecedores = new ObservableCollection<FornecedorModel>(fornecedores);
                if (ListaFornecedores.Any()) FornecedorSelecionado = ListaFornecedores.First();
                App.LogInfo($"{fornecedores.Count} fornecedores carregados do SQLite", "PECAS");
            }
            else
            {
                ListaFornecedores = new ObservableCollection<FornecedorModel>();
            }
        }

        private async Task CarregarPecasLocaisAsync()
        {
            var pecas = await _localDb.GetPecasAsync();
            if (pecas.Any())
            {
                ListaPecas = new ObservableCollection<PecaModel>(pecas);
                App.LogInfo($"{pecas.Count} peças carregadas do SQLite", "PECAS");
            }
            else
            {
                ListaPecas = new ObservableCollection<PecaModel>();
            }
        }

        private async Task AdicionarPecaAsync()
        {
            App.LogInfo("AdicionarPecaAsync iniciado", "PECAS");

            // Validações
            if (string.IsNullOrWhiteSpace(VinSelecionado))
            {
                App.LogWarning("VIN não selecionado", "PECAS");
                MessageBox.Show("Selecione um veículo.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (VinSelecionado.Length != 17)
            {
                App.LogWarning($"VIN inválido (tamanho {VinSelecionado.Length})", "PECAS");
                MessageBox.Show("O VIN deve ter exatamente 17 caracteres.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(NovaPecaNome))
            {
                App.LogWarning("Nome da peça vazio", "PECAS");
                MessageBox.Show("Preencha o nome da peça.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (NovaPecaPesoKg <= 0)
            {
                App.LogWarning($"Peso inválido: {NovaPecaPesoKg}", "PECAS");
                MessageBox.Show("Informe um peso maior que zero (kg).", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (FornecedorSelecionado == null || string.IsNullOrWhiteSpace(FornecedorSelecionado.Id))
            {
                App.LogWarning("Fornecedor não selecionado", "PECAS");
                MessageBox.Show("Selecione um fornecedor.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var novaPeca = new PecaModel
            {
                NomePeca = NovaPecaNome,
                VinAssociado = VinSelecionado,
                PesoKg = NovaPecaPesoKg,
                FornecedorId = FornecedorSelecionado.Id
            };

            App.LogInfo($"Enviando peça: {NovaPecaNome} (VIN: {VinSelecionado}, Fornecedor: {FornecedorSelecionado.Nome})", "PECAS");

            // CORREÇÃO: Enviar string vazia em vez de null
            var dtoEnvio = new
            {
                Id = Guid.NewGuid().ToString().Substring(0, 8),
                NomePeca = NovaPecaNome,
                Fk_Veiculo_Vin = VinSelecionado,
                Fk_LoteMateriaPrima_Id = string.Empty, // <-- ALTERADO de null para string.Empty
                PesoKg = NovaPecaPesoKg,
                Fk_Fornecedor_Id = FornecedorSelecionado.Id
            };

            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/dados/componentes", dtoEnvio);
                App.LogInfo($"POST Peça → {(int)response.StatusCode}", "PECAS");

                if (response.IsSuccessStatusCode)
                {
                    ListaPecas.Insert(0, novaPeca);
                    _ = _localDb.SalvarPecasAsync(new List<PecaModel> { novaPeca });

                    NovaPecaNome = "";
                    NovaPecaPesoKg = 0;
                    App.LogInfo("Peça registrada com sucesso!", "PECAS");
                    MessageBox.Show("Peça registrada com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    var erroContent = await response.Content.ReadAsStringAsync();
                    App.LogWarning($"API retornou erro: {response.StatusCode} - {erroContent}", "PECAS");
                    // Tenta salvar offline
                    var salvouLocal = await _localDb.SalvarPecaOfflineAsync(novaPeca);
                    if (salvouLocal)
                    {
                        ListaPecas.Insert(0, novaPeca);
                        NovaPecaNome = "";
                        NovaPecaPesoKg = 0;
                        MessageBox.Show("Peça salva OFFLINE! Será sincronizada quando a internet voltar.", "Modo Offline", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show($"Erro ao salvar peça (online: {response.StatusCode} - {erroContent})", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                App.LogError($"Erro de conexão ao registrar peça: {ex.Message}", "PECAS");
                var salvouLocal = await _localDb.SalvarPecaOfflineAsync(novaPeca);
                if (salvouLocal)
                {
                    ListaPecas.Insert(0, novaPeca);
                    NovaPecaNome = "";
                    NovaPecaPesoKg = 0;
                    MessageBox.Show("Peça salva OFFLINE! Será sincronizada quando a internet voltar.", "Modo Offline", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    MessageBox.Show($"Erro de conexão. Não foi possível salvar (offline). Detalhe: {ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}