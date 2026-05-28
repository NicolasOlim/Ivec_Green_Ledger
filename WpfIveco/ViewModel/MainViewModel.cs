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
using System.Windows.Threading;
using WpfIveco.Models;
using WpfIveco.ViewModels;

namespace WpfIveco.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private readonly HttpClient _httpClient;
        private readonly DispatcherTimer _timer;

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

        private string _mediaCarbono = "1.24K";
        public string MediaCarbono { get => _mediaCarbono; set { _mediaCarbono = value; OnPropertyChanged(); } }

        // ==========================================
        // VARIÁVEIS - RASTREABILIDADE (VEÍCULOS)
        // ==========================================
        private string _pesquisaVin = "";
        public string PesquisaVin { get => _pesquisaVin; set { _pesquisaVin = value; OnPropertyChanged(); } }

        private ObservableCollection<VeiculoModel> _listaVeiculos = new ObservableCollection<VeiculoModel>();
        public ObservableCollection<VeiculoModel> ListaVeiculos { get => _listaVeiculos; set { _listaVeiculos = value; OnPropertyChanged(); } }

        // ==========================================
        // VARIÁVEIS - FORNECEDORES (CADASTRO)
        // ==========================================
        private string _cnpjBusca = "";
        public string CnpjBusca { get => _cnpjBusca; set { _cnpjBusca = value; OnPropertyChanged(); } }

        private string _nomeFornecedorEncontrado = "";
        public string NomeFornecedorEncontrado { get => _nomeFornecedorEncontrado; set { _nomeFornecedorEncontrado = value; OnPropertyChanged(); } }

        private string _localizacaoFornecedorEncontrado = "";
        public string LocalizacaoFornecedorEncontrado { get => _localizacaoFornecedorEncontrado; set { _localizacaoFornecedorEncontrado = value; OnPropertyChanged(); } }

        private string _mensagemCadastro = "";
        public string MensagemCadastro { get => _mensagemCadastro; set { _mensagemCadastro = value; OnPropertyChanged(); } }

        // ==========================================
        // VARIÁVEIS - PEÇAS E COMPONENTES (NOVO)
        // ==========================================
        private string _novaPecaVin = "";
        public string NovaPecaVin { get => _novaPecaVin; set { _novaPecaVin = value; OnPropertyChanged(); } }

        private string _novaPecaNome = "";
        public string NovaPecaNome { get => _novaPecaNome; set { _novaPecaNome = value; OnPropertyChanged(); } }

        private ObservableCollection<PecaModel> _listaPecas = new ObservableCollection<PecaModel>();
        public ObservableCollection<PecaModel> ListaPecas { get => _listaPecas; set { _listaPecas = value; OnPropertyChanged(); } }


        // ==========================================
        // COMANDOS (BOTÕES)
        // ==========================================
        public ICommand MudarAbaCommand { get; }
        public ICommand LigarDesligarSimuladorCommand { get; }
        public ICommand PesquisarVinCommand { get; }
        public ICommand ConsultarCnpjCommand { get; }
        public ICommand SalvarFornecedorCommand { get; }
        public ICommand AdicionarPecaManualCommand { get; } // NOVO COMANDO

        // ==========================================
        // CONSTRUTOR
        // ==========================================
        public MainViewModel()
        {
            // Inicialização de Comandos Básicos
            MudarAbaCommand = new RelayCommand(p => AbaAtiva = p as string);
            LigarDesligarSimuladorCommand = new RelayCommand(p => StatusSimulador = StatusSimulador == "Ativo" ? "Desativado" : "Ativo");

            // Inicialização de Comandos de API
            PesquisarVinCommand = new RelayCommand(async p => await PesquisarVinAsync());
            ConsultarCnpjCommand = new RelayCommand(async p => await BuscarPorCnpjAsync());
            SalvarFornecedorCommand = new RelayCommand(async p => await SalvarFornecedorAsync());

            // -------------------------------------------------------------
            // LÓGICA DE ADICIONAR PEÇA MANUALMENTE
            // -------------------------------------------------------------
            AdicionarPecaManualCommand = new RelayCommand(p =>
            {
                if (string.IsNullOrWhiteSpace(NovaPecaNome) || string.IsNullOrWhiteSpace(NovaPecaVin))
                {
                    MessageBox.Show("⚠️ Por favor, preencha o VIN e o Nome da Peça.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (NovaPecaVin.Length != 17)
                {
                    MessageBox.Show("⚠️ O chassi (VIN) deve conter exatamente 17 caracteres.", "Aviso", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // 1. Adiciona à lista visual no painel (atualiza o ecrã na hora)
                ListaPecas.Insert(0, new PecaModel
                {
                    NomePeca = NovaPecaNome,
                    VinAssociado = NovaPecaVin
                });

                // 2. Limpa os campos para poder digitar a próxima
                NovaPecaNome = "";
                NovaPecaVin = "";

                MessageBox.Show("✅ Peça registada localmente no Ledger com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
            });
            // -------------------------------------------------------------

            // Configuração HTTP (Ignorar erro de certificado SSL em ambiente de desenvolvimento)
            var handler = new HttpClientHandler { ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true };
            _httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://localhost:44353/") }; // VERIFIQUE A SUA PORTA DA API AQUI

            // Carregamento inicial de dados
            _ = CarregarDadosDaApiAsync();

            // Atualização automática a cada 2 minutos (Polling)
            _timer = new DispatcherTimer { Interval = TimeSpan.FromMinutes(2) };
            _timer.Tick += async (s, e) => await CarregarDadosDaApiAsync();
            _timer.Start();
        }

        // ==========================================
        // MÉTODOS DE COMUNICAÇÃO COM A API
        // ==========================================
        private async Task CarregarDadosDaApiAsync()
        {
            try
            {
                var opcoes = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                // Atualizar lista de veículos e contador
                var responseVeiculos = await _httpClient.GetAsync("api/dados/veiculos");
                if (responseVeiculos.IsSuccessStatusCode)
                {
                    var json = await responseVeiculos.Content.ReadAsStringAsync();
                    var veiculos = JsonSerializer.Deserialize<List<VeiculoModel>>(json, opcoes);

                    if (veiculos != null)
                    {
                        ListaVeiculos = new ObservableCollection<VeiculoModel>(veiculos);
                        TotalVeiculos = veiculos.Count.ToString();
                    }
                }

                // Atualizar contador de fornecedores
                var responseFornecedores = await _httpClient.GetAsync("api/dados/fornecedores");
                if (responseFornecedores.IsSuccessStatusCode)
                {
                    var json = await responseFornecedores.Content.ReadAsStringAsync();
                    var fornecedores = JsonSerializer.Deserialize<List<FornecedorModel>>(json, opcoes);
                    TotalFornecedores = fornecedores?.Count.ToString() ?? "0";
                }
            }
            catch
            {
                /* Em caso de falha de conexão oculta o erro do utilizador para não incomodar no timer */
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
                        _ = CarregarDadosDaApiAsync(); // Atualiza a tabela na hora
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
                    _ = CarregarDadosDaApiAsync(); // Atualiza contador
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