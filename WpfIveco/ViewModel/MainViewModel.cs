using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using WpfIveco.ViewModels;

namespace WpfIveco.ViewModel
{
    /// <summary>
    /// ViewModel principal da aplicação. Gerencia autenticação, navegação entre abas,
    /// inicialização dos sub-ViewModels e carregamento global de dados.
    /// CORREÇÃO: Removido bypass de SSL, URL lida de App.config, HttpClient instanciado uma única vez.
    /// </summary>
    public class MainViewModel : ViewModelBase
    {
        // ============================================================
        // CAMPOS PRIVADOS
        // ============================================================

        private readonly HttpClient _httpClient;

        // ============================================================
        // SUB-VIEWMODELS
        // ============================================================

        public DashboardViewModel Dashboard { get; }
        public RastreabilidadeViewModel Rastreabilidade { get; }
        public FornecedorViewModel Fornecedor { get; }
        public PecasViewModel Pecas { get; }
        public AnalisesViewModel Analises { get; }
        public RelatoriosViewModel Relatorios { get; }

        // ============================================================
        // ESTADO DE LOGIN
        // ============================================================

        private bool _isBusy = false;
        public bool IsBusy { get => _isBusy; set { _isBusy = value; OnPropertyChanged(); } }

        private bool _isLoggedIn = false;
        public bool IsLoggedIn { get => _isLoggedIn; set { _isLoggedIn = value; OnPropertyChanged(); } }

        private bool _isAdmin = false;
        public bool IsAdmin { get => _isAdmin; set { _isAdmin = value; OnPropertyChanged(); } }

        private bool _modoCadastro = false;
        public bool ModoCadastro { get => _modoCadastro; set { _modoCadastro = value; OnPropertyChanged(); } }

        // ============================================================
        // CAMPOS DE LOGIN E CADASTRO
        // ============================================================

        private string _loginError = "";
        public string LoginError
        {
            get => _loginError;
            set
            {
                _loginError = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasLoginError));
            }
        }

        public bool HasLoginError => !string.IsNullOrEmpty(LoginError);

        private string _loginEmail = "";
        public string LoginEmail
        {
            get => _loginEmail;
            set
            {
                _loginEmail = value;
                OnPropertyChanged();
                if (!string.IsNullOrEmpty(EmailError))
                {
                    EmailError = "";
                    EmailValido = false;
                }
                if (!string.IsNullOrEmpty(LoginError))
                {
                    LoginError = "";
                }
            }
        }

        private string _loginSenha = "";
        public string LoginSenha
        {
            get => _loginSenha;
            set
            {
                _loginSenha = value;
                OnPropertyChanged();
                if (!string.IsNullOrEmpty(LoginError))
                {
                    LoginError = "";
                }
            }
        }

        private string _cadastroNome = "";
        public string CadastroNome { get => _cadastroNome; set { _cadastroNome = value; OnPropertyChanged(); } }

        private string _cadastroPerfil = "Usuario";
        public string CadastroPerfil { get => _cadastroPerfil; set { _cadastroPerfil = value; OnPropertyChanged(); } }

        private string _nomeUsuario = "Visitante";
        public string NomeUsuario { get => _nomeUsuario; set { _nomeUsuario = value; OnPropertyChanged(); } }

        private string _perfilUsuario = "Sessão não iniciada";
        public string PerfilUsuario { get => _perfilUsuario; set { _perfilUsuario = value; OnPropertyChanged(); } }

        // ============================================================
        // VALIDAÇÃO DE E-MAIL
        // ============================================================

        private string _emailError = "";
        public string EmailError
        {
            get => _emailError;
            set { _emailError = value; OnPropertyChanged(); }
        }

        private bool _emailValido = false;
        public bool EmailValido
        {
            get => _emailValido;
            set { _emailValido = value; OnPropertyChanged(); }
        }

        private bool _isValidatingEmail = false;
        public bool IsValidatingEmail
        {
            get => _isValidatingEmail;
            set { _isValidatingEmail = value; OnPropertyChanged(); }
        }

        // ============================================================
        // NAVEGAÇÃO E CONFIGURAÇÕES
        // ============================================================

        private string _abaAtiva = "Dashboard";
        public string AbaAtiva
        {
            get => _abaAtiva;
            set
            {
                if (_abaAtiva != value)
                {
                    _abaAtiva = value;
                    OnPropertyChanged();

                    if (IsLoggedIn)
                    {
                        switch (_abaAtiva)
                        {
                            case "Dashboard":
                                _ = Dashboard.AtualizarPegadaMediaAsync();
                                break;
                            case "Analises":
                                _ = Analises.AtualizarAsync();
                                break;
                        }
                    }
                }
            }
        }

        private string _apiUrlConfig;
        public string ApiUrlConfig
        {
            get => _apiUrlConfig;
            set
            {
                _apiUrlConfig = value;
                OnPropertyChanged();
                // Não recria o HttpClient para evitar problemas com requisições em andamento
            }
        }

        private string _statusSimulador = "Desativado";
        public string StatusSimulador { get => _statusSimulador; set { _statusSimulador = value; OnPropertyChanged(); } }

        // ============================================================
        // COMANDOS
        // ============================================================

        public ICommand FazerLoginCommand { get; }
        public ICommand FazerCadastroCommand { get; }
        public ICommand FazerLogoutCommand { get; }
        public ICommand AlternarModoAuthCommand { get; }
        public ICommand MudarAbaCommand { get; }
        public ICommand LigarDesligarSimuladorCommand { get; }
        public ICommand EsqueciMinhaSenhaCommand { get; }

        // ============================================================
        // CONSTRUTOR
        // ============================================================

        public MainViewModel()
        {
            App.LogInfo("Construtor iniciado", "MAIN");

            // Lê a URL do App.config
            _apiUrlConfig = System.Configuration.ConfigurationManager.AppSettings["ApiBaseUrl"]
                ?? "https://apiivecogreenledger.runasp.net/";

            // ============================================================
            // CORREÇÃO: Removido o bypass SSL (ServerCertificateCustomValidationCallback)
            // Isso garante que a aplicação valide certificados em produção.
            // ============================================================
            _httpClient = new HttpClient { BaseAddress = new Uri(_apiUrlConfig) };
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "IvecoWpfApp/1.0");

            Dashboard = new DashboardViewModel(_httpClient);
            Rastreabilidade = new RastreabilidadeViewModel(_httpClient);
            Fornecedor = new FornecedorViewModel(_httpClient);
            Pecas = new PecasViewModel(_httpClient);
            Analises = new AnalisesViewModel(_httpClient);
            Relatorios = new RelatoriosViewModel(_httpClient);

            MudarAbaCommand = new RelayCommand(p => AbaAtiva = p as string);
            AlternarModoAuthCommand = new RelayCommand(p => ModoCadastro = !ModoCadastro);
            LigarDesligarSimuladorCommand = new RelayCommand(p =>
                StatusSimulador = StatusSimulador == "Ativo" ? "Desativado" : "Ativo");

            FazerLoginCommand = new RelayCommand(async p => await ExecutarLoginAsync());
            FazerCadastroCommand = new RelayCommand(async p => await ExecutarCadastroAsync());
            FazerLogoutCommand = new RelayCommand(p => ExecutarLogout());
            EsqueciMinhaSenhaCommand = new RelayCommand(async p => await ExecutarEsqueciSenhaAsync());

            App.LogInfo("Construtor finalizado", "MAIN");
        }

        // ============================================================
        // MÉTODOS PRIVADOS
        // ============================================================

        private async Task<bool> ValidarEmailAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                EmailError = "O e-mail é obrigatório.";
                EmailValido = false;
                return false;
            }

            IsValidatingEmail = true;
            EmailError = "Verificando e-mail...";

            try
            {
                var response = await _httpClient.GetAsync($"api/dados/validar-email?email={Uri.EscapeDataString(email)}");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(json);
                    var valido = doc.RootElement.GetProperty("valido").GetBoolean();
                    var mensagem = doc.RootElement.GetProperty("mensagem").GetString();

                    if (valido)
                    {
                        EmailError = "";
                        EmailValido = true;
                        return true;
                    }
                    else
                    {
                        EmailError = mensagem ?? "E-mail inválido.";
                        EmailValido = false;
                        return false;
                    }
                }
                else
                {
                    EmailError = "Erro ao validar e-mail. Tente novamente.";
                    EmailValido = false;
                    return false;
                }
            }
            catch
            {
                EmailError = "Erro de conexão. Não foi possível validar o e-mail.";
                EmailValido = false;
                return false;
            }
            finally
            {
                IsValidatingEmail = false;
            }
        }

        private async Task ExecutarLoginAsync()
        {
            App.LogInfo($"Tentativa para {LoginEmail}", "LOGIN");
            LoginError = "";

            if (!await ValidarEmailAsync(LoginEmail))
            {
                LoginError = EmailError;
                return;
            }

            if (string.IsNullOrWhiteSpace(LoginSenha))
            {
                LoginError = "Por favor, introduza a sua palavra-passe.";
                return;
            }

            var credenciais = new { Email = LoginEmail, Senha = LoginSenha };

            try
            {
                IsBusy = true;
                var response = await _httpClient.PostAsJsonAsync("api/dados/login", credenciais);
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(json);
                    var userJson = doc.RootElement.GetProperty("usuario");

                    NomeUsuario = userJson.GetProperty("nome").GetString();
                    PerfilUsuario = userJson.GetProperty("acesso").GetString();
                    IsAdmin = PerfilUsuario == "Admin";
                    IsLoggedIn = true;
                    LoginSenha = "";
                    LoginError = "";
                    await CarregarTudoAsync();
                }
                else
                {
                    var erro = await response.Content.ReadAsStringAsync();
                    LoginError = "Credenciais inválidas ou acesso negado à plataforma.";
                }
            }
            catch
            {
                LoginError = "Erro ao conectar ao servidor. Verifique sua ligação de rede.";
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task ExecutarCadastroAsync()
        {
            App.LogInfo($"Tentativa para {LoginEmail}", "CADASTRO");
            if (!await ValidarEmailAsync(LoginEmail))
            {
                MessageBox.Show(EmailError, "E-mail inválido", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (string.IsNullOrWhiteSpace(CadastroNome))
            {
                MessageBox.Show("Preencha o nome completo.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            if (string.IsNullOrWhiteSpace(LoginSenha))
            {
                MessageBox.Show("Preencha a senha.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var novoUser = new { Nome = CadastroNome, Email = LoginEmail, Senha = LoginSenha, Acesso = CadastroPerfil };
            try
            {
                IsBusy = true;
                var response = await _httpClient.PostAsJsonAsync("api/dados/cadastrar", novoUser);
                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("Conta criada com sucesso!\nJá pode fazer login.", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                    ModoCadastro = false;
                    CadastroNome = "";
                    LoginSenha = "";
                }
                else
                {
                    var erro = await response.Content.ReadAsStringAsync();
                    MessageBox.Show($"Erro ao criar conta: {erro}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch
            {
                MessageBox.Show("Erro de conexão. Verifique sua rede.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private void ExecutarLogout()
        {
            App.LogInfo($"Usuário {NomeUsuario} desconectado", "LOGOUT");
            IsLoggedIn = false;
            IsAdmin = false;
            NomeUsuario = "Visitante";
            PerfilUsuario = "Sessão não iniciada";
            LoginEmail = "";
            LoginSenha = "";
            EmailError = "";
            LoginError = "";
            EmailValido = false;
            AbaAtiva = "Dashboard";
        }

        private async Task CarregarTudoAsync()
        {
            App.LogInfo("Carregando todos os dados...", "CARREGAR");
            try
            {
                await Dashboard.AtualizarPegadaMediaAsync();
                await Rastreabilidade.CarregarVeiculosAsync();
                await Fornecedor.CarregarFornecedoresAsync();
                await Pecas.CarregarFornecedoresAsync();
                await Pecas.CarregarVinsAsync();
                await Pecas.CarregarPecasAsync();
                await Analises.AtualizarAsync();
            }
            catch (Exception ex)
            {
                App.LogError($"Falha ao carregar alguns dados: {ex.Message}", "CARREGAR");
            }
        }

        private async Task ExecutarEsqueciSenhaAsync()
        {
            LoginError = "";
            if (string.IsNullOrWhiteSpace(LoginEmail))
            {
                LoginError = "Por favor, preencha o campo de e-mail corporativo para iniciar a redefinição.";
                return;
            }
            try
            {
                IsBusy = true;
                var response = await _httpClient.PostAsJsonAsync("api/dados/recuperar-senha", new { Email = LoginEmail });
                if (response.IsSuccessStatusCode)
                {
                    MessageBox.Show("As instruções de recuperação foram despachadas para o seu e-mail corporativo.", "Recuperação", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    LoginError = "O e-mail informado não foi localizado na base da Iveco.";
                }
            }
            catch
            {
                LoginError = "Falha na comunicação. Não foi possível registrar a requisição.";
            }
            finally
            {
                IsBusy = false;
            }
        }
    }
}