using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using static SimuladorIveco.Models.Model;

namespace ApiIveco.ConsoleApp
{
    class Program
    {
        // Rota corrigida para apontar para o endpoint base da API
        private const string ApiBaseUrl = "https://localhost:44353/api";
        private static readonly Random _random = new Random();

        static async Task Main(string[] args)
        {
            Console.WriteLine("=== Simulador de Produção IVECO Iniciado ===");
            Console.WriteLine("Enviando dados aleatórios a cada 15 segundos. Pressione [Ctrl+C] para parar.\n");

            using var httpClient = new HttpClient();

            while (true)
            {
                try
                {
                    Console.WriteLine($"--- Iniciando ciclo de envio: {DateTime.Now:HH:mm:ss} ---");

                    // 1. Gera e envia Fornecedor para api/dados/fornecedores
                    var fornecedor = GerarFornecedorAleatorio();
                    await EnviarDados(httpClient, "dados/fornecedores", fornecedor);

                    // 2. Gera e envia Lote para api/dados/lotes
                    var lote = GerarLoteAleatorio(fornecedor.Id);
                    await EnviarDados(httpClient, "dados/lotes", lote);

                    // 3. Gera e envia Veículo para api/dados/veiculos
                    var veiculo = GerarVeiculoAleatorio();
                    await EnviarDados(httpClient, "dados/veiculos", veiculo);

                    // 4. Gera e envia Componente para api/dados/componentes
                    var componente = GerarComponenteAleatorio(veiculo.Vin, lote.Id);
                    await EnviarDados(httpClient, "dados/componentes", componente);

                    Console.WriteLine("Ciclo concluído. Aguardando 15 segundos...\n");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"\n[ERRO DE CONEXÃO] {ex.Message}");
                    Console.WriteLine("Tentando novamente no próximo ciclo...\n");
                }

                await Task.Delay(TimeSpan.FromSeconds(15));
            }
        }

        // --- GERADORES DE DADOS ALEATÓRIOS ---

        static Fornecedor GerarFornecedorAleatorio()
        {
            string[] nomes = { "Usiminas", "Gerdau", "Bosch", "ZF Friedrichshafen", "Michelin" };
            string[] locais = { "Ipatinga - MG", "Ouro Branco - MG", "Campinas - SP", "Sorocaba - SP", "Resende - RJ" };

            return new Fornecedor
            {
                Id = _random.Next(1, 1000),
                Nome = nomes[_random.Next(nomes.Length)],
                Localizacao = locais[_random.Next(locais.Length)],
                Cnpj = $"{_random.Next(10, 99)}.{_random.Next(100, 999)}.{_random.Next(100, 999)}/0001-{_random.Next(10, 99)}"
            };
        }

        static LoteMateriaPrima GerarLoteAleatorio(int fornecedorId)
        {
            string[] materiais = { "Bobina de Aço", "Tinta Automotiva", "Borracha", "Plástico Injetado", "Fios de Cobre" };

            return new LoteMateriaPrima
            {
                Id = _random.Next(1000, 9999),
                TipoMaterial = materiais[_random.Next(materiais.Length)],
                DataProducao = DateTime.UtcNow.AddDays(-_random.Next(1, 30)),
                QuantidadeKg = Math.Round(_random.NextDouble() * 5000 + 100, 2),
                PegadaCarbonoPorKg = Math.Round(_random.NextDouble() * 5 + 0.5, 2),
                fk_Fornecedor_Id = fornecedorId
            };
        }

        static Veiculo GerarVeiculoAleatorio()
        {
            string[] modelos = { "Iveco S-Way", "Iveco Tector", "Iveco Daily", "Iveco Hi-Way" };

            string caracteresVin = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            string vin = "9BCC";
            for (int i = 0; i < 13; i++) vin += caracteresVin[_random.Next(caracteresVin.Length)];

            return new Veiculo
            {
                Vin = vin,
                Modelo = modelos[_random.Next(modelos.Length)],
                DataMontagem = DateTime.UtcNow
            };
        }

        static VeiculoComponente GerarComponenteAleatorio(string vin, int loteId)
        {
            string[] pecas = { "Porta Esquerda", "Chassi", "Motor", "Eixo Traseiro", "Painel Frontal", "Cabine" };

            return new VeiculoComponente
            {
                Id = _random.Next(10000, 99999),
                fk_Veiculo_Vin = vin,
                fk_LoteMateriaPrima_Id = loteId,
                NomePeca = pecas[_random.Next(pecas.Length)]
            };
        }

        // --- MÉTODO DE ENVIO HTTP ---

        static async Task EnviarDados<T>(HttpClient client, string endpoint, T dados)
        {
            HttpResponseMessage response = await client.PostAsJsonAsync($"{ApiBaseUrl}/{endpoint}", dados);

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine($"[OK] {endpoint.Replace("dados/", "").ToUpper()} registrado.");
            }
            else
            {
                Console.WriteLine($"[ERRO] Falha ao salvar {endpoint.Replace("dados/", "")}: HTTP {response.StatusCode}");
            }
        }
    }
}