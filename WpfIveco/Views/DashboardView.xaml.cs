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
        private readonly HttpClient _httpClient;
        private DispatcherTimer _statusTimer;

        public DashboardView()
        {
            InitializeComponent();

            // HttpClient próprio do Dashboard – sem depender do App
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://apiivecogreenledger.runasp.net/"),
                Timeout = TimeSpan.FromSeconds(10)
            };

            // Inicia verificação de status imediatamente
            _ = VerificarStatusApis();

            // Timer para verificar a cada 30 segundos
            _statusTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(30) };
            _statusTimer.Tick += async (s, e) => await VerificarStatusApis();
            _statusTimer.Start();
        }

        private async Task VerificarStatusApis()
        {
            // Executa todas as verificações ao mesmo tempo (não bloqueia a UI)
            await Task.WhenAll(
                SetStatus(StatusBrasilApi, "https://brasilapi.com.br/api/cnpj/v1/00000000000191"),
                SetStatus(StatusGooglePlaces, "https://maps.googleapis.com/maps/api/place/nearbysearch/json?location=-23.5,-46.6&radius=100&key=SUA_CHAVE"),
                SetStatus(StatusNhtsa, "https://vpic.nhtsa.dot.gov/api/vehicles/decodevin/1GNCS18Z2M0115561?format=json"),
                SetStatus(StatusMercadoLivre, "https://api.mercadolibre.com/sites/MLB/search?q=test")
            );
        }

        private async Task SetStatus(System.Windows.Shapes.Ellipse indicator, string url)
        {
            try
            {
                var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
                bool ok = response.IsSuccessStatusCode ||
                          ((int)response.StatusCode >= 300 && (int)response.StatusCode < 400);

                indicator.Fill = ok
                    ? new SolidColorBrush(Color.FromRgb(16, 185, 129))   // Verde (igual ao design do XAML)
                    : new SolidColorBrush(Color.FromRgb(239, 68, 68));   // Vermelho 
            }
            catch
            {
                indicator.Fill = new SolidColorBrush(Color.FromRgb(239, 68, 68));
            }
        }

        private async void EnviarChamado_Click(object sender, RoutedEventArgs e)
        {
            // Alterado de ComboSetor para ComboTipoProblema
            var setor = ((ComboBoxItem)ComboTipoProblema.SelectedItem).Content.ToString();
            var nome = TxtNome.Text;
            var descricao = TxtDescricao.Text;

            var dto = new { setor, nome, descricao };
            var json = JsonSerializer.Serialize(dto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            try
            {
                var resposta = await _httpClient.PostAsync("api/chamados", content);
                if (resposta.IsSuccessStatusCode)
                {
                    MessageBox.Show("Chamado enviado com sucesso!", "Sucesso",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    TxtDescricao.Clear();
                    ToggleChamado.IsChecked = false; // Fecha o painel automaticamente após enviar
                }
                else
                {
                    MessageBox.Show($"Erro ao enviar chamado. Código: {resposta.StatusCode}", "Erro",
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Falha na comunicação: {ex.Message}", "Erro",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}