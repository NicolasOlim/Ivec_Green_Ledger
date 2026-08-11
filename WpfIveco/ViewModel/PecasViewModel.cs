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
    /// <summary>
    /// ViewModel para a tela de gestão de peças e componentes.
    /// Gerencia a lista de VINs, fornecedores, peças, e o registro de novas peças.
    /// Possui fallback offline via SQLite.
    /// </summary>
    public class PecasViewModel : ViewModelBase
    {
        private readonly HttpClient _httpClient;
        private readonly LocalDatabaseService _localDb;

        // ============================================================
        // PROPRIEDADES (BINDINGS)
        // ============================================================

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

        // ============================================================
        // COMANDOS
        // ============================================================

        public ICommand AdicionarPecaManualCommand { get; }

        // ============================================================
        // CONSTRUTOR
        // ============================================================

        public PecasViewModel(HttpClient httpClient)
        {
            App.LogInfo("Construtor", "PECAS");
            _httpClient = httpClient;
            _localDb = new LocalDatabaseService();
            AdicionarPecaManualCommand = new RelayCommand(async p => await AdicionarPecaAsync());
        }

        // ============================================================
        // MÉTODOS PÚBLICOS (CARREGAMENTO COM FALLBACK)
        // ============================================================

        /// <summary>
        /// Carrega a lista de VINs disponíveis (para o ComboBox).
        /// Prioriza a API, com fallback para o SQLite.
        /// </summary>
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

                // Fallback local
                App.LogWarning("API indisponível, usando VINs locais", "PECAS");
                await CarregarVinsLocaisAsync();
            }
            catch
            {
                App.LogError("Erro ao carregar VINs – carregando locais", "PECAS");
                await CarregarVinsLocaisAsync();
            }
        }

        /// <summary>
        /// Carrega a lista de fornecedores para o ComboBox.
        /// Prioriza a API, com fallback para o SQLite.
        /// </summary>
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

        /// <summary>
        /// Carrega a lista de peças (para exibição).
        /// Prioriza a API, com fallback para o SQLite.
        /// </summary>
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
                            .Reverse() // Mais recentes primeiro
                            .ToList();

                        ListaPecas = new ObservableCollection<PecaModel>(listaMapeada);
                        App.LogInfo($"{listaMapeada.Count} peças carregadas da API", "PECAS");

                        // Salva em background no SQLite
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

        // ============================================================
        // MÉTODOS PRIVADOS (CARREGAMENTO LOCAL)
        // ============================================================

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

        // ============================================================
        // OPERAÇÃO DE ADIÇÃO (COM FALLBACK OFFLINE)
        // ============================================================

        /// <summary>
        /// Registra uma nova peça associada a um VIN e fornecedor.
        /// Tenta salvar na API; se falhar, salva localmente no SQLite.
        /// </summary>
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

            if (string.IsNullOrWhiteSpace(NovaPecaNome))
            {
                App.LogWarning("Nome da peça vazio", "PECAS");
                MessageBox.Show("Preencha o nome da peça.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (NovaPecaPesoKg <= 0)
            {
                App.LogWarning($"Peso inválido: {NovaPecaPesoKg}", "PECAS");
                MessageBox.Show("Informe um peso > 0 kg.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
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

            // DTO para envio à API (inclui campos específicos do backend)
            var dtoEnvio = new
            {
                Id = Guid.NewGuid().ToString().Substring(0, 8),
                NomePeca = NovaPecaNome,
                Fk_Veiculo_Vin = VinSelecionado,
                Fk_LoteMateriaPrima_Id = "LOTE-MANUAL-" + DateTime.Now.ToString("yyyyMMdd"),
                PesoKg = NovaPecaPesoKg,
                Fk_Fornecedor_Id = FornecedorSelecionado.Id
            };

            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/dados/componentes", dtoEnvio);
                App.LogInfo($"POST Peça → {(int)response.StatusCode}", "PECAS");

                if (response.IsSuccessStatusCode)
                {
                    // Adiciona à lista local (UI)
                    ListaPecas.Insert(0, novaPeca);
                    // Salva em background no SQLite
                    _ = _localDb.SalvarPecasAsync(new List<PecaModel> { novaPeca });

                    NovaPecaNome = "";
                    NovaPecaPesoKg = 0;
                    App.LogInfo("Peça registrada com sucesso!", "PECAS");
                    MessageBox.Show("Peça registrada com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    // Se a API rejeitou, tenta salvar localmente (offline)
                    App.LogWarning("API falhou. Salvando peça localmente.", "PECAS");
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
                        MessageBox.Show("Erro ao salvar peça (online e offline).", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                // Exceção de rede – fallback offline
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
                    MessageBox.Show("Erro de conexão. Não foi possível salvar (offline).", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
}