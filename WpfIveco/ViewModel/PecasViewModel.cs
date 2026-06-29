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
using WpfIveco.DTO;
using WpfIveco.Models;
using WpfIveco.ViewModels;

namespace WpfIveco.ViewModel
{
    /// <summary>
    /// ViewModel para a gestão de peças e componentes.
    /// Gerencia listas de VINs, fornecedores, peças e o registro de novas peças.
    /// </summary>
    public class PecasViewModel : ViewModelBase
    {
        private readonly HttpClient _httpClient;

        /// ============================================================
        /// PROPRIEDADES
        /// ============================================================

        private ObservableCollection<string> _listaVins = new();
        /// <summary>Lista de VINs disponíveis para seleção.</summary>
        public ObservableCollection<string> ListaVins
        {
            get => _listaVins;
            set { _listaVins = value; OnPropertyChanged(nameof(ListaVins)); }
        }

        private string _vinSelecionado = "";
        /// <summary>VIN selecionado no ComboBox.</summary>
        public string VinSelecionado
        {
            get => _vinSelecionado;
            set { _vinSelecionado = value; OnPropertyChanged(nameof(VinSelecionado)); }
        }

        private string _novaPecaNome = "";
        /// <summary>Nome da nova peça a ser registrada.</summary>
        public string NovaPecaNome
        {
            get => _novaPecaNome;
            set { _novaPecaNome = value; OnPropertyChanged(); }
        }

        private double _novaPecaPesoKg = 0;
        /// <summary>Peso da nova peça em kg.</summary>
        public double NovaPecaPesoKg
        {
            get => _novaPecaPesoKg;
            set { _novaPecaPesoKg = value; OnPropertyChanged(); }
        }

        private ObservableCollection<PecaModel> _listaPecas = new();
        /// <summary>Lista de peças já registradas.</summary>
        public ObservableCollection<PecaModel> ListaPecas
        {
            get => _listaPecas;
            set { _listaPecas = value; OnPropertyChanged(); }
        }

        private ObservableCollection<FornecedorModel> _listaFornecedores = new();
        /// <summary>Lista de fornecedores disponíveis.</summary>
        public ObservableCollection<FornecedorModel> ListaFornecedores
        {
            get => _listaFornecedores;
            set { _listaFornecedores = value; OnPropertyChanged(); }
        }

        private FornecedorModel _fornecedorSelecionado;
        /// <summary>Fornecedor selecionado para a nova peça.</summary>
        public FornecedorModel FornecedorSelecionado
        {
            get => _fornecedorSelecionado;
            set { _fornecedorSelecionado = value; OnPropertyChanged(nameof(FornecedorSelecionado)); }
        }

        /// ============================================================
        /// COMANDOS
        /// ============================================================

        /// <summary>Comando para registrar uma nova peça.</summary>
        public ICommand AdicionarPecaManualCommand { get; }

        /// ============================================================
        /// CONSTRUTOR
        /// ============================================================

        /// <summary>Inicializa o ViewModel com o HttpClient.</summary>
        public PecasViewModel(HttpClient httpClient)
        {
            App.LogInfo("Construtor", "PECAS");
            _httpClient = httpClient;
            AdicionarPecaManualCommand = new RelayCommand(async p => await AdicionarPecaAsync());
        }

        /// ============================================================
        /// MÉTODOS PÚBLICOS
        /// ============================================================

        /// <summary>Carrega a lista de VINs disponíveis da API.</summary>
        public async Task CarregarVinsAsync()
        {
            App.LogInfo("CarregarVinsAsync iniciado", "PECAS");
            try
            {
                var response = await _httpClient.GetAsync("api/dados/veiculos");
                App.LogInfo($"GET VINs → {(int)response.StatusCode}", "PECAS");
                if (!response.IsSuccessStatusCode) return;

                var json = await response.Content.ReadAsStringAsync();
                var veiculos = JsonSerializer.Deserialize<List<VeiculoModel>>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (veiculos != null && veiculos.Any())
                {
                    var vinList = veiculos.Select(v => v.Vin).Where(vin => !string.IsNullOrEmpty(vin)).Distinct().ToList();
                    ListaVins = new ObservableCollection<string>(vinList);
                    if (ListaVins.Any()) VinSelecionado = ListaVins.First();
                    App.LogInfo($"{vinList.Count} VINs carregados", "PECAS");
                }
            }
            catch
            {
                App.LogError("Erro ao carregar VINs – lista vazia", "PECAS");
                ListaVins = new ObservableCollection<string>();
                MessageBox.Show("Erro ao carregar VINs.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>Carrega a lista de fornecedores da API.</summary>
        public async Task CarregarFornecedoresAsync()
        {
            App.LogInfo("CarregarFornecedoresAsync iniciado", "PECAS");
            try
            {
                var response = await _httpClient.GetAsync("api/dados/fornecedores");
                App.LogInfo($"GET Fornecedores → {(int)response.StatusCode}", "PECAS");
                if (!response.IsSuccessStatusCode) return;

                var json = await response.Content.ReadAsStringAsync();
                var fornecedores = JsonSerializer.Deserialize<List<FornecedorModel>>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (fornecedores != null && fornecedores.Any())
                {
                    ListaFornecedores = new ObservableCollection<FornecedorModel>(fornecedores);
                    if (ListaFornecedores.Any()) FornecedorSelecionado = ListaFornecedores.First();
                    App.LogInfo($"{fornecedores.Count} fornecedores carregados", "PECAS");
                }
            }
            catch
            {
                App.LogError("Erro ao carregar fornecedores – lista vazia", "PECAS");
                ListaFornecedores = new ObservableCollection<FornecedorModel>();
                MessageBox.Show("Erro ao carregar fornecedores.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>Carrega a lista de peças da API.</summary>
        public async Task CarregarPecasAsync()
        {
            App.LogInfo("CarregarPecasAsync iniciado", "PECAS");
            try
            {
                var response = await _httpClient.GetAsync("api/dados/componentes");
                App.LogInfo($"GET Peças → {(int)response.StatusCode}", "PECAS");
                if (!response.IsSuccessStatusCode) return;

                var json = await response.Content.ReadAsStringAsync();
                var componentesApi = JsonSerializer.Deserialize<List<VeiculoComponenteApiDto>>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (componentesApi != null)
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
                    App.LogInfo($"{listaMapeada.Count} peças carregadas", "PECAS");
                }
            }
            catch
            {
                App.LogError("Erro ao carregar peças – lista vazia", "PECAS");
                ListaPecas = new ObservableCollection<PecaModel>();
            }
        }

        /// ============================================================
        /// MÉTODOS PRIVADOS
        /// ============================================================

        /// <summary>Registra uma nova peça associada a um VIN e fornecedor.</summary>
        private async Task AdicionarPecaAsync()
        {
            App.LogInfo("AdicionarPecaAsync iniciado", "PECAS");
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

            var novaPeca = new
            {
                Id = Guid.NewGuid().ToString().Substring(0, 8),
                NomePeca = NovaPecaNome,
                Fk_Veiculo_Vin = VinSelecionado,
                Fk_LoteMateriaPrima_Id = "LOTE-MANUAL-" + DateTime.Now.ToString("yyyyMMdd"),
                PesoKg = NovaPecaPesoKg,
                Fk_Fornecedor_Id = FornecedorSelecionado.Id
            };

            App.LogInfo($"Enviando peça: {NovaPecaNome} (VIN: {VinSelecionado}, Fornecedor: {FornecedorSelecionado.Nome})", "PECAS");

            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/dados/componentes", novaPeca);
                App.LogInfo($"POST Peça → {(int)response.StatusCode}", "PECAS");

                if (response.IsSuccessStatusCode)
                {
                    ListaPecas.Insert(0, new PecaModel
                    {
                        NomePeca = NovaPecaNome,
                        VinAssociado = VinSelecionado,
                        PesoKg = NovaPecaPesoKg,
                        FornecedorId = FornecedorSelecionado.Id
                    });

                    NovaPecaNome = "";
                    NovaPecaPesoKg = 0;
                    App.LogInfo("Peça registrada com sucesso!", "PECAS");
                    MessageBox.Show("Peça registrada com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    var erro = await response.Content.ReadAsStringAsync();
                    App.LogError($"Falha POST: {erro}", "PECAS");
                    MessageBox.Show("Erro ao registrar peça.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch
            {
                App.LogError("Erro de conexão ao registrar peça", "PECAS");
                MessageBox.Show("Erro de conexão. Verifique sua rede.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}