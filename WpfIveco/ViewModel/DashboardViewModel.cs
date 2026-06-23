using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using WpfIveco.ViewModels;

namespace WpfIveco.ViewModels
{
    /// <summary>
    /// ViewModel para o Dashboard principal.
    /// Gerencia a exibição da pegada média de carbono.
    /// </summary>
    public class DashboardViewModel : ViewModelBase
    {
        private readonly HttpClient _httpClient;
        private string _pegadaMediaFormatada = "Carregando...";

        /// <summary>
        /// Pegada média de carbono formatada para exibição (ex: "560.3K" ou "160.6 kg CO2").
        /// </summary>
        public string PegadaMediaFormatada
        {
            get => _pegadaMediaFormatada;
            set { _pegadaMediaFormatada = value; OnPropertyChanged(); }
        }

        /// <summary>
        /// Inicializa o ViewModel com o HttpClient para comunicação com a API.
        /// </summary>
        /// <param name="httpClient">Cliente HTTP para requisições à API.</param>
        public DashboardViewModel(HttpClient httpClient)
        {
            App.LogInfo("Construtor", "DASH");
            _httpClient = httpClient;
        }

        /// <summary>
        /// Atualiza a pegada média de carbono consultando o endpoint /pegada-media.
        /// Formata o valor com sufixo K (milhar) ou kg CO2.
        /// </summary>
        public async Task AtualizarPegadaMediaAsync()
        {
            App.LogInfo("Atualizando pegada média...", "DASH");
            try
            {
                var response = await _httpClient.GetAsync("api/dados/pegada-media");
                App.LogInfo($"GET pegada-media → {(int)response.StatusCode}", "DASH");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(json);
                    var media = doc.RootElement.GetProperty("pegadaMedia").GetDouble();

                    // Formata o valor com base no tamanho
                    if (media >= 1000)
                        PegadaMediaFormatada = (media / 1000).ToString("N1") + "K";
                    else if (media > 0)
                        PegadaMediaFormatada = media.ToString("N1") + " kg CO2";
                    else
                        PegadaMediaFormatada = "0.0 kg CO2";

                    App.LogInfo($"Pegada média: {PegadaMediaFormatada}", "DASH");
                }
                else
                {
                    App.LogError($"Falha: HTTP {response.StatusCode}", "DASH");
                    PegadaMediaFormatada = "Erro ao carregar";
                }
            }
            catch
            {
                App.LogError("Erro ao carregar pegada média", "DASH");
                PegadaMediaFormatada = "Indisponível";
            }
        }
    }
}