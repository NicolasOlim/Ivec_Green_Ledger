using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using WpfIveco.ViewModels;

namespace WpfIveco.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private HttpClient _httpClient;
        private readonly DispatcherTimer _timer;

        public DashboardViewModel Dashboard { get; }
        public RastreabilidadeViewModel Rastreabilidade { get; }
        public FornecedorViewModel Fornecedor { get; }
        public PecasViewModel Pecas { get; }
        public AnalisesViewModel Analises { get; }
        public RelatoriosViewModel Relatorios { get; }

        private bool _isBusy = false;
        public bool IsBusy { get => _isBusy; set { _isBusy = value; OnPropertyChanged(); } }
        private bool _isLoggedIn = false;
        public bool IsLoggedIn { get => _isLoggedIn; set { _isLoggedIn = value; OnPropertyChanged(); } }
        private bool _isAdmin = false;
        public bool IsAdmin { get => _isAdmin; set { _isAdmin = value; OnPropertyChanged(); } }
        private bool _modoCadastro = false;
        public bool ModoCadastro { get => _modoCadastro; set { _modoCadastro = value; OnPropertyChanged(); } }

        private string _loginEmail = "";
        public string LoginEmail { get => _loginEmail; set { _loginEmail = value; OnPropertyChanged(); } }
        private string _loginSenha = "";
        public string LoginSenha { get => _loginSenha; set { _loginSenha = value; OnPropertyChanged(); } }
        private string _cadastroNome = "";
        public string CadastroNome { get => _cadastroNome; set { _cadastroNome = value; OnPropertyChanged(); } }
        private string _cadastroPerfil = "Usuario";
        public string CadastroPerfil { get => _cadastroPerfil; set { _cadastroPerfil = value; OnPropertyChanged(); } }
        private string _nomeUsuario = "Visitante";
        public string NomeUsuario { get => _nomeUsuario; set { _nomeUsuario = value; OnPropertyChanged(); } }
        private string _perfilUsuario = "Sessão não iniciada";
        public string PerfilUsuario { get => _perfilUsuario; set { _perfilUsuario = value; OnPropertyChanged(); } }

        private string _abaAtiva = "Dashboard";
        public string AbaAtiva { get => _abaAtiva; set { _abaAtiva = value; OnPropertyChanged(); } }

        private string _apiUrlConfig = "https://apiivecogreenledger.runasp.net/";
        public string ApiUrlConfig
        {
            get => _apiUrlConfig;
            set
            {
                _apiUrlConfig = value;
                OnPropertyChanged();
                InicializarHttpClient(value);
            }
        }

        private string _statusSimulador = "Desativado";
        public string StatusSimulador { get => _statusSimulador; set { _statusSimulador = value; OnPropertyChanged(); } }

        public ICommand FazerLoginCommand { get; }
        public ICommand FazerCadastroCommand { get; }
        public ICommand FazerLogoutCommand { get; }
        public ICommand AlternarModoAuthCommand { get; }
        public ICommand MudarAbaCommand { get; }
        public ICommand LigarDesligarSimuladorCommand { get; }

        public MainViewModel()
        {
            App.LogInfo("Construtor iniciado", "MAIN");
            InicializarHttpClient(_apiUrlConfig);

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

            _timer = new DispatcherTimer { Interval = TimeSpan.FromMinutes(2) };
            _timer.Tick += async (s, e) => await CarregarTudoAsync();

            App.LogInfo("Construtor finalizado", "MAIN");
        }

        private void InicializarHttpClient(string baseUrl)
        {
            App.LogInfo($"Inicializando HttpClient com base: {baseUrl}", "MAIN");
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
            };
            _httpClient = new HttpClient(handler) { BaseAddress = new Uri(baseUrl) };
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "IvecoWpfApp/1.0");
        }

        private async Task ExecutarLoginAsync()
        {
            App.LogInfo($"Tentativa para {LoginEmail}", "LOGIN");
            if (string.IsNullOrWhiteSpace(LoginEmail) || string.IsNullOrWhiteSpace(LoginSenha))
            {
                App.LogWarning("Campos vazios – abortando", "LOGIN");
                MessageBox.Show("Preencha o e-mail e a senha.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var credenciais = new { Email = LoginEmail, Senha = LoginSenha };

            try
            {
                IsBusy = true;
                var response = await _httpClient.PostAsJsonAsync("api/dados/login", credenciais);
                App.LogInfo($"Resposta HTTP: {(int)response.StatusCode}", "LOGIN");

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

                    App.LogInfo($"Sucesso – Usuário: {NomeUsuario}, Perfil: {PerfilUsuario}", "LOGIN");
                    await CarregarTudoAsync();
                    _timer.Start();
                }
                else
                {
                    var erro = await response.Content.ReadAsStringAsync();
                    App.LogError($"Falha: {erro}", "LOGIN");
                    MessageBox.Show($"Falha no login: {erro}", "Acesso Negado", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch
            {
                App.LogError("Erro de conexão ou resposta inválida no login", "LOGIN");
                MessageBox.Show("Erro ao conectar ao servidor. Verifique sua rede.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task ExecutarCadastroAsync()
        {
            App.LogInfo($"Tentativa para {LoginEmail}", "CADASTRO");
            if (string.IsNullOrWhiteSpace(CadastroNome) || string.IsNullOrWhiteSpace(LoginEmail) || string.IsNullOrWhiteSpace(LoginSenha))
            {
                App.LogWarning("Campos vazios – abortando", "CADASTRO");
                MessageBox.Show("Preencha todos os campos para criar a conta.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var novoUser = new { Nome = CadastroNome, Email = LoginEmail, Senha = LoginSenha, Acesso = CadastroPerfil };

            try
            {
                IsBusy = true;
                var response = await _httpClient.PostAsJsonAsync("api/dados/cadastrar", novoUser);
                App.LogInfo($"Resposta HTTP: {(int)response.StatusCode}", "CADASTRO");

                if (response.IsSuccessStatusCode)
                {
                    App.LogInfo("Sucesso!", "CADASTRO");
                    MessageBox.Show("Conta criada com sucesso!\nJá pode fazer login.", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                    ModoCadastro = false;
                    CadastroNome = "";
                    LoginSenha = "";
                }
                else
                {
                    var erro = await response.Content.ReadAsStringAsync();
                    App.LogError($"Falha: {erro}", "CADASTRO");
                    MessageBox.Show($"Erro ao criar conta: {erro}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch
            {
                App.LogError("Erro de conexão no cadastro", "CADASTRO");
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
            AbaAtiva = "Dashboard";
            _timer.Stop();
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
                App.LogInfo("Carregamento concluído.", "CARREGAR");
            }
            catch
            {
                App.LogError("Falha ao carregar alguns dados – verifique a API", "CARREGAR");
            }
        }
    }
}