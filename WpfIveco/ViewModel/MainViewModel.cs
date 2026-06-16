using System;
using System.Diagnostics;
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
        private readonly HttpClient _httpClient;
        private readonly DispatcherTimer _timer;

        
        /// <summary>
        /// SUB-VIEWMODELS — cada aba tem o seu
        /// </summary>
        
        public RastreabilidadeViewModel Rastreabilidade { get; }
        public FornecedorViewModel Fornecedor { get; }
        public PecasViewModel Pecas { get; }
        public AnalisesViewModel Analises { get; }
        public RelatoriosViewModel Relatorios { get; }

        
        /// <summary>
        /// VARIÁVEIS - SISTEMA DE LOGIN / SESSÃO
        /// </summary>
        
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



        
        /// <summary>
        /// VARIÁVEIS - NAVEGAÇÃO E SISTEMA
        /// </summary>
        
        private string _abaAtiva = "Dashboard";
        public string AbaAtiva { get => _abaAtiva; set { _abaAtiva = value; OnPropertyChanged(); } }

        private string _apiUrlConfig = "https://localhost:7221/api";
        public string ApiUrlConfig { get => _apiUrlConfig; set { _apiUrlConfig = value; OnPropertyChanged(); } }

        private string _statusSimulador = "Desativado";
        public string StatusSimulador { get => _statusSimulador; set { _statusSimulador = value; OnPropertyChanged(); } }

        
        /// <summary>
        /// COMANDOS
        /// </summary>
        
        public ICommand FazerLoginCommand { get; }
        public ICommand FazerCadastroCommand { get; }
        public ICommand FazerLogoutCommand { get; }
        public ICommand AlternarModoAuthCommand { get; }
        public ICommand MudarAbaCommand { get; }
        public ICommand LigarDesligarSimuladorCommand { get; }

        
        /// <summary>
        /// CONSTRUTOR
        /// </summary>
        
        public MainViewModel()
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
            };
            _httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://localhost:7221/") };

            /// Instancia cada sub-ViewModel partilhando o mesmo HttpClient
            Rastreabilidade = new RastreabilidadeViewModel(_httpClient);
            Fornecedor = new FornecedorViewModel(_httpClient);
            Pecas = new PecasViewModel(_httpClient);
            Analises = new AnalisesViewModel(_httpClient);
            Relatorios = new RelatoriosViewModel(_httpClient);

            /// Comandos de navegação e sistema
            MudarAbaCommand = new RelayCommand(p => AbaAtiva = p as string);
            AlternarModoAuthCommand = new RelayCommand(p => ModoCadastro = !ModoCadastro);
            LigarDesligarSimuladorCommand = new RelayCommand(p =>
                StatusSimulador = StatusSimulador == "Ativo" ? "Desativado" : "Ativo");

            /// Comandos de autenticação
            FazerLoginCommand = new RelayCommand(async p => await ExecutarLoginAsync());
            FazerCadastroCommand = new RelayCommand(async p => await ExecutarCadastroAsync());
            FazerLogoutCommand = new RelayCommand(p => ExecutarLogout());

            /// Timer de atualização automática a cada 2 minutos
            _timer = new DispatcherTimer { Interval = TimeSpan.FromMinutes(2) };
            _timer.Tick += async (s, e) => await CarregarTudoAsync();
        }

        /// LOGIN
        
        private async Task ExecutarLoginAsync()
        {
            if (string.IsNullOrWhiteSpace(LoginEmail) || string.IsNullOrWhiteSpace(LoginSenha))
            {
                MessageBox.Show("Preencha o e-mail e a senha.", "Aviso",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var credenciais = new { Email = LoginEmail, Senha = LoginSenha };

            try
            {
                IsBusy = true; /// Ativa a animação de carregamento

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

                    await CarregarTudoAsync();
                    _timer.Start();
                }
                else
                {
                    var erro = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine($"[ERRO LOGIN] HTTP {(int)response.StatusCode} -> {erro}");

                    var mensagem = response.StatusCode switch
                    {
                        System.Net.HttpStatusCode.Unauthorized => "Credenciais incorretas.\nVerifique o e-mail e a senha.",
                        System.Net.HttpStatusCode.BadRequest => "Os dados enviados são inválidos.\nVerifique e tente novamente.",
                        _ => "Não foi possível entrar no sistema.\nTente novamente mais tarde."
                    };

                    MessageBox.Show(mensagem, "Acesso Negado", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"[ERRO CONEXÃO LOGIN] {ex.GetType().Name}: {ex.Message}");
                MessageBox.Show("Não foi possível conectar ao servidor.\nVerifique a sua ligação e tente novamente.", "Erro de Ligação", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERRO INESPERADO LOGIN] {ex.GetType().Name}: {ex.Message}");
                MessageBox.Show("Ocorreu um erro inesperado.\nTente novamente ou contacte o suporte.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false; // Desativa a animação, independentemente do sucesso ou falha
            }
        }

        
        /// <summary>
        /// CADASTRO
        /// </summary>
        /// <returns></returns>
        
        private async Task ExecutarCadastroAsync()
        {
            if (string.IsNullOrWhiteSpace(CadastroNome) || string.IsNullOrWhiteSpace(LoginEmail) || string.IsNullOrWhiteSpace(LoginSenha))
            {
                MessageBox.Show("Preencha todos os campos para criar a conta.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var novoUser = new { Nome = CadastroNome, Email = LoginEmail, Senha = LoginSenha, Acesso = CadastroPerfil };

            try
            {
                IsBusy = true; /// Ativa a animação de carregamento

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
                    Debug.WriteLine($"[ERRO CADASTRO] HTTP {(int)response.StatusCode} -> {erro}");
                    MessageBox.Show("Não foi possível criar a conta.\nTente novamente ou contacte o suporte.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"[ERRO CONEXÃO CADASTRO] {ex.GetType().Name}: {ex.Message}");
                MessageBox.Show("Não foi possível conectar ao servidor.\nVerifique a sua ligação e tente novamente.", "Erro de Ligação", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            finally
            {
                IsBusy = false; // Desativa a animação
            }
        }

        
        /// <summary>
        /// LOGOUT
        /// </summary>
        
        private void ExecutarLogout()
        {
            IsLoggedIn = false;
            IsAdmin = false;
            NomeUsuario = "Visitante";
            PerfilUsuario = "Sessão não iniciada";
            LoginEmail = "";
            AbaAtiva = "Dashboard";
            _timer.Stop();
        }

        
        /// <summary>
        /// CARREGAMENTO GLOBAL — chama todos os sub-ViewModels
        /// </summary>
        /// <returns></returns>
        
        private async Task CarregarTudoAsync()
        {
            await Rastreabilidade.CarregarVeiculosAsync();
            await Fornecedor.CarregarFornecedoresAsync();
            await Pecas.CarregarPecasAsync();
            await Analises.AtualizarAsync(Rastreabilidade.ListaVeiculos.ToList());
        }
    }
}