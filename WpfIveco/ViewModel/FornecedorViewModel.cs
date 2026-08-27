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
using WpfIveco.Models;
using WpfIveco.ViewModels;

namespace WpfIveco.ViewModels
{
    /// <summary>
    /// ViewModel para a tela de gestão de fornecedores.
    /// Gerencia consulta de CNPJ (Brasil API), cadastro de fornecedores e fallback offline.
    /// CORREÇÃO: Adicionada validação dos dígitos verificadores do CNPJ antes de chamar a API.
    /// </summary>
    public class FornecedorViewModel : ViewModelBase
    {
        // ============================================================
        // CAMPOS PRIVADOS
        // ============================================================

        private readonly HttpClient _httpClient;
        private readonly LocalDatabaseService _localDb;

        // ============================================================
        // PROPRIEDADES (BINDINGS)
        // ============================================================

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

        private bool _isErro = false;
        public bool IsErro
        {
            get => _isErro;
            set { _isErro = value; OnPropertyChanged(); }
        }

        private ObservableCollection<FornecedorModel> _listaFornecedores = new();
        public ObservableCollection<FornecedorModel> ListaFornecedores
        {
            get => _listaFornecedores;
            set { _listaFornecedores = value; OnPropertyChanged(); }
        }

        public int TotalFornecedores => ListaFornecedores?.Count ?? 0;

        private string _statusRfb = "Aguardando consulta";
        public string StatusRfb
        {
            get => _statusRfb;
            set { _statusRfb = value; OnPropertyChanged(); }
        }

        private string _categoriaEsg = "Não avaliado";
        public string CategoriaEsg
        {
            get => _categoriaEsg;
            set { _categoriaEsg = value; OnPropertyChanged(); }
        }

        // ============================================================
        // COMANDOS
        // ============================================================

        public ICommand ConsultarCnpjCommand { get; }
        public ICommand SalvarFornecedorCommand { get; }

        // ============================================================
        // CONSTRUTOR
        // ============================================================

        public FornecedorViewModel(HttpClient httpClient)
        {
            App.LogInfo("Construtor", "FORNEC");
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            _localDb = new LocalDatabaseService();
            ConsultarCnpjCommand = new RelayCommand(async p => await ConsultarCnpjAsync());
            SalvarFornecedorCommand = new RelayCommand(async p => await SalvarFornecedorAsync());
        }

        // ============================================================
        // MÉTODOS PÚBLICOS
        // ============================================================

        public async Task CarregarFornecedoresAsync()
        {
            App.LogInfo("CarregarFornecedoresAsync iniciado", "FORNEC");
            try
            {
                var response = await _httpClient.GetAsync("api/dados/fornecedores");
                App.LogInfo($"GET Fornecedores → {(int)response.StatusCode}", "FORNEC");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var fornecedores = JsonSerializer.Deserialize<List<FornecedorModel>>(json,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (fornecedores != null && fornecedores.Count > 0)
                    {
                        ListaFornecedores = new ObservableCollection<FornecedorModel>(fornecedores);
                        App.LogInfo($"{fornecedores.Count} fornecedores carregados da API", "FORNEC");
                        _ = _localDb.SalvarFornecedoresAsync(fornecedores);
                        return;
                    }
                }

                App.LogWarning("API indisponível, usando dados locais (fornecedores)", "FORNEC");
                await CarregarFornecedoresLocaisAsync();
            }
            catch (Exception ex)
            {
                App.LogError($"Erro ao carregar fornecedores: {ex.Message}", "FORNEC");
                await CarregarFornecedoresLocaisAsync();
            }
        }

        // ============================================================
        // MÉTODOS PRIVADOS
        // ============================================================

        private async Task CarregarFornecedoresLocaisAsync()
        {
            var locais = await _localDb.GetFornecedoresAsync();
            if (locais.Any())
            {
                ListaFornecedores = new ObservableCollection<FornecedorModel>(locais);
                App.LogInfo($"Carregados {locais.Count} fornecedores do SQLite", "FORNEC");
            }
            else
            {
                ListaFornecedores = new ObservableCollection<FornecedorModel>();
            }
        }

        /// <summary>
        /// Valida os dígitos verificadores de um CNPJ (algoritmo padrão da Receita Federal).
        /// CORREÇÃO: Adicionado para evitar chamadas desnecessárias à API.
        /// </summary>
        private bool ValidarCnpj(string cnpj)
        {
            if (cnpj.Length != 14) return false;

            int[] multiplicador1 = { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
            int[] multiplicador2 = { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };

            string tempCnpj = cnpj.Substring(0, 12);
            int soma = 0;
            for (int i = 0; i < 12; i++)
                soma += int.Parse(tempCnpj[i].ToString()) * multiplicador1[i];

            int resto = soma % 11;
            int digito1 = resto < 2 ? 0 : 11 - resto;
            tempCnpj += digito1;

            soma = 0;
            for (int i = 0; i < 13; i++)
                soma += int.Parse(tempCnpj[i].ToString()) * multiplicador2[i];

            resto = soma % 11;
            int digito2 = resto < 2 ? 0 : 11 - resto;

            return cnpj.EndsWith($"{digito1}{digito2}");
        }

        /// <summary>
        /// Consulta um CNPJ na Brasil API via backend.
        /// Preenche os campos do formulário com os dados retornados.
        /// CORREÇÃO: Validação dos dígitos do CNPJ antes de chamar a API.
        /// </summary>
        private async Task ConsultarCnpjAsync()
        {
            App.LogInfo($"Consultando CNPJ: {CnpjBusca}", "FORNEC");
            IsErro = false;
            MensagemCadastro = "";

            if (string.IsNullOrWhiteSpace(CnpjBusca))
            {
                App.LogWarning("CNPJ vazio", "FORNEC");
                MessageBox.Show("Digite um CNPJ para consultar.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            // Remove formatação
            var cnpjLimpo = CnpjBusca.Replace(".", "").Replace("/", "").Replace("-", "");

            // ============================================================
            // CORREÇÃO: Valida os dígitos verificadores antes de chamar a API
            // ============================================================
            if (!ValidarCnpj(cnpjLimpo))
            {
                App.LogWarning("CNPJ com dígitos inválidos", "FORNEC");
                MensagemCadastro = "CNPJ inválido (dígitos verificadores incorretos).";
                IsErro = true;
                return;
            }

            try
            {
                var response = await _httpClient.GetAsync($"api/dados/fornecedores/buscar-cnpj/{cnpjLimpo}");
                App.LogInfo($"GET buscar-cnpj → {(int)response.StatusCode}", "FORNEC");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(json);
                    var fornecedor = doc.RootElement.GetProperty("fornecedor");

                    NomeFornecedorEncontrado = fornecedor.GetProperty("nome").GetString();
                    LocalizacaoFornecedorEncontrado = fornecedor.GetProperty("localizacao").GetString();
                    StatusRfb = fornecedor.TryGetProperty("situacao", out var sit) ? sit.GetString() : "ATIVA (assumido)";

                    var fornecedorExistente = ListaFornecedores.FirstOrDefault(f => f.Cnpj == cnpjLimpo);
                    if (fornecedorExistente != null)
                    {
                        CategoriaEsg = fornecedorExistente.CategoriaEsg ?? "Não avaliado";
                        MensagemCadastro = $"Fornecedor já cadastrado. Categoria ESG: {CategoriaEsg}";
                        App.LogInfo($"Fornecedor encontrado. Categoria: {CategoriaEsg}", "FORNEC");
                    }
                    else
                    {
                        CategoriaEsg = "Não avaliado";
                        MensagemCadastro = "Fornecedor novo! Clique em 'Registrar no Ledger' para salvar.";
                        App.LogInfo("Fornecedor novo – categoria definida como 'Não avaliado'", "FORNEC");
                    }
                    IsErro = false;
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    App.LogWarning("CNPJ não encontrado", "FORNEC");
                    MensagemCadastro = "CNPJ não encontrado na Receita Federal.";
                    IsErro = true;
                    NomeFornecedorEncontrado = "";
                    LocalizacaoFornecedorEncontrado = "";
                    StatusRfb = "Não encontrado";
                    CategoriaEsg = "N/A";
                }
                else
                {
                    App.LogError($"Erro na consulta: {await response.Content.ReadAsStringAsync()}", "FORNEC");
                    MensagemCadastro = "Erro ao consultar CNPJ. Tente novamente.";
                    IsErro = true;
                }
            }
            catch (Exception ex)
            {
                App.LogError($"Erro de conexão na consulta CNPJ: {ex.Message}", "FORNEC");
                MensagemCadastro = "Erro de conexão. Verifique sua internet.";
                IsErro = true;
            }
        }

        /// <summary>
        /// Salva o fornecedor no backend (Firestore via API).
        /// Se a API falhar, salva localmente no SQLite (modo offline).
        /// </summary>
        private async Task SalvarFornecedorAsync()
        {
            App.LogInfo($"Salvando fornecedor: {NomeFornecedorEncontrado}", "FORNEC");

            if (string.IsNullOrWhiteSpace(NomeFornecedorEncontrado))
            {
                App.LogWarning("Nome vazio – abortando", "FORNEC");
                MensagemCadastro = "Consulte um CNPJ válido primeiro.";
                IsErro = true;
                return;
            }

            var cnpjLimpo = CnpjBusca.Replace(".", "").Replace("/", "").Replace("-", "");
            var fornecedor = new FornecedorModel
            {
                Id = Guid.NewGuid().ToString().Substring(0, 8),
                Nome = NomeFornecedorEncontrado,
                Localizacao = LocalizacaoFornecedorEncontrado,
                Cnpj = cnpjLimpo,
                CategoriaEsg = CategoriaEsg
            };

            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/dados/fornecedores", fornecedor);
                App.LogInfo($"POST Fornecedor → {(int)response.StatusCode}", "FORNEC");

                if (response.IsSuccessStatusCode)
                {
                    App.LogInfo($"Fornecedor registrado com sucesso! Categoria: {CategoriaEsg}", "FORNEC");
                    MensagemCadastro = "Fornecedor registrado com sucesso!";
                    IsErro = false;

                    _ = _localDb.SalvarFornecedoresAsync(new List<FornecedorModel> { fornecedor });

                    CnpjBusca = "";
                    NomeFornecedorEncontrado = "";
                    LocalizacaoFornecedorEncontrado = "";
                    StatusRfb = "Aguardando consulta";
                    CategoriaEsg = "Não avaliado";

                    await CarregarFornecedoresAsync();
                }
                else
                {
                    App.LogWarning("API falhou. Salvando fornecedor localmente.", "FORNEC");
                    var salvouLocal = await _localDb.SalvarFornecedorOfflineAsync(fornecedor);
                    if (salvouLocal)
                    {
                        MensagemCadastro = "Fornecedor salvo OFFLINE! Será sincronizado quando a internet voltar.";
                        IsErro = false;
                        await CarregarFornecedoresLocaisAsync();
                    }
                    else
                    {
                        MensagemCadastro = "Erro ao salvar fornecedor (online e offline).";
                        IsErro = true;
                    }
                }
            }
            catch (Exception ex)
            {
                App.LogError($"Erro de conexão ao salvar fornecedor: {ex.Message}", "FORNEC");
                var salvouLocal = await _localDb.SalvarFornecedorOfflineAsync(fornecedor);
                if (salvouLocal)
                {
                    MensagemCadastro = "Fornecedor salvo OFFLINE! Será sincronizado quando a internet voltar.";
                    IsErro = false;
                    await CarregarFornecedoresLocaisAsync();
                }
                else
                {
                    MensagemCadastro = "Erro de conexão. Não foi possível salvar (offline).";
                    IsErro = true;
                }
            }
        }
    }
}