using System;
using System.Diagnostics;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

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
            _httpClient = httpClient;
        }

        public async Task AtualizarPegadaMediaAsync()
        {
            try
            {
                var response = await _httpClient.GetAsync("api/dados/pegada-media");
                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    using var doc = JsonDocument.Parse(json);
                    var media = doc.RootElement.GetProperty("pegadaMedia").GetDouble();

                    if (media >= 1000)
                        PegadaMediaFormatada = (media / 1000).ToString("N1") + "K";
                    else
                        PegadaMediaFormatada = media.ToString("N1") + " kg CO2";
                }
                else
                {
                    PegadaMediaFormatada = "Erro ao carregar";
                }
            }
            catch
            {
                PegadaMediaFormatada = "Indisponível";
            }
        }
    }
}