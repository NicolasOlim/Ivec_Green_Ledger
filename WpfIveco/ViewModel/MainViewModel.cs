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

namespace WpfIveco.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly HttpClient _httpClient;
        private readonly DispatcherTimer _timer;

        // --- VARIÁVEIS DA TELA DASHBOARD ---
        private string _totalVeiculos = "A carregar...";
        public string TotalVeiculos { get => _totalVeiculos; set { _totalVeiculos = value; OnPropertyChanged(); } }

        private string _totalFornecedores = "A carregar...";
        public string TotalFornecedores { get => _totalFornecedores; set { _totalFornecedores = value; OnPropertyChanged(); } }

        // --- VARIÁVEIS DA TELA CHÃO DE FÁBRICA / PROJETOS (RASTREABILIDADE) ---
        private string _pesquisaVin = "";
        public string PesquisaVin { get => _pesquisaVin; set { _pesquisaVin = value; OnPropertyChanged(); } }

        // Nova lista que vai alimentar a interface gráfica
        private ObservableCollection<VeiculoModel> _listaVeiculos = new ObservableCollection<VeiculoModel>();
        public ObservableCollection<VeiculoModel> ListaVeiculos
        {
            get => _listaVeiculos;
            set { _listaVeiculos = value; OnPropertyChanged(); }
        }

        // Controla se a mensagem "Nenhum veículo" aparece ou não
        private bool _listaVazia = true;
        public bool ListaVazia
        {
            get => _listaVazia;
            set { _listaVazia = value; OnPropertyChanged(); }
        }

        // --- VARIÁVEIS DA TELA AJUSTES ---
        private string _apiUrlConfig = "https://localhost:44353/api";
        public string ApiUrlConfig { get => _apiUrlConfig; set { _apiUrlConfig = value; OnPropertyChanged(); } }

        private string _statusSimulador = "Parado";
        public string StatusSimulador { get => _statusSimulador; set { _statusSimulador = value; OnPropertyChanged(); } }

        // --- VARIÁVEIS DA TELA CADASTRO (BRASILAPI) ---
        private string _cnpjBusca = "";
        public string CnpjBusca { get => _cnpjBusca; set { _cnpjBusca = value; OnPropertyChanged(); } }

        private string _nomeFornecedorEncontrado = "";
        public string NomeFornecedorEncontrado { get => _nomeFornecedorEncontrado; set { _nomeFornecedorEncontrado = value; OnPropertyChanged(); } }

        private string _localizacaoFornecedorEncontrado = "";
        public string LocalizacaoFornecedorEncontrado { get => _localizacaoFornecedorEncontrado; set { _localizacaoFornecedorEncontrado = value; OnPropertyChanged(); } }

        private string _mensagemCadastro = "Insira o CNPJ da empresa e clique em Consultar.";
        public string MensagemCadastro { get => _mensagemCadastro; set { _mensagemCadastro = value; OnPropertyChanged(); } }

        // --- NAVEGAÇÃO ---
        private string _abaAtiva = "Dashboard";
        public string AbaAtiva { get => _abaAtiva; set { _abaAtiva = value; OnPropertyChanged(); } }

        // --- COMANDOS DOS BOTÕES ---
        public ICommand MudarAbaCommand { get; }
        public ICommand PesquisarVinCommand { get; }
        public ICommand LigarDesligarSimuladorCommand { get; }
        public ICommand LimparBaseDadosCommand { get; }
        public ICommand ConsultarCnpjCommand { get; }
        public ICommand SalvarFornecedorCommand { get; }

        public MainViewModel()
        {
            MudarAbaCommand = new RelayCommand(MudarAba);
            LigarDesligarSimuladorCommand = new RelayCommand(p => StatusSimulador = StatusSimulador == "A Correr" ? "Parado" : "A Correr");
            LimparBaseDadosCommand = new RelayCommand(p => MessageBox.Show("Base de dados local limpa com sucesso!", "Gestão de Dados"));

            PesquisarVinCommand = new RelayCommand(async p => await PesquisarVinAsync());
            ConsultarCnpjCommand = new RelayCommand(async p => await BuscarPorCnpjAsync());
            SalvarFornecedorCommand = new RelayCommand(async p => await SalvarFornecedorAsync());

            var handler = new HttpClientHandler { ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true };
            _httpClient = new HttpClient(handler) { BaseAddress = new Uri("https://localhost:44353/") };

            _ = CarregarDadosDaApiAsync();

            _timer = new DispatcherTimer { Interval = TimeSpan.FromMinutes(5) };
            _timer.Tick += async (sender, args) => await CarregarDadosDaApiAsync();
            _timer.Start();
        }

        private void MudarAba(object parametro)
        {
            if (parametro is string nomeDaAba) AbaAtiva = nomeDaAba;
        }

        private async Task CarregarDadosDaApiAsync()
        {
            try
            {
                var jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                // CARREGAR VEÍCULOS
                var responseVeiculos = await _httpClient.GetAsync("api/dados/veiculos");
                if (responseVeiculos.IsSuccessStatusCode)
                {
                    var json = await responseVeiculos.Content.ReadAsStringAsync();
                    var veiculos = JsonSerializer.Deserialize<List<VeiculoModel>>(json, jsonOptions);

                    TotalVeiculos = veiculos?.Count.ToString() ?? "0";

                    // === NOVIDADE: ALIMENTAR A LISTA NA TELA DE RASTREABILIDADE ===
                    if (veiculos != null && veiculos.Count > 0)
                    {
                        ListaVeiculos = new ObservableCollection<VeiculoModel>(veiculos);
                        ListaVazia = false;
                    }
                    else
                    {
                        ListaVeiculos.Clear();
                        ListaVazia = true;
                    }
                }
                else { TotalVeiculos = $"Erro {responseVeiculos.StatusCode}"; }

                // CARREGAR FORNECEDORES
                var responseFornecedores = await _httpClient.GetAsync("api/dados/fornecedores");
                if (responseFornecedores.IsSuccessStatusCode)
                {
                    var json = await responseFornecedores.Content.ReadAsStringAsync();
                    var fornecedores = JsonSerializer.Deserialize<List<FornecedorModel>>(json, jsonOptions);
                    TotalFornecedores = fornecedores?.Count.ToString() ?? "0";
                }
                else { TotalFornecedores = $"Erro {responseFornecedores.StatusCode}"; }
            }
            catch (Exception)
            {
                TotalVeiculos = "Falha de Conexão";
                TotalFornecedores = "Falha de Conexão";
            }
        }

        private async Task PesquisarVinAsync()
        {
            if (string.IsNullOrWhiteSpace(PesquisaVin) || PesquisaVin.Length != 17)
            {
                MessageBox.Show("⚠️ Por favor, introduza um VIN válido de 17 caracteres.", "VIN Inválido", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                var respostaApi = await _httpClient.GetAsync($"api/dados/veiculos/validar-vin/{PesquisaVin}");
                var json = await respostaApi.Content.ReadAsStringAsync();

                if (respostaApi.IsSuccessStatusCode)
                {
                    using var doc = JsonDocument.Parse(json);
                    var veiculoElement = doc.RootElement.GetProperty("veiculo");

                    var opcoes = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var veiculoCompleto = JsonSerializer.Deserialize<VeiculoModel>(veiculoElement.GetRawText(), opcoes);

                    var respostaGuardar = await _httpClient.PostAsJsonAsync("api/dados/veiculos", veiculoCompleto);

                    if (respostaGuardar.IsSuccessStatusCode)
                    {
                        MessageBox.Show($"✅ Sucesso!\n\nVeículo ({veiculoCompleto.Modelo}) validado e guardado no Firebase!", "Veículo Registado", MessageBoxButton.OK, MessageBoxImage.Information);
                        PesquisaVin = "";
                        _ = CarregarDadosDaApiAsync(); // Atualiza a lista na hora!
                    }
                    else if (respostaGuardar.StatusCode == System.Net.HttpStatusCode.Conflict)
                    {
                        MessageBox.Show($"⚠️ O veículo IVECO ({veiculoCompleto.Vin}) é autêntico, mas já estava registado no Firebase.", "Veículo Duplicado", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                    else
                    {
                        var erroJson = await respostaGuardar.Content.ReadAsStringAsync();
                        MessageBox.Show($"❌ Erro ao guardar no Firebase: HTTP {respostaGuardar.StatusCode}\nDetalhes: {erroJson}", "Erro ao Guardar", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                else if (respostaApi.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    using var doc = JsonDocument.Parse(json);
                    var mensagemErro = doc.RootElement.GetProperty("mensagem").GetString();
                    MessageBox.Show($"❌ BLOQUEIO DE SEGURANÇA:\n\n{mensagemErro}", "Acesso Negado", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    MessageBox.Show($"❌ Erro {respostaApi.StatusCode} ao consultar o VIN.", "Falha", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Erro de comunicação com a API:\n{ex.Message}", "Erro Critico", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task BuscarPorCnpjAsync()
        {
            if (string.IsNullOrWhiteSpace(CnpjBusca))
            {
                MensagemCadastro = "⚠️ Digite o CNPJ para pesquisar.";
                return;
            }

            var cnpjLimpo = new string(CnpjBusca.Where(char.IsDigit).ToArray());
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
                    MensagemCadastro = "✅ Empresa localizada com sucesso!";
                }
                else
                {
                    NomeFornecedorEncontrado = "";
                    LocalizacaoFornecedorEncontrado = "";
                    MensagemCadastro = $"❌ Erro {response.StatusCode}: Verifica a consola da API preta!";
                }
            }
            catch (Exception ex) { MensagemCadastro = $"Erro na comunicação com a API: {ex.Message}"; }
        }

        private async Task SalvarFornecedorAsync()
        {
            if (string.IsNullOrWhiteSpace(NomeFornecedorEncontrado))
            {
                MensagemCadastro = "⚠️ Consulte um CNPJ válido antes de guardar!";
                return;
            }

            MensagemCadastro = "A guardar no Firebase...";
            var cnpjLimpo = new string(CnpjBusca.Where(char.IsDigit).ToArray());

            var novoFornecedor = new
            {
                Id = "GeradoPelaAPI",
                Nome = NomeFornecedorEncontrado,
                Localizacao = LocalizacaoFornecedorEncontrado,
                Cnpj = cnpjLimpo
            };

            try
            {
                var response = await _httpClient.PostAsJsonAsync("api/dados/fornecedores", novoFornecedor);
                if (response.IsSuccessStatusCode)
                {
                    MensagemCadastro = "✅ Fornecedor guardado com sucesso!";
                    CnpjBusca = "";
                    NomeFornecedorEncontrado = "";
                    LocalizacaoFornecedorEncontrado = "";
                    _ = CarregarDadosDaApiAsync();
                }
                else { MensagemCadastro = $"Erro ao guardar (Limite do Firebase?): HTTP {response.StatusCode}"; }
            }
            catch (Exception ex) { MensagemCadastro = $"Erro ao guardar: {ex.Message}"; }
        }
    }
}