using LiveCharts;
using LiveCharts.Wpf;
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
using System.Windows.Media;
using System.Windows.Threading;
using WpfIveco.Models;
using WpfIveco.Models.WpfIveco.Models;
using WpfIveco.ViewModels;

namespace WpfIveco.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private readonly HttpClient _httpClient;
        private readonly DispatcherTimer _timer;

        // ==========================================
        // VARIÁVEIS - SISTEMA DE LOGIN / SESSÃO
        // ==========================================
        private bool _isLoggedIn = false;
        public bool IsLoggedIn { get => _isLoggedIn; set { _isLoggedIn = value; OnPropertyChanged(); } }

        private bool _isAdmin = false;
        public bool IsAdmin { get => _isAdmin; set { _isAdmin = value; OnPropertyChanged(); } }

        // Variável para alternar entre a Tela de Login e a de Cadastro
        private bool _modoCadastro = false;
        public bool ModoCadastro { get => _modoCadastro; set { _modoCadastro = value; OnPropertyChanged(); } }

        private string _loginEmail = "";
        public string LoginEmail { get => _loginEmail; set { _loginEmail = value; OnPropertyChanged(); } }

        private string _loginSenha = "";
        public string LoginSenha { get => _loginSenha; set { _loginSenha = value; OnPropertyChanged(); } }

        // Variáveis exclusivas para o momento do Cadastro
        private string _cadastroNome = "";
        public string CadastroNome { get => _cadastroNome; set { _cadastroNome = value; OnPropertyChanged(); } }

        private string _cadastroPerfil = "Usuario"; // Padrão
        public string CadastroPerfil { get => _cadastroPerfil; set { _cadastroPerfil = value; OnPropertyChanged(); } }

        private string _nomeUsuario = "Visitante";
        public string NomeUsuario { get => _nomeUsuario; set { _nomeUsuario = value; OnPropertyChanged(); } }

        private string _perfilUsuario = "Sessão não iniciada";
        public string PerfilUsuario { get => _perfilUsuario; set { _perfilUsuario = value; OnPropertyChanged(); } }

        public ICommand FazerLoginCommand { get; }
        public ICommand FazerCadastroCommand { get; }
        public ICommand FazerLogoutCommand { get; }
        public ICommand AlternarModoAuthCommand { get; }

        // ==========================================
        // VARIÁVEIS - NAVEGAÇÃO E SISTEMA
        // ==========================================
        private string _abaAtiva = "Dashboard";
        public string AbaAtiva { get => _abaAtiva; set { _abaAtiva = value; OnPropertyChanged(); } }

        private string _apiUrlConfig = "https://localhost:44353/api";
        public string ApiUrlConfig { get => _apiUrlConfig; set { _apiUrlConfig = value; OnPropertyChanged(); } }

        private string _statusSimulador = "Desativado";
        public string StatusSimulador { get => _statusSimulador; set { _statusSimulador = value; OnPropertyChanged(); } }

        // ==========================================
        // VARIÁVEIS - DASHBOARD
        // ==========================================
        private string _totalVeiculos = "0";
        public string TotalVeiculos { get => _totalVeiculos; set { _totalVeiculos = value; OnPropertyChanged(); } }

        private string _totalFornecedores = "0";
        public string TotalFornecedores { get => _totalFornecedores; set { _totalFornecedores = value; OnPropertyChanged(); } }

        private string _mediaCarbono = "0.0K";
        public string MediaCarbono { get => _mediaCarbono; set { _mediaCarbono = value; OnPropertyChanged(); } }

        // ==========================================
        // VARIÁVEIS - RELATÓRIOS
        // ==========================================
        private string _tipoRelatorioSelecionado = "Veiculos";
        public string TipoRelatorioSelecionado { get => _tipoRelatorioSelecionado; set { _tipoRelatorioSelecionado = value; OnPropertyChanged(); } }

        public ICommand MudarTipoRelatorioCommand { get; }
        public ICommand GerarRelatorioPdfCommand { get; }

        // ==========================================
        // VARIÁVEIS - RASTREABILIDADE / FORNECEDORES / PEÇAS
        // ==========================================
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

        // ==========================================
        // CONSTRUTOR
        // ==========================================
        public MainViewModel()
        {
            var handler = new HttpClientHandler { ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true };
            _httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://localhost:7221/") };
            MudarTipoRelatorioCommand = new RelayCommand(p => TipoRelatorioSelecionado = p?.ToString());
            GerarRelatorioPdfCommand = new RelayCommand(async p => await GerarRelatorioPdfAsync());

            MudarAbaCommand = new RelayCommand(p => AbaAtiva = p as string);
            LigarDesligarSimuladorCommand = new RelayCommand(p => StatusSimulador = StatusSimulador == "Ativo" ? "Desativado" : "Ativo");

            PesquisarVinCommand = new RelayCommand(async p => await PesquisarVinAsync());
            ConsultarCnpjCommand = new RelayCommand(async p => await BuscarPorCnpjAsync());
            SalvarFornecedorCommand = new RelayCommand(async p => await SalvarFornecedorAsync());

            // -------------------------------------------------------------
            // COMANDOS DE AUTENTICAÇÃO (LOGIN E CADASTRO)
            // -------------------------------------------------------------
            AlternarModoAuthCommand = new RelayCommand(p => ModoCadastro = !ModoCadastro);

            FazerLoginCommand = new RelayCommand(async p =>
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
                    var response = await _httpClient.PostAsJsonAsync("api/dados/login", credenciais);

                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        using var doc = JsonDocument.Parse(json);
                        var userJson = doc.RootElement.GetProperty("usuario");

                        NomeUsuario = userJson.GetProperty("nome").GetString();
                        PerfilUsuario = userJson.GetProperty("acesso").GetString();
                        IsAdmin = (PerfilUsuario == "Admin");

                        IsLoggedIn = true;
                        LoginSenha = "";

                        _ = CarregarDadosDaApiAsync();
                        _timer.Start();
                    }
                    else
                    {
                        // Mostra o erro real retornado pela API
                        var erro = await response.Content.ReadAsStringAsync();
                        MessageBox.Show(
                            $"❌ Acesso negado.\nHTTP {(int)response.StatusCode}\n\nDetalhes: {erro}",
                            "Acesso Negado",
                            MessageBoxButton.OK,
                            MessageBoxImage.Error);
                    }
                }
                catch (HttpRequestException ex)
                {
                    MessageBox.Show(
                        $"❌ API inacessível.\nVerifique se o projeto ApiIveco está rodando.\n\nErro: {ex.Message}",
                        "Erro de Conexão",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"Erro inesperado:\n{ex.Message}\n\nInner: {ex.InnerException?.Message}",
                        "Erro Crítico",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            });

            FazerCadastroCommand = new RelayCommand(async p =>
            {
                if (string.IsNullOrWhiteSpace(CadastroNome) || string.IsNullOrWhiteSpace(LoginEmail) || string.IsNullOrWhiteSpace(LoginSenha))
                {
                    MessageBox.Show("Preencha todos os campos para criar a conta.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Cria o payload que a API espera
                var novoUser = new
                {
                    Nome = CadastroNome,
                    Email = LoginEmail,
                    Senha = LoginSenha,
                    Acesso = CadastroPerfil
                };

                try
                {
                    var response = await _httpClient.PostAsJsonAsync("api/dados/cadastrar", novoUser); ;
                    if (response.IsSuccessStatusCode)
                    {
                        MessageBox.Show("✅ Conta criada com sucesso! Já pode fazer login.", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                        ModoCadastro = false; // Volta para a tela de Login
                        CadastroNome = "";
                        LoginSenha = "";
                    }
                    else
                    {
                        var erroJson = await response.Content.ReadAsStringAsync();
                        MessageBox.Show($"❌ Erro ao criar conta.\nDetalhes: {erroJson}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Falha de comunicação com a API:\n{ex.Message}", "Erro Crítico", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            });

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

            // Inicializa Gráfico Vazio
            EmissoesSeries = new SeriesCollection {
                new LineSeries { Title = "Escopo 1 (Fábrica)", Values = new ChartValues<double>(), Stroke = new SolidColorBrush(Color.FromRgb(10, 132, 255)), Fill = new SolidColorBrush(Color.FromArgb(50, 10, 132, 255)), PointGeometrySize = 10 },
                new LineSeries { Title = "Escopo 3 (Cadeia)", Values = new ChartValues<double>(), Stroke = new SolidColorBrush(Color.FromRgb(48, 209, 88)), Fill = new SolidColorBrush(Color.FromArgb(50, 48, 209, 88)), PointGeometrySize = 10 }
            };

            AdicionarPecaManualCommand = new RelayCommand(async p =>
            {
                if (string.IsNullOrWhiteSpace(NovaPecaNome) || string.IsNullOrWhiteSpace(NovaPecaVin)) return;
                var novaPecaParaApi = new { Id = Guid.NewGuid().ToString().Substring(0, 8), NomePeca = NovaPecaNome, Fk_Veiculo_Vin = NovaPecaVin, Fk_LoteMateriaPrima_Id = "LOTE-MANUAL-" + DateTime.Now.ToString("yyyyMMdd") };
                try
                {
                    var response = await _httpClient.PostAsJsonAsync("api/dados/componentes", novaPecaParaApi);
                    if (response.IsSuccessStatusCode)
                    {
                        ListaPecas.Insert(0, new PecaModel { NomePeca = NovaPecaNome, VinAssociado = NovaPecaVin });
                        NovaPecaNome = ""; NovaPecaVin = "";
                        MessageBox.Show("✅ Peça registada no Firebase com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch (Exception ex) { MessageBox.Show(ex.Message); }
            });

            _timer = new DispatcherTimer { Interval = TimeSpan.FromMinutes(2) };
            _timer.Tick += async (s, e) => await CarregarDadosDaApiAsync();
        }

        // ==========================================
        // MÉTODOS DE COMUNICAÇÃO COM A API
        // ==========================================
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

                var responseFornecedores = await _httpClient.GetAsync("api/dados/fornecedores");
                if (responseFornecedores.IsSuccessStatusCode)
                {
                    var json = await responseFornecedores.Content.ReadAsStringAsync();
                    var fornecedores = JsonSerializer.Deserialize<List<FornecedorModel>>(json, opcoes);
                    TotalFornecedores = fornecedores?.Count.ToString() ?? "0";
                }

                var responseComponentes = await _httpClient.GetAsync("api/dados/componentes");
                if (responseComponentes.IsSuccessStatusCode)
                {
                    var json = await responseComponentes.Content.ReadAsStringAsync();
                    var componentesApi = JsonSerializer.Deserialize<List<VeiculoComponenteApi>>(json, opcoes);
                    if (componentesApi != null)
                    {
                        var listaMapeada = componentesApi.Select(c => new PecaModel { NomePeca = c.NomePeca, VinAssociado = c.Fk_Veiculo_Vin }).Reverse().ToList();
                        ListaPecas = new ObservableCollection<PecaModel>(listaMapeada);
                    }
                }
            }
            catch { }
        }

        private void AtualizarGraficoComDadosReais(List<VeiculoModel> veiculos)
        {
            var ultimosMeses = new List<DateTime>();
            for (int i = 5; i >= 0; i--) ultimosMeses.Add(DateTime.Now.AddMonths(-i));
            MesesLabels = ultimosMeses.Select(m => m.ToString("MMM")).ToArray();

            var valoresEscopo1 = new ChartValues<double>();
            var valoresEscopo3 = new ChartValues<double>();

            double baseEscopo1 = 120.0; double baseEscopo3 = 250.0;

            foreach (var mes in ultimosMeses)
            {
                int veiculosNoMes = 0;
                try { veiculosNoMes = veiculos.Count(v => Convert.ToDateTime(v.DataMontagem).Year == mes.Year && Convert.ToDateTime(v.DataMontagem).Month == mes.Month); }
                catch { veiculosNoMes = veiculos.Count / 6; }
                valoresEscopo1.Add(Math.Round(baseEscopo1 + (veiculosNoMes * 1.8), 1));
                valoresEscopo3.Add(Math.Round(baseEscopo3 + (veiculosNoMes * 3.5), 1));
            }

            if (EmissoesSeries.Count == 2) { EmissoesSeries[0].Values = valoresEscopo1; EmissoesSeries[1].Values = valoresEscopo3; }
            if (veiculos.Count > 0) MediaCarbono = $"{((valoresEscopo1.Sum() + valoresEscopo3.Sum()) / veiculos.Count):F1}K";
        }

        // ==========================================
        // OUTROS MÉTODOS DE API
        // ==========================================

        // ==========================================
        // MÉTODO: GERAR E BAIXAR RELATÓRIO PDF
        // ==========================================
        private async Task GerarRelatorioPdfAsync()
        {
            string endpoint = "";
            string defaultFileName = "";

            // Verifica qual relatório o usuário escolheu na tela
            if (TipoRelatorioSelecionado == "Veiculos")
            {
                endpoint = "api/dados/relatorios/veiculos/pdf";
                defaultFileName = $"Relatorio_Veiculos_{DateTime.Now:yyyyMMdd_HHmm}.pdf";
            }
            else
            {
                MessageBox.Show("Este relatório ainda não está disponível na API.\nPara implementá-lo, crie um novo endpoint na API nos mesmos moldes do Veículo.", "Em Desenvolvimento", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                // Faz a requisição GET para a API
                var response = await _httpClient.GetAsync(endpoint);

                if (response.IsSuccessStatusCode)
                {
                    // Lê o arquivo recebido em bytes
                    var pdfBytes = await response.Content.ReadAsByteArrayAsync();

                    // Abre a janela padrão do Windows para salvar o arquivo
                    var saveFileDialog = new Microsoft.Win32.SaveFileDialog
                    {
                        Filter = "Arquivos PDF (*.pdf)|*.pdf",
                        FileName = defaultFileName,
                        Title = "Salvar Relatório PDF"
                    };

                    if (saveFileDialog.ShowDialog() == true)
                    {
                        // Salva o arquivo no disco do usuário
                        System.IO.File.WriteAllBytes(saveFileDialog.FileName, pdfBytes);

                        var resultado = MessageBox.Show("Relatório PDF gerado e salvo com sucesso!\nDeseja abrir o arquivo agora?", "Sucesso", MessageBoxButton.YesNo, MessageBoxImage.Information);

                        // Abre o PDF automaticamente no leitor padrão do PC
                        if (resultado == MessageBoxResult.Yes)
                        {
                            var process = new System.Diagnostics.Process();
                            process.StartInfo = new System.Diagnostics.ProcessStartInfo(saveFileDialog.FileName)
                            {
                                UseShellExecute = true // Necessário no .NET 8 para abrir arquivos
                            };
                            process.Start();
                        }
                    }
                }
                else
                {
                    MessageBox.Show("Erro ao solicitar a geração do relatório à API.", "Falha", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro crítico ao comunicar com o servidor:\n{ex.Message}", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task PesquisarVinAsync()
        {
            if (string.IsNullOrWhiteSpace(PesquisaVin) || PesquisaVin.Length != 17)
            {
                MessageBox.Show("⚠️ Introduza um VIN válido com 17 caracteres.", "Erro de Validação", MessageBoxButton.OK, MessageBoxImage.Warning);
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
                        MessageBox.Show("✅ Veículo IVECO rastreado e guardado no Ledger!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                        PesquisaVin = "";
                        _ = CarregarDadosDaApiAsync();
                    }
                    else if (resSalvar.StatusCode == System.Net.HttpStatusCode.Conflict)
                    {
                        MessageBox.Show("⚠️ Veículo autêntico, mas já estava registado no sistema.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                else
                {
                    MessageBox.Show("❌ Este número de VIN não pertence a um veículo Iveco válido ou ocorreu um erro.", "Acesso Negado", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Falha de comunicação: {ex.Message}", "Erro Crítico", MessageBoxButton.OK, MessageBoxImage.Error);
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
                    MensagemCadastro = "❌ CNPJ não encontrado na base de dados.";
                    NomeFornecedorEncontrado = "";
                    LocalizacaoFornecedorEncontrado = "";
                }
            }
            catch (Exception ex)
            {
                MensagemCadastro = $"Erro de API: {ex.Message}";
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
                    MensagemCadastro = $"❌ Erro ao guardar fornecedor (HTTP {response.StatusCode}).";
                }
            }
            catch (Exception ex)
            {
                MensagemCadastro = $"Erro: {ex.Message}";
            }
        }
    }
}