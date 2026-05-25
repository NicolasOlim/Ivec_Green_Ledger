using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using SimuladorIveco.Data;     // Importa o seu AppDbContext
using SimuladorIveco.Models;   // Importa os seus Models
using static SimuladorIveco.Models.Model;

namespace SimuladorIveco
{
    class Program
    {
        // Rota corrigida para apontar para o endpoint base da API
        private const string ApiBaseUrl = "https://localhost:44353/api";
        private static readonly Random _random = new Random();

        static async Task Main(string[] args)
        {
            Console.WriteLine("=== Simulador de Produção IVECO Iniciado ===");

            // 1. GARANTE QUE O BANCO DE DADOS LOCAL EXISTE
            try
            {
                using (var db = new AppDbContext()) // <-- Agora usa AppDbContext!
                {
                    db.Database.EnsureCreated(); // Cria o arquivo .db se ele não existir
                    Console.WriteLine("[BANCO LOCAL] Arquivo SQLite 'dados_gerados.db' verificado/criado com sucesso!\n");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERRO BANCO LOCAL] Falha ao configurar o banco de dados: {ex.Message}\n");
            }

            Console.WriteLine("Enviando dados aleatórios a cada 15 segundos. Pressione [Ctrl+C] para parar.\n");

            using var httpClient = new HttpClient();

            while (true)
            {
                try
                {
                    Console.WriteLine($"--- Iniciando ciclo de envio: {DateTime.Now:HH:mm:ss} ---");

                    // 2. GERAÇÃO DOS DADOS ALEATÓRIOS
                    var fornecedor = GerarFornecedorAleatorio();
                    var lote = GerarLoteAleatorio(fornecedor.Id);
                    var veiculo = GerarVeiculoAleatorio();
                    var componente = GerarComponenteAleatorio(veiculo.Vin, lote.Id);

                    // 3. SALVA OS DADOS NO BANCO LOCAL (SQLITE)
                    using (var db = new AppDbContext()) // <-- Agora usa AppDbContext!
                    {
                        db.Fornecedores.Add(fornecedor);
                        db.Lotes.Add(lote);
                        db.Veiculos.Add(veiculo);
                        db.Componentes.Add(componente);

                        db.SaveChanges(); // Escreve as alterações no arquivo físico local
                        Console.WriteLine("[BANCO LOCAL] Dados guardados no histórico local com sucesso.");
                    }

                    // 4. ENVIA OS DADOS PARA A API
                    await EnviarDados(httpClient, "dados/fornecedores", fornecedor);
                    await EnviarDados(httpClient, "dados/lotes", lote);
                    await EnviarDados(httpClient, "dados/veiculos", veiculo);
                    await EnviarDados(httpClient, "dados/componentes", componente);

                    Console.WriteLine("Ciclo concluído. Aguardando 15 segundos...\n");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"\n[ERRO DE CICLO] {ex.Message}");
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
                Id = _random.Next(1, 1000).ToString(),
                Nome = nomes[_random.Next(nomes.Length)],
                Localizacao = locais[_random.Next(locais.Length)],
                Cnpj = $"{_random.Next(10, 99)}.{_random.Next(100, 999)}.{_random.Next(100, 999)}/0001-{_random.Next(10, 99)}"
            };
        }

        static LoteMateriaPrima GerarLoteAleatorio(string fornecedorId)
        {
            string[] materiais = { "Bobina de Aço", "Tinta Automotiva", "Borracha", "Plástico Injetado", "Fios de Cobre" };

            return new LoteMateriaPrima
            {
                Id = _random.Next(1000, 9999).ToString(),
                TipoMaterial = materiais[_random.Next(materiais.Length)],
                DataProducao = DateTime.UtcNow.AddDays(-_random.Next(1, 30)),
                QuantidadeKg = Math.Round(_random.NextDouble() * 5000 + 100, 2),
                PegadaCarbonoPorKg = Math.Round(_random.NextDouble() * 5 + 0.5, 2),
                fk_Fornecedor_Id = fornecedorId
            };
        }

        static VeiculoComponente GerarComponenteAleatorio(string vin, string loteId)
        {
            string[] pecas = { "Porta Esquerda", "Chassi", "Motor", "Eixo Traseiro", "Painel Frontal", "Cabine" };

            return new VeiculoComponente
            {
                Id = _random.Next(10000, 99999).ToString(),
                fk_Veiculo_Vin = vin,
                fk_LoteMateriaPrima_Id = loteId,
                NomePeca = pecas[_random.Next(pecas.Length)]
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

        // --- MÉTODO DE ENVIO HTTP ---

        static async Task EnviarDados<T>(HttpClient client, string endpoint, T dados)
        {
            HttpResponseMessage response = await client.PostAsJsonAsync($"{ApiBaseUrl}/{endpoint}", dados);

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine($"[OK API] {endpoint.Replace("dados/", "").ToUpper()} registrado no servidor.");
            }
            else
            {
                Console.WriteLine($"[ERRO API] Falha ao salvar {endpoint.Replace("dados/", "")}: HTTP {response.StatusCode}");
            }
        }
    }
}