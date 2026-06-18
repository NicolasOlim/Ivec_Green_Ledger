using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

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

                    // Formata o valor
                    if (media >= 1000)
                        PegadaMediaFormatada = (media / 1000).ToString("N1") + "K";
                    else
                        PegadaMediaFormatada = media.ToString("N1") + " kg CO2";
                }
                else
                {
                    // Se a resposta não for sucesso, tenta usar dados mockados ou 0
                    PegadaMediaFormatada = "0.0 kg CO2";
                }
            }
            catch
            {
                // Em caso de erro de conexão, exibe 0 (em vez de "Erro ao carregar")
                PegadaMediaFormatada = "0.0 kg CO2";
                System.Diagnostics.Debug.WriteLine("[Dashboard] Falha ao carregar pegada média, usando 0.");
            }
        }
    }
}