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
using WpfIveco.DTO;
using WpfIveco.Models;
using WpfIveco.ViewModels;

namespace WpfIveco.ViewModel
{
    public class PecasViewModel : ViewModelBase
    {
        private readonly HttpClient _httpClient;

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
            Debug.WriteLine("[PECAS] Construtor");
            _httpClient = httpClient;
            AdicionarPecaManualCommand = new RelayCommand(async p => await AdicionarPecaAsync());
        }

        public async Task CarregarVinsAsync()
        {
            Debug.WriteLine("[PECAS] CarregarVinsAsync iniciado.");
            try
            {
                var response = await _httpClient.GetAsync("api/dados/veiculos");
                Debug.WriteLine($"[PECAS] GET VINs → {(int)response.StatusCode}");
                if (!response.IsSuccessStatusCode) return;

                var json = await response.Content.ReadAsStringAsync();
                Debug.WriteLine("[PECAS] JSON VINs recebido.");
                var veiculos = JsonSerializer.Deserialize<List<VeiculoModel>>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (veiculos != null && veiculos.Any())
                {
                    var vinList = veiculos.Select(v => v.Vin).Where(vin => !string.IsNullOrEmpty(vin)).Distinct().ToList();
                    ListaVins = new ObservableCollection<string>(vinList);
                    if (ListaVins.Any()) VinSelecionado = ListaVins.First();
                    Debug.WriteLine($"[PECAS] {vinList.Count} VINs carregados.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[PECAS] ERRO VINs: {ex.Message}");
                Debug.WriteLine(ex.StackTrace);
                MessageBox.Show("Erro ao carregar VINs.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public async Task CarregarFornecedoresAsync()
        {
            Debug.WriteLine("[PECAS] CarregarFornecedoresAsync iniciado.");
            try
            {
                var response = await _httpClient.GetAsync("api/dados/fornecedores");
                Debug.WriteLine($"[PECAS] GET Fornecedores → {(int)response.StatusCode}");
                if (!response.IsSuccessStatusCode) return;

                var json = await response.Content.ReadAsStringAsync();
                Debug.WriteLine("[PECAS] JSON Fornecedores recebido.");
                var fornecedores = JsonSerializer.Deserialize<List<FornecedorModel>>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (fornecedores != null && fornecedores.Any())
                {
                    ListaFornecedores = new ObservableCollection<FornecedorModel>(fornecedores);
                    if (ListaFornecedores.Any()) FornecedorSelecionado = ListaFornecedores.First();
                    Debug.WriteLine($"[PECAS] {fornecedores.Count} fornecedores carregados.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[PECAS] ERRO Fornecedores: {ex.Message}");
                Debug.WriteLine(ex.StackTrace);
                MessageBox.Show("Erro ao carregar fornecedores.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public async Task CarregarPecasAsync()
        {
            Debug.WriteLine("[PECAS] CarregarPecasAsync iniciado.");
            try
            {
                var response = await _httpClient.GetAsync("api/dados/componentes");
                Debug.WriteLine($"[PECAS] GET Peças → {(int)response.StatusCode}");
                if (!response.IsSuccessStatusCode) return;

                var json = await response.Content.ReadAsStringAsync();
                Debug.WriteLine("[PECAS] JSON Peças recebido.");
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
                    Debug.WriteLine($"[PECAS] {listaMapeada.Count} peças carregadas.");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[PECAS] ERRO Peças: {ex.Message}");
                Debug.WriteLine(ex.StackTrace);
            }
        }

        private async Task AdicionarPecaAsync()
        {
            Debug.WriteLine("[PECAS] AdicionarPecaAsync iniciado.");
            if (string.IsNullOrWhiteSpace(VinSelecionado))
            {
                Debug.WriteLine("[PECAS] VIN não selecionado.");
                MessageBox.Show("Selecione um veículo.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(NovaPecaNome))
            {
                Debug.WriteLine("[PECAS] Nome da peça vazio.");
                MessageBox.Show("Preencha o nome da peça.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (NovaPecaPesoKg <= 0)
            {
                Debug.WriteLine($"[PECAS] Peso inválido: {NovaPecaPesoKg}");
                MessageBox.Show("Informe um peso > 0 kg.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (FornecedorSelecionado == null || string.IsNullOrWhiteSpace(FornecedorSelecionado.Id))
            {
                Debug.WriteLine("[PECAS] Fornecedor não selecionado.");
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

            Debug.WriteLine($"[PECAS] Enviando peça: {NovaPecaNome} (VIN: {VinSelecionado}, Fornecedor: {FornecedorSelecionado.Nome})");

            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/dados/componentes", novaPeca);
                Debug.WriteLine($"[PECAS] POST Peça → {(int)response.StatusCode}");

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
                    Debug.WriteLine("[PECAS] Peça registrada com sucesso!");
                    MessageBox.Show("Peça registrada com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    var erro = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine($"[PECAS] Falha POST: {erro}");
                    MessageBox.Show("Erro ao registrar peça.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[PECAS] ERRO POST: {ex.Message}");
                Debug.WriteLine(ex.StackTrace);
                MessageBox.Show("Erro de conexão.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}