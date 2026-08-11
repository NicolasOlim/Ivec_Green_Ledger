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
    /// </summary>
    public class FornecedorViewModel : ViewModelBase
    {
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
            _httpClient = httpClient;
            _localDb = new LocalDatabaseService();
            ConsultarCnpjCommand = new RelayCommand(async p => await ConsultarCnpjAsync());
            SalvarFornecedorCommand = new RelayCommand(async p => await SalvarFornecedorAsync());
        }

        // ============================================================
        // MÉTODOS PÚBLICOS
        // ============================================================

        /// <summary>
        /// Carrega a lista de fornecedores, priorizando a API.
        /// Se falhar, busca no SQLite local.
        /// </summary>
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
                        // Atualiza cache local em background
                        _ = _localDb.SalvarFornecedoresAsync(fornecedores);
                        return;
                    }
                }

                // Fallback para dados locais
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

        /// <summary>
        /// Carrega fornecedores do banco local SQLite.
        /// </summary>
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
        /// Consulta um CNPJ na Brasil API via backend.
        /// Preenche os campos do formulário com os dados retornados.
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
                    StatusRfb = fornecedor.TryGetProperty("situacao", out var sit) ? sit.GetString() : "ATIVA (assumido)";

                    // Verifica se o fornecedor já está cadastrado na lista local
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
                Id = Guid.NewGuid().ToString().Substring(0, 8), // ID temporário
                Nome = NomeFornecedorEncontrado,
                Localizacao = LocalizacaoFornecedorEncontrado,
                Cnpj = cnpjLimpo,
                CategoriaEsg = CategoriaEsg
            };

            try
            {
                // Tenta salvar na API
                var response = await _httpClient.PostAsJsonAsync("api/dados/fornecedores", fornecedor);
                App.LogInfo($"POST Fornecedor → {(int)response.StatusCode}", "FORNEC");

                if (response.IsSuccessStatusCode)
                {
                    App.LogInfo($"Fornecedor registrado com sucesso! Categoria: {CategoriaEsg}", "FORNEC");
                    MensagemCadastro = "Fornecedor registrado com sucesso!";
                    IsErro = false;

                    // Salva também localmente (para manter sincronia)
                    _ = _localDb.SalvarFornecedoresAsync(new List<FornecedorModel> { fornecedor });

                    // Limpa os campos
                    CnpjBusca = "";
                    NomeFornecedorEncontrado = "";
                    LocalizacaoFornecedorEncontrado = "";
                    StatusRfb = "Aguardando consulta";
                    CategoriaEsg = "Não avaliado";

                    await CarregarFornecedoresAsync(); // Recarrega a lista
                }
                else
                {
                    // Se a API falhou, tenta salvar localmente (offline)
                    App.LogWarning("API falhou. Salvando fornecedor localmente.", "FORNEC");
                    var salvouLocal = await _localDb.SalvarFornecedorOfflineAsync(fornecedor);
                    if (salvouLocal)
                    {
                        MensagemCadastro = "Fornecedor salvo OFFLINE! Será sincronizado quando a internet voltar.";
                        IsErro = false;
                        await CarregarFornecedoresLocaisAsync(); // Atualiza a lista com os dados locais
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
                // Exceção de rede – fallback offline
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