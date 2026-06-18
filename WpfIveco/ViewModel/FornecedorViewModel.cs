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

namespace WpfIveco.ViewModels
{
    public class FornecedorViewModel : ViewModelBase
    {
        private readonly HttpClient _httpClient;

        /// Propriedades

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

        /// Comandos
        public ICommand ConsultarCnpjCommand { get; }
        public ICommand SalvarFornecedorCommand { get; }

        public FornecedorViewModel(HttpClient httpClient)
        {
            _httpClient = httpClient;
            ConsultarCnpjCommand = new RelayCommand(async p => await ConsultarCnpjAsync());
            SalvarFornecedorCommand = new RelayCommand(async p => await SalvarFornecedorAsync());
        }

        public async Task CarregarFornecedoresAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/dados/fornecedores");
                if (!response.IsSuccessStatusCode)
                {
                    Debug.WriteLine($"[ERRO CARREGAR FORNECEDORES] HTTP {(int)response.StatusCode}");
                    return;
                }

                var json = await response.Content.ReadAsStringAsync();
                var fornecedores = JsonSerializer.Deserialize<List<FornecedorModel>>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (fornecedores != null)
                {
                    ListaFornecedores = new ObservableCollection<FornecedorModel>(fornecedores);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERRO CARREGAR FORNECEDORES] {ex.Message}");
            }
        }

        private async Task ConsultarCnpjAsync()
        {
            if (string.IsNullOrWhiteSpace(CnpjBusca))
            {
                MessageBox.Show("Digite um CNPJ para consultar.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var cnpjLimpo = CnpjBusca.Replace(".", "").Replace("/", "").Replace("-", "");
                var response = await _httpClient.GetAsync($"api/dados/fornecedores/buscar-cnpj/{cnpjLimpo}");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(json);
                    var fornecedor = doc.RootElement.GetProperty("fornecedor");

                    NomeFornecedorEncontrado = fornecedor.GetProperty("nome").GetString();
                    LocalizacaoFornecedorEncontrado = fornecedor.GetProperty("localizacao").GetString();
                    MensagemCadastro = "Fornecedor encontrado! Clique em 'Registrar no Ledger' para salvar.";
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    MensagemCadastro = "CNPJ não encontrado na Receita Federal.";
                    NomeFornecedorEncontrado = "";
                    LocalizacaoFornecedorEncontrado = "";
                }
                else
                {
                    var erro = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine($"[ERRO CONSULTAR CNPJ] {erro}");
                    MensagemCadastro = "Erro ao consultar CNPJ. Tente novamente.";
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERRO CONSULTAR CNPJ] {ex.Message}");
                MensagemCadastro = "Erro de conexão. Verifique sua internet.";
            }
        }

        private async Task SalvarFornecedorAsync()
        {
            if (string.IsNullOrWhiteSpace(NomeFornecedorEncontrado))
            {
                MensagemCadastro = "Consulte um CNPJ válido primeiro.";
                return;
            }

            try
            {
                /// ================================================
                ///CORREÇÃO: Adicionado o campo "Id" com valor vazio
                /// ================================================
                var fornecedor = new
                {
                    Id = "",  // <-- LINHA ADICIONADA
                    Nome = NomeFornecedorEncontrado,
                    Localizacao = LocalizacaoFornecedorEncontrado,
                    Cnpj = CnpjBusca.Replace(".", "").Replace("/", "").Replace("-", "")
                };

                var response = await _httpClient.PostAsJsonAsync("api/dados/fornecedores", fornecedor);

                if (response.IsSuccessStatusCode)
                {
                    MensagemCadastro = "Fornecedor registrado com sucesso!";
                    CnpjBusca = "";
                    NomeFornecedorEncontrado = "";
                    LocalizacaoFornecedorEncontrado = "";
                    await CarregarFornecedoresAsync();
                }
                else
                {
                    var erro = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine($"[ERRO SALVAR FORNECEDOR] HTTP {(int)response.StatusCode} -> {erro}");

                    ///Tratamento específico para erro de validação
                    if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                    {
                        try
                        {
                            using var doc = JsonDocument.Parse(erro);
                            if (doc.RootElement.TryGetProperty("errors", out var errors))
                            {
                                var mensagem = "Erro de validação:\n";
                                foreach (var prop in errors.EnumerateObject())
                                {
                                    var field = prop.Name;
                                    var messages = prop.Value.EnumerateArray().Select(x => x.GetString());
                                    mensagem += $"- {field}: {string.Join(", ", messages)}\n";
                                }
                                MensagemCadastro = mensagem;
                            }
                        }
                        catch { }
                    }
                    else
                    {
                        MensagemCadastro = $"Erro ao salvar: {erro}";
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERRO SALVAR FORNECEDOR] {ex.Message}");
                MensagemCadastro = $"Erro de conexão: {ex.Message}";
            }
        }
    }
}