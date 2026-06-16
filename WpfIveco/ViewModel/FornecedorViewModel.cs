using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Input;
using WpfIveco.Models;
using WpfIveco.ViewModels;

namespace WpfIveco.ViewModel
{
    public class FornecedorViewModel : ViewModelBase
    {
        private readonly HttpClient _httpClient;

        /// <summary>
        /// PROPRIEDADES
        /// </summary>
        
        private string _totalFornecedores = "0";
        public string TotalFornecedores
        {
            get => _totalFornecedores;
            set { _totalFornecedores = value; OnPropertyChanged(); }
        }

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

        /// <summary>
        /// COMANDOS
        /// </summary>
       
        public ICommand ConsultarCnpjCommand { get; }
        public ICommand SalvarFornecedorCommand { get; }

        
        /// <summary>
        /// CONSTRUTOR
        /// </summary>
        /// <param name="httpClient"></param>
        
        public FornecedorViewModel(HttpClient httpClient)
        {
            _httpClient = httpClient;
            ConsultarCnpjCommand = new RelayCommand(async p => await BuscarPorCnpjAsync());
            SalvarFornecedorCommand = new RelayCommand(async p => await SalvarFornecedorAsync());
        }

      
        /// <summary>
        /// CARREGAR FORNECEDORES
        /// </summary>
        /// <returns></returns>
        
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

                TotalFornecedores = fornecedores?.Count.ToString() ?? "0";
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERRO INESPERADO FORNECEDORES] {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}");
            }
        }

        
        /// <summary>
        /// BUSCAR POR CNPJ
        /// </summary>
        /// <returns></returns>
        
        private async Task BuscarPorCnpjAsync()
        {
            var cnpjLimpo = new string(System.Linq.Enumerable.ToArray(
                System.Linq.Enumerable.Where(CnpjBusca, char.IsDigit)));

            if (cnpjLimpo.Length != 14)
            {
                MensagemCadastro = "⚠️ O CNPJ necessita de 14 números.";
                return;
            }

            MensagemCadastro = "A consultar a Receita Federal...";

            try
            {
                var response = await _httpClient.GetAsync($"api/dados/fornecedores/buscar-cnpj/{cnpjLimpo}");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(json);
                    var fornecedor = doc.RootElement.GetProperty("fornecedor");

                    NomeFornecedorEncontrado = fornecedor.GetProperty("nome").GetString();
                    LocalizacaoFornecedorEncontrado = fornecedor.GetProperty("localizacao").GetString();
                    MensagemCadastro = "✅ Empresa localizada e qualificada.";
                }
                else
                {
                    var erro = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine($"[ERRO BUSCAR CNPJ] HTTP {(int)response.StatusCode} -> {erro}");

                    MensagemCadastro = "❌ CNPJ não encontrado. Verifique e tente novamente.";
                    NomeFornecedorEncontrado = "";
                    LocalizacaoFornecedorEncontrado = "";
                }
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"[ERRO CONEXÃO CNPJ] {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}");
                MensagemCadastro = "❌ Erro de ligação. Verifique a sua rede.";
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERRO INESPERADO CNPJ] {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}");
                MensagemCadastro = "❌ Ocorreu um erro inesperado. Tente novamente.";
            }
        }

        
        /// <summary>
        /// SALVAR FORNECEDOR
        /// </summary>
        /// <returns></returns>
        
        private async Task SalvarFornecedorAsync()
        {
            if (string.IsNullOrWhiteSpace(NomeFornecedorEncontrado))
            {
                MensagemCadastro = "⚠️ Efetue a consulta primeiro.";
                return;
            }

            var cnpjLimpo = new string(System.Linq.Enumerable.ToArray(
                System.Linq.Enumerable.Where(CnpjBusca, char.IsDigit)));

            var novoFornecedor = new FornecedorModel
            {
                Cnpj = cnpjLimpo,
                Nome = NomeFornecedorEncontrado,
                Localizacao = LocalizacaoFornecedorEncontrado
            };

            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/dados/fornecedores", novoFornecedor);

                if (response.IsSuccessStatusCode)
                {
                    MensagemCadastro = "✅ Fornecedor registado com sucesso no Ledger!";
                    CnpjBusca = "";
                    NomeFornecedorEncontrado = "";
                    LocalizacaoFornecedorEncontrado = "";
                    await CarregarFornecedoresAsync();
                }
                else
                {
                    var erro = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine($"[ERRO SALVAR FORNECEDOR] HTTP {(int)response.StatusCode} -> {erro}");
                    MensagemCadastro = "❌ Não foi possível guardar o fornecedor. Tente novamente.";
                }
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"[ERRO CONEXÃO FORNECEDOR] {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}");
                MensagemCadastro = "❌ Erro de ligação. Verifique a sua rede.";
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERRO INESPERADO FORNECEDOR] {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}");
                MensagemCadastro = "❌ Ocorreu um erro inesperado. Tente novamente.";
            }
        }
    }
}