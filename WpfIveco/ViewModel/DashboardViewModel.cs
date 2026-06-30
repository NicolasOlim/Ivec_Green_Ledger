using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using WpfIveco.Models;
using WpfIveco.ViewModels;

namespace WpfIveco.ViewModels
{
    /// <summary>
    /// ViewModel para o Dashboard principal.
    /// Gerencia a exibição da pegada média de carbono e métricas gerais do sistema.
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

        // ------------------------------------------------------------
        // NOVAS PROPRIEDADES PARA OS CARDS
        // ------------------------------------------------------------

        private int _consultasHoje = 0;
        /// <summary>Número total de consultas realizadas hoje (ex: total de registros).</summary>
        public int ConsultasHoje
        {
            get => _consultasHoje;
            set { _consultasHoje = value; OnPropertyChanged(); }
        }

        private double _falhasIntegracao = 0.0;
        /// <summary>Percentual de falhas nas integrações.</summary>
        public double FalhasIntegracao
        {
            get => _falhasIntegracao;
            set { _falhasIntegracao = value; OnPropertyChanged(); }
        }

        private int _tempoRespostaMs = 0;
        /// <summary>Tempo médio de resposta da API em milissegundos.</summary>
        public int TempoRespostaMs
        {
            get => _tempoRespostaMs;
            set { _tempoRespostaMs = value; OnPropertyChanged(); }
        }

        private int _usoServidor = 0;
        /// <summary>Percentual de uso do servidor.</summary>
        public int UsoServidor
        {
            get => _usoServidor;
            set { _usoServidor = value; OnPropertyChanged(); }
        }

        private string _variacaoConsultas = "+12%";
        /// <summary>Variação percentual em relação ao dia anterior.</summary>
        public string VariacaoConsultas
        {
            get => _variacaoConsultas;
            set { _variacaoConsultas = value; OnPropertyChanged(); }
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
        /// Também obtém veículos e peças para calcular o total de consultas e outras métricas.
        /// Formata o valor com sufixo K (milhar) ou kg CO2.
        /// </summary>
        public async Task AtualizarPegadaMediaAsync()
        {
            App.LogInfo("Atualizando pegada média...", "DASH");
            try
            {
                // 1. PEGADA MÉDIA
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

                // 2. CONSULTAS HOJE (total de veículos + peças, por exemplo)
                var veiculos = await _httpClient.GetFromJsonAsync<List<VeiculoModel>>("api/dados/veiculos");
                var pecas = await _httpClient.GetFromJsonAsync<List<PecaModel>>("api/dados/componentes");
                var totalConsultas = (veiculos?.Count ?? 0) + (pecas?.Count ?? 0);
                ConsultasHoje = totalConsultas;

                // 3. FALHAS DE INTEGRAÇÃO (simulação – pode vir de um contador de erros)
                // Aqui você pode buscar um endpoint de monitoramento ou calcular a partir de logs
                FalhasIntegracao = 0.3; // valor fixo de exemplo

                // 4. TEMPO DE RESPOSTA (pode ser medido com um cronômetro nas chamadas)
                TempoRespostaMs = 120; // valor fixo de exemplo

                // 5. USO DO SERVIDOR (pode vir de um endpoint de health check)
                UsoServidor = 42; // valor fixo de exemplo

                // 6. VARIAÇÃO (calculada ou fixa)
                VariacaoConsultas = "+12%";

                App.LogInfo($"Consultas: {ConsultasHoje}, Falhas: {FalhasIntegracao}%, Tempo: {TempoRespostaMs}ms, Servidor: {UsoServidor}%", "DASH");
            }
            catch (Exception ex)
            {
                App.LogError($"Erro ao carregar dados do dashboard: {ex.Message}", "DASH");
                PegadaMediaFormatada = "Indisponível";
                ConsultasHoje = 0;
                FalhasIntegracao = 0;
                TempoRespostaMs = 0;
                UsoServidor = 0;
            }
        }
    }
}