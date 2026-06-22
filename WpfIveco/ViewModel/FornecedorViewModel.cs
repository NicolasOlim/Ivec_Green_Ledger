using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using WpfIveco.Models;
using WpfIveco.ViewModels;

namespace WpfIveco.ViewModels
{
    public class FornecedorViewModel : ViewModelBase
    {
        private readonly HttpClient _httpClient;

        private string _cnpjBusca = "";
        public string CnpjBusca
        {
            get => _cnpjBusca;
            set { _cnpjBusca = value; OnPropertyChanged(); }
        }

        private string _nomeFornecedorEncontrado = "";
        public string NomeFornecedorEncontrado
        {
            get => _nomeFornecedorEncontrado;
            set { _nomeFornecedorEncontrado = value; OnPropertyChanged(); }
        }

        private string _localizacaoFornecedorEncontrado = "";
        public string LocalizacaoFornecedorEncontrado
        {
            get => _localizacaoFornecedorEncontrado;
            set { _localizacaoFornecedorEncontrado = value; OnPropertyChanged(); }
        }

        private string _mensagemCadastro = "";
        public string MensagemCadastro
        {
            get => _mensagemCadastro;
            set { _mensagemCadastro = value; OnPropertyChanged(); }
        }

        private ObservableCollection<FornecedorModel> _listaFornecedores = new();
        public ObservableCollection<FornecedorModel> ListaFornecedores
        {
            get => _listaFornecedores;
            set { _listaFornecedores = value; OnPropertyChanged(); }
        }

        public int TotalFornecedores => ListaFornecedores?.Count ?? 0;

        public ICommand ConsultarCnpjCommand { get; }
        public ICommand SalvarFornecedorCommand { get; }

        public FornecedorViewModel(HttpClient httpClient)
        {
            App.LogInfo("Construtor", "FORNEC");
            _httpClient = httpClient;
            ConsultarCnpjCommand = new RelayCommand(async p => await ConsultarCnpjAsync());
            SalvarFornecedorCommand = new RelayCommand(async p => await SalvarFornecedorAsync());
        }

        public async Task CarregarFornecedoresAsync()
        {
            App.LogInfo("CarregarFornecedoresAsync iniciado", "FORNEC");
            try
            {
                var response = await _httpClient.GetAsync("api/dados/fornecedores");
                App.LogInfo($"GET Fornecedores → {(int)response.StatusCode}", "FORNEC");
                if (!response.IsSuccessStatusCode) return;

                var json = await response.Content.ReadAsStringAsync();
                var fornecedores = JsonSerializer.Deserialize<List<FornecedorModel>>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (fornecedores != null)
                {
                    ListaFornecedores = new ObservableCollection<FornecedorModel>(fornecedores);
                    App.LogInfo($"{fornecedores.Count} fornecedores carregados", "FORNEC");
                }
            }
            catch
            {
                App.LogError("Erro ao carregar fornecedores – lista vazia", "FORNEC");
                ListaFornecedores = new ObservableCollection<FornecedorModel>();
            }
        }

        private async Task ConsultarCnpjAsync()
        {
            App.LogInfo($"Consultando CNPJ: {CnpjBusca}", "FORNEC");
            if (string.IsNullOrWhiteSpace(CnpjBusca))
            {
                App.LogWarning("CNPJ vazio", "FORNEC");
                MessageBox.Show("Digite um CNPJ para consultar.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var cnpjLimpo = CnpjBusca.Replace(".", "").Replace("/", "").Replace("-", "");
                var response = await _httpClient.GetAsync($"api/dados/fornecedores/buscar-cnpj/{cnpjLimpo}");
                App.LogInfo($"GET buscar-cnpj → {(int)response.StatusCode}", "FORNEC");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(json);
                    var fornecedor = doc.RootElement.GetProperty("fornecedor");

                    NomeFornecedorEncontrado = fornecedor.GetProperty("nome").GetString();
                    LocalizacaoFornecedorEncontrado = fornecedor.GetProperty("localizacao").GetString();
                    MensagemCadastro = "Fornecedor encontrado! Clique em 'Registrar no Ledger' para salvar.";
                    App.LogInfo($"Fornecedor encontrado: {NomeFornecedorEncontrado}", "FORNEC");
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    App.LogWarning("CNPJ não encontrado", "FORNEC");
                    MensagemCadastro = "CNPJ não encontrado na Receita Federal.";
                    NomeFornecedorEncontrado = "";
                    LocalizacaoFornecedorEncontrado = "";
                }
                else
                {
                    var erro = await response.Content.ReadAsStringAsync();
                    App.LogError($"Erro na consulta: {erro}", "FORNEC");
                    MensagemCadastro = "Erro ao consultar CNPJ. Tente novamente.";
                }
            }
            catch
            {
                App.LogError("Erro de conexão na consulta CNPJ", "FORNEC");
                MensagemCadastro = "Erro de conexão. Verifique sua internet.";
            }
        }

        private async Task SalvarFornecedorAsync()
        {
            App.LogInfo($"Salvando fornecedor: {NomeFornecedorEncontrado}", "FORNEC");
            if (string.IsNullOrWhiteSpace(NomeFornecedorEncontrado))
            {
                App.LogWarning("Nome vazio – abortando", "FORNEC");
                MensagemCadastro = "Consulte um CNPJ válido primeiro.";
                return;
            }

            try
            {
                var fornecedor = new
                {
                    Id = "",
                    Nome = NomeFornecedorEncontrado,
                    Localizacao = LocalizacaoFornecedorEncontrado,
                    Cnpj = CnpjBusca.Replace(".", "").Replace("/", "").Replace("-", "")
                };

                var response = await _httpClient.PostAsJsonAsync("api/dados/fornecedores", fornecedor);
                App.LogInfo($"POST Fornecedor → {(int)response.StatusCode}", "FORNEC");

                if (response.IsSuccessStatusCode)
                {
                    App.LogInfo("Fornecedor registrado com sucesso!", "FORNEC");
                    MensagemCadastro = "Fornecedor registrado com sucesso!";
                    CnpjBusca = "";
                    NomeFornecedorEncontrado = "";
                    LocalizacaoFornecedorEncontrado = "";
                    await CarregarFornecedoresAsync();
                }
                else
                {
                    var erro = await response.Content.ReadAsStringAsync();
                    App.LogError($"Falha ao salvar: {erro}", "FORNEC");
                    MensagemCadastro = $"Erro ao salvar: {erro}";
                }
            }
            catch
            {
                App.LogError("Erro de conexão ao salvar fornecedor", "FORNEC");
                MensagemCadastro = "Erro de conexão. Verifique sua rede.";
            }
        }
    }
}