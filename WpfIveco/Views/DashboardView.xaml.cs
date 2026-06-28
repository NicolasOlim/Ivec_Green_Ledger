using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using System.Text;
using System.Text.Json;

namespace WpfIveco.Views
{
    public partial class DashboardView : UserControl
    {
        // Instância única de HttpClient é uma boa prática para evitar o esgotamento de sockets (Socket Exhaustion).
        private readonly HttpClient _httpClient;

        // Timer responsável por engatilhar a verificação periódica das APIs.
        private DispatcherTimer _statusTimer;

        public DashboardView()
        {
            InitializeComponent();

            // Configuração do HttpClient exclusivo para este Dashboard.
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://apiivecogreenledger.runasp.net/"),
                Timeout = TimeSpan.FromSeconds(10) // Timeout curto evita que a UI trave por muito tempo
            };

            // Dispara a primeira verificação de status assim que a View for instanciada.
            // O uso do "Discard" (_) indica intencionalmente que não vamos aguardar essa Task no construtor.
            _ = VerificarStatusApis();

            // Configuração do Timer para rodar a cada 30 segundos em segundo plano.
            _statusTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(30) };
            _statusTimer.Tick += async (s, e) => await VerificarStatusApis();
            _statusTimer.Start();
        }

        /// <summary>
        /// Verifica o status de múltiplas APIs simultaneamente de forma assíncrona.
        /// </summary>
        private async Task VerificarStatusApis()
        {
            // Task.WhenAll executa as requisições em paralelo. 
            // Isso é muito mais rápido do que verificar uma API de cada vez, e não trava a interface gráfica (UI).
            await Task.WhenAll(
                SetStatus(StatusBrasilApi, "https://brasilapi.com.br/api/cnpj/v1/00000000000191"),
                SetStatus(StatusGooglePlaces, "https://maps.googleapis.com/maps/api/place/nearbysearch/json?location=-23.5,-46.6&radius=100&key=SUA_CHAVE"),
                SetStatus(StatusNhtsa, "https://vpic.nhtsa.dot.gov/api/vehicles/decodevin/1GNCS18Z2M0115561?format=json"),
                SetStatus(StatusMercadoLivre, "https://api.mercadolibre.com/sites/MLB/search?q=test")
            );
        }

        /// <summary>
        /// Faz um requisição GET na URL fornecida e altera a cor do indicador visual (Elipse) com base no resultado.
        /// </summary>
        private async Task SetStatus(System.Windows.Shapes.Ellipse indicator, string url)
        {
            try
            {
                // HttpCompletionOption.ResponseHeadersRead otimiza a memória, 
                // pois não precisamos baixar o corpo da resposta inteiro só para saber o status.
                var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);

                // Consideramos sucesso qualquer código 2xx ou redirecionamentos 3xx.
                bool ok = response.IsSuccessStatusCode ||
                          ((int)response.StatusCode >= 300 && (int)response.StatusCode < 400);

                // Atualiza a cor da Elipse na UI.
                indicator.Fill = ok
                    ? new SolidColorBrush(Color.FromRgb(16, 185, 129))   // Verde (API Online)
                    : new SolidColorBrush(Color.FromRgb(239, 68, 68));   // Vermelho (API Offline ou Erro)
            }
            catch
            {
                // Se a requisição der timeout ou cair, garantimos que fique vermelho em vez de travar o app.
                indicator.Fill = new SolidColorBrush(Color.FromRgb(239, 68, 68));
            }
        }

        /// <summary>
        /// Evento acionado ao clicar no botão de enviar chamado.
        /// </summary>
        private async void EnviarChamado_Click(object sender, RoutedEventArgs e)
        {
            // VALIDAÇÃO DE SEGURANÇA: Previne NullReferenceException caso o usuário não tenha selecionado um setor.
            if (ComboTipoProblema.SelectedItem == null)
            {
                MessageBox.Show("Por favor, selecione o tipo de problema antes de enviar.", "Aviso",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var setor = ((ComboBoxItem)ComboTipoProblema.SelectedItem).Content.ToString();
            var nome = TxtNome.Text;
            var descricao = TxtDescricao.Text;

            // Cria o objeto anônimo (DTO) e serializa para JSON.
            var dto = new { setor, nome, descricao };
            var json = JsonSerializer.Serialize(dto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                // Dispara o POST para o endpoint configurado no BaseAddress.
                var resposta = await _httpClient.PostAsync("api/chamados", content);

                if (resposta.IsSuccessStatusCode)
                {
                    MessageBox.Show("Chamado enviado com sucesso!", "Sucesso",
                        MessageBoxButton.OK, MessageBoxImage.Information);

                    // Limpa a descrição para o próximo chamado.
                    TxtDescricao.Clear();

                    // TODO: A linha abaixo estava causando o Erro CS0103. 
                    // Se você não tiver um controle chamado x:Name="ToggleChamado" no XAML, apague esta linha.
                    // Caso já tenha criado no XAML, pode descomentar a linha abaixo.

                    // ToggleChamado.IsChecked = false; 
                }
                else
                {
                    MessageBox.Show($"Erro ao enviar chamado. Código: {resposta.StatusCode}", "Erro",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                // Captura problemas de rede (ex: servidor da API fora do ar, sem internet, etc.)
                MessageBox.Show($"Falha na comunicação com o servidor: {ex.Message}", "Erro",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}