using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using WpfIveco.ViewModels;

namespace WpfIveco.ViewModels
{
    public class DashboardViewModel : ViewModelBase
    {
        private readonly HttpClient _httpClient;
        private string _pegadaMediaFormatada = "Carregando...";

        public string PegadaMediaFormatada
        {
            get => _pegadaMediaFormatada;
            set { _pegadaMediaFormatada = value; OnPropertyChanged(); }
        }

        public DashboardViewModel(HttpClient httpClient)
        {
            App.LogInfo("Construtor", "DASH");
            _httpClient = httpClient;
        }

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