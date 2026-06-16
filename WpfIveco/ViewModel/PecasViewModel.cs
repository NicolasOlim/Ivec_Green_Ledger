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
using WpfIveco.Models.WpfIveco.Models;
using WpfIveco.ViewModels;

namespace WpfIveco.ViewModel
{
    public class PecasViewModel : ViewModelBase
    {
        private readonly HttpClient _httpClient;

        /// <summary>
        /// PROPRIEDADES
        /// </summary>
        
        private string _novaPecaVin = "";
        public string NovaPecaVin
        {
            get => _novaPecaVin;
            set { _novaPecaVin = value; OnPropertyChanged(); }
        }

        private string _novaPecaNome = "";
        public string NovaPecaNome
        {
            get => _novaPecaNome;
            set { _novaPecaNome = value; OnPropertyChanged(); }
        }

        private ObservableCollection<PecaModel> _listaPecas = new();
        public ObservableCollection<PecaModel> ListaPecas
        {
            get => _listaPecas;
            set { _listaPecas = value; OnPropertyChanged(); }
        }

        
        /// <summary>
        /// COMANDOS
        /// </summary>
        
        public ICommand AdicionarPecaManualCommand { get; }

        
        /// <summary>
        /// CONSTRUTOR
        /// </summary>
        /// <param name="httpClient"></param>
        
        public PecasViewModel(HttpClient httpClient)
        {
            _httpClient = httpClient;
            AdicionarPecaManualCommand = new RelayCommand(async p => await AdicionarPecaAsync());
        }

        
        /// <summary>
        /// CARREGAR PEÇAS
        /// </summary>
        /// <returns></returns>
        
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
                            VinAssociado = c.Fk_Veiculo_Vin
                        })
                        .Reverse()
                        .ToList();

                    ListaPecas = new ObservableCollection<PecaModel>(listaMapeada);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERRO INESPERADO PEÇAS] {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}");
            }
        }

        
        /// ADICIONAR PEÇA MANUAL
        
        private async Task AdicionarPecaAsync()
        {
            if (string.IsNullOrWhiteSpace(NovaPecaNome) || string.IsNullOrWhiteSpace(NovaPecaVin))
            {
                MessageBox.Show("Preencha o VIN e o nome da peça.", "Aviso",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var novaPeca = new
            {
                Id = Guid.NewGuid().ToString().Substring(0, 8),
                NomePeca = NovaPecaNome,
                Fk_Veiculo_Vin = NovaPecaVin,
                Fk_LoteMateriaPrima_Id = "LOTE-MANUAL-" + DateTime.Now.ToString("yyyyMMdd")
            };

            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/dados/componentes", novaPeca);

                if (response.IsSuccessStatusCode)
                {
                    ListaPecas.Insert(0, new PecaModel
                    {
                        NomePeca = NovaPecaNome,
                        VinAssociado = NovaPecaVin
                    });
                    NovaPecaNome = "";
                    NovaPecaVin = "";
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