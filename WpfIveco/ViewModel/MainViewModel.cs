using LiveCharts;
using LiveCharts.Wpf;
using Microsoft.Win32;
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
using System.Windows.Media;
using System.Windows.Threading;
using WpfIveco.Models;
using WpfIveco.Models.WpfIveco.Models;
using WpfIveco.ViewModels;
using Microsoft.Win32; 
using System.Diagnostics;
using System.IO;

namespace WpfIveco.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private readonly HttpClient _httpClient;
        private readonly DispatcherTimer _timer;

        
        /// <summary>
        /// VARIÁVEIS - SISTEMA DE LOGIN / SESSÃO
        /// </summary>
        
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

        public ICommand FazerLoginCommand { get; }
        public ICommand FazerCadastroCommand { get; }
        public ICommand FazerLogoutCommand { get; }
        public ICommand AlternarModoAuthCommand { get; }
        public ICommand MudarTipoRelatorioCommand { get; }
        public ICommand GerarRelatorioPdfCommand { get; }

        
        /// <summary>
        /// VARIÁVEIS - NAVEGAÇÃO E SISTEMA
        /// </summary>
        
        private string _abaAtiva = "Dashboard";
        public string AbaAtiva { get => _abaAtiva; set { _abaAtiva = value; OnPropertyChanged(); } }

        private string _apiUrlConfig = "http://localhost:7221/api";
        public string ApiUrlConfig { get => _apiUrlConfig; set { _apiUrlConfig = value; OnPropertyChanged(); } }

        private string _statusSimulador = "Desativado";
        public string StatusSimulador { get => _statusSimulador; set { _statusSimulador = value; OnPropertyChanged(); } }

        
        /// <summary>
        /// VARIÁVEIS - DASHBOARD
        /// </summary>
        
        private string _totalVeiculos = "0";
        public string TotalVeiculos { get => _totalVeiculos; set { _totalVeiculos = value; OnPropertyChanged(); } }

        private string _totalFornecedores = "0";
        public string TotalFornecedores { get => _totalFornecedores; set { _totalFornecedores = value; OnPropertyChanged(); } }

        private string _mediaCarbono = "0.0K";
        public string MediaCarbono { get => _mediaCarbono; set { _mediaCarbono = value; OnPropertyChanged(); } }

        
        /// <summary>
        /// VARIÁVEIS - RASTREABILIDADE / FORNECEDORES / PEÇAS
        /// </summary>
        
        private string _pesquisaVin = "";
        public string PesquisaVin { get => _pesquisaVin; set { _pesquisaVin = value; OnPropertyChanged(); } }

        private ObservableCollection<VeiculoModel> _listaVeiculos = new ObservableCollection<VeiculoModel>();
        public ObservableCollection<VeiculoModel> ListaVeiculos { get => _listaVeiculos; set { _listaVeiculos = value; OnPropertyChanged(); } }

        private string _cnpjBusca = "";
        public string CnpjBusca { get => _cnpjBusca; set { _cnpjBusca = value; OnPropertyChanged(); } }

        private string _nomeFornecedorEncontrado = "";
        public string NomeFornecedorEncontrado { get => _nomeFornecedorEncontrado; set { _nomeFornecedorEncontrado = value; OnPropertyChanged(); } }

        private string _localizacaoFornecedorEncontrado = "";
        public string LocalizacaoFornecedorEncontrado { get => _localizacaoFornecedorEncontrado; set { _localizacaoFornecedorEncontrado = value; OnPropertyChanged(); } }

        private string _mensagemCadastro = "";
        public string MensagemCadastro { get => _mensagemCadastro; set { _mensagemCadastro = value; OnPropertyChanged(); } }

        private string _novaPecaVin = "";
        public string NovaPecaVin { get => _novaPecaVin; set { _novaPecaVin = value; OnPropertyChanged(); } }

        private string _novaPecaNome = "";
        public string NovaPecaNome { get => _novaPecaNome; set { _novaPecaNome = value; OnPropertyChanged(); } }

        private ObservableCollection<PecaModel> _listaPecas = new ObservableCollection<PecaModel>();
        public ObservableCollection<PecaModel> ListaPecas { get => _listaPecas; set { _listaPecas = value; OnPropertyChanged(); } }

        public ICommand MudarAbaCommand { get; }
        public ICommand LigarDesligarSimuladorCommand { get; }
        public ICommand PesquisarVinCommand { get; }
        public ICommand ConsultarCnpjCommand { get; }
        public ICommand SalvarFornecedorCommand { get; }
        public ICommand AdicionarPecaManualCommand { get; }

        private SeriesCollection _emissoesSeries;
        public SeriesCollection EmissoesSeries { get => _emissoesSeries; set { _emissoesSeries = value; OnPropertyChanged(); } }

        private string[] _mesesLabels;
        public string[] MesesLabels { get => _mesesLabels; set { _mesesLabels = value; OnPropertyChanged(); } }

        
        /// <summary>
        /// CONSTRUTOR
        /// </summary>
        
        public MainViewModel()
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
            };
            _httpClient = new HttpClient(handler)
            {
                BaseAddress = new Uri("http://localhost:7221/")
            };

            MudarAbaCommand = new RelayCommand(p => AbaAtiva = p as string);
            LigarDesligarSimuladorCommand = new RelayCommand(p =>
                StatusSimulador = StatusSimulador == "Ativo" ? "Desativado" : "Ativo");

            PesquisarVinCommand = new RelayCommand(async p => await PesquisarVinAsync());
            ConsultarCnpjCommand = new RelayCommand(async p => await BuscarPorCnpjAsync());
            SalvarFornecedorCommand = new RelayCommand(async p => await SalvarFornecedorAsync());
            AlternarModoAuthCommand = new RelayCommand(p => ModoCadastro = !ModoCadastro);
            MudarTipoRelatorioCommand = new RelayCommand(ExecutarMudarTipoRelatorio);
            GerarRelatorioPdfCommand = new RelayCommand(ExecutarGerarRelatorioPdf);
            // ==========================================
            // COMANDO: LOGIN
            // ==========================================
            FazerLoginCommand = new RelayCommand(async p =>
            {
                if (string.IsNullOrWhiteSpace(LoginEmail) || string.IsNullOrWhiteSpace(LoginSenha))
                {
                    MostrarAvisoValidacao("Preencha o e-mail e a senha.");
                    return;
                }

                var credenciais = new { Email = LoginEmail, Senha = LoginSenha };

                try
                {
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

                        _ = CarregarDadosDaApiAsync();
                        _timer.Start();
                    }
                    else
                    {
                        // Loga o erro real no Output do Visual Studio
                        var erroDetalhado = await response.Content.ReadAsStringAsync();
                        Debug.WriteLine($"[ERRO LOGIN] HTTP {(int)response.StatusCode} -> {erroDetalhado}");

                        // Utilizador vê mensagem genérica baseada no status code
                        var mensagem = response.StatusCode switch
                        {
                            System.Net.HttpStatusCode.Unauthorized =>
                                "Credenciais incorretas.\nVerifique o e-mail e a senha.",
                            System.Net.HttpStatusCode.BadRequest =>
                                "Os dados enviados são inválidos.\nVerifique e tente novamente.",
                            _ => "Não foi possível entrar no sistema.\nTente novamente mais tarde."
                        };

                        MessageBox.Show(mensagem, "Acesso Negado",
                            MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                catch (HttpRequestException ex)
                {
                    // Erro técnico completo no Output
                    Debug.WriteLine($"[ERRO CONEXÃO LOGIN] {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}");

                    // Mensagem genérica para o utilizador
                    MessageBox.Show(
                        "Não foi possível conectar ao servidor.\nVerifique a sua ligação e tente novamente.",
                        "Erro de Ligação", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                catch (Exception ex)
                {
                    // Erro inesperado completo no Output
                    Debug.WriteLine($"[ERRO INESPERADO LOGIN] {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}");

                    // Mensagem genérica para o utilizador
                    MessageBox.Show(
                        "Ocorreu um erro inesperado.\nTente novamente ou contacte o suporte.",
                        "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            });

            
            /// COMANDO: CADASTRO
            
            FazerCadastroCommand = new RelayCommand(async p =>
            {
                if (string.IsNullOrWhiteSpace(CadastroNome) ||
                    string.IsNullOrWhiteSpace(LoginEmail) ||
                    string.IsNullOrWhiteSpace(LoginSenha))
                {
                    MostrarAvisoValidacao("Preencha todos os campos para criar a conta.");
                    return;
                }

                var novoUser = new
                {
                    Nome = CadastroNome,
                    Email = LoginEmail,
                    Senha = LoginSenha,
                    Acesso = CadastroPerfil
                };

                try
                {
                    var response = await _httpClient.PostAsJsonAsync("api/dados/cadastrar", novoUser);

                    if (response.IsSuccessStatusCode)
                    {
                        MessageBox.Show(
                            "Conta criada com sucesso!\nJá pode fazer login.",
                            "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                        ModoCadastro = false;
                        CadastroNome = "";
                        LoginSenha = "";
                    }
                    else
                    {
                        var erroDetalhado = await response.Content.ReadAsStringAsync();
                        Debug.WriteLine($"[ERRO CADASTRO] HTTP {(int)response.StatusCode} -> {erroDetalhado}");

                        MessageBox.Show(
                            "Não foi possível criar a conta.\nTente novamente ou contacte o suporte.",
                            "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (HttpRequestException ex)
                {
                    Debug.WriteLine($"[ERRO CONEXÃO CADASTRO] {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}");
                    MessageBox.Show(
                        "Não foi possível conectar ao servidor.\nVerifique a sua ligação e tente novamente.",
                        "Erro de Ligação", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[ERRO INESPERADO CADASTRO] {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}");
                    MessageBox.Show(
                        "Ocorreu um erro inesperado.\nTente novamente ou contacte o suporte.",
                        "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            });

            /// COMANDO: LOGOUT
            
            FazerLogoutCommand = new RelayCommand(p =>
            {
                IsLoggedIn = false;
                IsAdmin = false;
                NomeUsuario = "Visitante";
                PerfilUsuario = "Sessão não iniciada";
                LoginEmail = "";
                AbaAtiva = "Dashboard";
                _timer.Stop();
            });

            
            /// COMANDO: ADICIONAR PEÇA
            
            AdicionarPecaManualCommand = new RelayCommand(async p =>
            {
                if (string.IsNullOrWhiteSpace(NovaPecaNome) || string.IsNullOrWhiteSpace(NovaPecaVin))
                    return;

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
                        ListaPecas.Insert(0, new PecaModel { NomePeca = NovaPecaNome, VinAssociado = NovaPecaVin });
                        NovaPecaNome = "";
                        NovaPecaVin = "";
                        MessageBox.Show("Peça registada com sucesso!", "Sucesso",
                            MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        var erroDetalhado = await response.Content.ReadAsStringAsync();
                        Debug.WriteLine($"[ERRO ADICIONAR PEÇA] HTTP {(int)response.StatusCode} -> {erroDetalhado}");
                        MessageBox.Show(
                            "Não foi possível registar a peça.\nTente novamente.",
                            "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[ERRO INESPERADO PEÇA] {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}");
                    MessageBox.Show(
                        "Ocorreu um erro inesperado.\nTente novamente ou contacte o suporte.",
                        "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            });

            /// Inicializa gráfico vazio
            EmissoesSeries = new SeriesCollection
            {
                new LineSeries
                {
                    Title            = "Escopo 1 (Fábrica)",
                    Values           = new ChartValues<double>(),
                    Stroke           = new SolidColorBrush(Color.FromRgb(10, 132, 255)),
                    Fill             = new SolidColorBrush(Color.FromArgb(50, 10, 132, 255)),
                    PointGeometrySize = 10
                },
                new LineSeries
                {
                    Title            = "Escopo 3 (Cadeia)",
                    Values           = new ChartValues<double>(),
                    Stroke           = new SolidColorBrush(Color.FromRgb(48, 209, 88)),
                    Fill             = new SolidColorBrush(Color.FromArgb(50, 48, 209, 88)),
                    PointGeometrySize = 10
                }
            };

            _timer = new DispatcherTimer { Interval = TimeSpan.FromMinutes(2) };
            _timer.Tick += async (s, e) => await CarregarDadosDaApiAsync();
        }

        
        // MÉTODOS DE COMUNICAÇÃO COM A API
        
        private async Task CarregarDadosDaApiAsync()
        {
            try
            {
                var opcoes = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                var responseVeiculos = await _httpClient.GetAsync("api/dados/veiculos");
                if (responseVeiculos.IsSuccessStatusCode)
                {
                    var json = await responseVeiculos.Content.ReadAsStringAsync();
                    var veiculos = JsonSerializer.Deserialize<List<VeiculoModel>>(json, opcoes);
                    if (veiculos != null)
                    {
                        ListaVeiculos = new ObservableCollection<VeiculoModel>(veiculos);
                        TotalVeiculos = veiculos.Count.ToString();
                        AtualizarGraficoComDadosReais(veiculos);
                    }
                }
                else
                {
                    Debug.WriteLine($"[ERRO CARREGAR VEÍCULOS] HTTP {(int)responseVeiculos.StatusCode}");
                }

                var responseFornecedores = await _httpClient.GetAsync("api/dados/fornecedores");
                if (responseFornecedores.IsSuccessStatusCode)
                {
                    var json = await responseFornecedores.Content.ReadAsStringAsync();
                    var fornecedores = JsonSerializer.Deserialize<List<FornecedorModel>>(json, opcoes);
                    TotalFornecedores = fornecedores?.Count.ToString() ?? "0";
                }
                else
                {
                    Debug.WriteLine($"[ERRO CARREGAR FORNECEDORES] HTTP {(int)responseFornecedores.StatusCode}");
                }

                var responseComponentes = await _httpClient.GetAsync("api/dados/componentes");
                if (responseComponentes.IsSuccessStatusCode)
                {
                    var json = await responseComponentes.Content.ReadAsStringAsync();
                    var componentesApi = JsonSerializer.Deserialize<List<VeiculoComponenteApi>>(json, opcoes);
                    if (componentesApi != null)
                    {
                        var listaMapeada = componentesApi
                            .Select(c => new PecaModel { NomePeca = c.NomePeca, VinAssociado = c.Fk_Veiculo_Vin })
                            .Reverse()
                            .ToList();
                        ListaPecas = new ObservableCollection<PecaModel>(listaMapeada);
                    }
                }
                else
                {
                    Debug.WriteLine($"[ERRO CARREGAR COMPONENTES] HTTP {(int)responseComponentes.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                // Erro real no Output — sem popup para não interromper o utilizador
                Debug.WriteLine($"[ERRO CARREGAR DADOS] {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}");
            }
        }

        private void AtualizarGraficoComDadosReais(List<VeiculoModel> veiculos)
        {
            var ultimosMeses = new List<DateTime>();
            for (int i = 5; i >= 0; i--) ultimosMeses.Add(DateTime.Now.AddMonths(-i));
            MesesLabels = ultimosMeses.Select(m => m.ToString("MMM")).ToArray();

            var valoresEscopo1 = new ChartValues<double>();
            var valoresEscopo3 = new ChartValues<double>();

            double baseEscopo1 = 120.0;
            double baseEscopo3 = 250.0;

            foreach (var mes in ultimosMeses)
            {
                int veiculosNoMes = 0;
                try
                {
                    veiculosNoMes = veiculos.Count(v =>
                        Convert.ToDateTime(v.DataMontagem).Year == mes.Year &&
                        Convert.ToDateTime(v.DataMontagem).Month == mes.Month);
                }
                catch
                {
                    veiculosNoMes = veiculos.Count / 6;
                }

                valoresEscopo1.Add(Math.Round(baseEscopo1 + (veiculosNoMes * 1.8), 1));
                valoresEscopo3.Add(Math.Round(baseEscopo3 + (veiculosNoMes * 3.5), 1));
            }

            if (EmissoesSeries.Count == 2)
            {
                EmissoesSeries[0].Values = valoresEscopo1;
                EmissoesSeries[1].Values = valoresEscopo3;
            }

            if (veiculos.Count > 0)
                MediaCarbono = $"{((valoresEscopo1.Sum() + valoresEscopo3.Sum()) / veiculos.Count):F1}K";
        }

        
        /// <summary>
        /// OUTROS MÉTODOS DE API
        /// </summary>
        /// <returns></returns>
        
        private async Task PesquisarVinAsync()
        {
            if (string.IsNullOrWhiteSpace(PesquisaVin) || PesquisaVin.Length != 17)
            {
                MostrarAvisoValidacao("Introduza um VIN válido com 17 caracteres.");
                return;
            }

            try
            {
                var response = await _httpClient.GetAsync($"api/dados/veiculos/validar-vin/{PesquisaVin}");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(json);
                    var veiculoJson = doc.RootElement.GetProperty("veiculo").GetRawText();

                    var content = new StringContent(veiculoJson, System.Text.Encoding.UTF8, "application/json");
                    var resSalvar = await _httpClient.PostAsync("api/dados/veiculos", content);

                    if (resSalvar.IsSuccessStatusCode)
                    {
                        MessageBox.Show("Veículo IVECO rastreado e guardado no Ledger!",
                            "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                        PesquisaVin = "";
                        _ = CarregarDadosDaApiAsync();
                    }
                    else if (resSalvar.StatusCode == System.Net.HttpStatusCode.Conflict)
                    {
                        MessageBox.Show("Veículo autêntico, mas já estava registado no sistema.",
                            "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                    else
                    {
                        var erroDetalhado = await resSalvar.Content.ReadAsStringAsync();
                        Debug.WriteLine($"[ERRO SALVAR VIN] HTTP {(int)resSalvar.StatusCode} -> {erroDetalhado}");
                        MessageBox.Show(
                            "Não foi possível guardar o veículo.\nTente novamente.",
                            "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else
                {
                    var erroDetalhado = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine($"[ERRO VALIDAR VIN] HTTP {(int)response.StatusCode} -> {erroDetalhado}");
                    MessageBox.Show(
                        "Este VIN não pertence a um veículo Iveco válido.",
                        "Acesso Negado", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"[ERRO CONEXÃO VIN] {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}");
                MessageBox.Show(
                    "Não foi possível conectar ao servidor.\nVerifique a sua ligação.",
                    "Erro de Ligação", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERRO INESPERADO VIN] {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}");
                MessageBox.Show(
                    "Ocorreu um erro inesperado.\nTente novamente ou contacte o suporte.",
                    "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task BuscarPorCnpjAsync()
        {
            var cnpjLimpo = new string(CnpjBusca.Where(char.IsDigit).ToArray());
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
                    var erroDetalhado = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine($"[ERRO BUSCAR CNPJ] HTTP {(int)response.StatusCode} -> {erroDetalhado}");

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

        private async Task SalvarFornecedorAsync()
        {
            if (string.IsNullOrWhiteSpace(NomeFornecedorEncontrado))
            {
                MensagemCadastro = "⚠️ Efetue a consulta primeiro.";
                return;
            }

            var cnpjLimpo = new string(CnpjBusca.Where(char.IsDigit).ToArray());
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
                    _ = CarregarDadosDaApiAsync();
                }
                else
                {
                    var erroDetalhado = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine($"[ERRO SALVAR FORNECEDOR] HTTP {(int)response.StatusCode} -> {erroDetalhado}");
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

        public async Task BaixarRelatorioPdf()
        {
            try
            {
                using (var client = new HttpClient())
                {
                    /// ATENÇÃO: Substitua 7196 pela porta correta da sua API!
                    string urlDaApi = "http://localhost:7221/api/Dados/relatorios/veiculos/pdf";

                    /// Faz o pedido do PDF à API
                    var response = await client.GetAsync(urlDaApi);

                    if (response.IsSuccessStatusCode)
                    {
                        /// Lê o ficheiro PDF em formato de bytes
                        byte[] fileBytes = await response.Content.ReadAsByteArrayAsync();

                        /// Abre a janela do Windows para o utilizador escolher onde guardar
                        SaveFileDialog saveFileDialog = new SaveFileDialog
                        {
                            Filter = "Ficheiro PDF (*.pdf)|*.pdf",
                            FileName = "Relatorio_Veiculos_Iveco.pdf", // Nome sugerido
                            Title = "Guardar Relatório de Veículos"
                        };

                        if (saveFileDialog.ShowDialog() == true)
                        {
                            /// Guarda o ficheiro no disco rígido
                            File.WriteAllBytes(saveFileDialog.FileName, fileBytes);

                            MessageBox.Show("Relatório gerado e guardado com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);

                            /// BÓNUS: Abre o PDF automaticamente no leitor padrão (ex: Edge, Adobe Reader)
                            Process.Start(new ProcessStartInfo
                            {
                                FileName = saveFileDialog.FileName,
                                UseShellExecute = true
                            });
                        }
                    }
                    else
                    {
                        /// Se a API der erro, lemos o motivo
                        string erro = await response.Content.ReadAsStringAsync();
                        MessageBox.Show($"Falha ao gerar o relatório.\nStatus: {response.StatusCode}\nErro: {erro}", "Erro da API");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro interno ao tentar descarregar o PDF:\n{ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        /// <summary>
        /// Método para lidar com os RadioButtons (Erros das linhas 967, 972 e 976)
        /// </summary>
        /// <param name="param"></param>
        private void ExecutarMudarTipoRelatorio(object param)
        {
            /// O 'param' vai receber o CommandParameter do XAML (ex: "Veiculos", "Fornecedores", etc.)
            /// Aqui você pode colocar a lógica para saber qual relatório gerar.
            string tipoEscolhido = param as string;

            /// (Lógica futura entra aqui)
        }

        /// <summary>
        /// Método para lidar com o Botão de Download (Erro da linha 981)
        /// </summary>
        /// <param name="param"></param>
        private async void ExecutarGerarRelatorioPdf(object param)
        {
            // Chama a função de baixar o PDF que criámos anteriormente
            await BaixarRelatorioPdf();
        }

        
        /// <summary>
        /// MÉTODO AUXILIAR — VALIDAÇÃO DE CAMPOS
        /// </summary>
        /// <param name="mensagem"></param>
       
        private void MostrarAvisoValidacao(string mensagem)
        {
            MessageBox.Show(mensagem, "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }
}