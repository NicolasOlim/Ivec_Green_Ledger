using ApiIveco.Data;
using ApiIveco.Models;
using Google.Cloud.Firestore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace ApiIveco.Service
{
    public class DadosService
    {
        private readonly ILogger<DadosService> _logger;
        private readonly FireBaseData _firestoreDb;

        private readonly string _collectionFornecedor = "fornecedores";
        private readonly string _collectionLote = "lotes_materia_prima";
        private readonly string _collectionVeiculo = "veiculos";
        private readonly string _collectionComponente = "veiculo_componentes";

        public DadosService(ILogger<DadosService> logger, FireBaseData firestoreDb)
        {
            _logger = logger;
            _firestoreDb = firestoreDb;
        }

        // ==========================================
        // MÉTODO EXTERNO: BUSCAR NA BRASILAPI (CNPJ)
        // ==========================================
        public async Task<Fornecedor> BuscarFornecedorPorCnpjAsync(string cnpj)
        {
            try
            {
                var cnpjLimpo = new string(cnpj.Where(char.IsDigit).ToArray());
                var url = $"https://brasilapi.com.br/api/cnpj/v1/{cnpjLimpo}";

                using var client = new HttpClient();
                // BLINDAGEM: Diz à Receita Federal que somos um software legítimo para não sermos bloqueados
                client.DefaultRequestHeaders.Add("User-Agent", "IvecoApp/1.0");

                var response = await client.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    var data = JsonSerializer.Deserialize<BrasilApiCnpjResponse>(json, options);

                    if (data != null)
                    {
                        // Atualizado com os novos nomes da Model
                        string nomeEmpresa = !string.IsNullOrWhiteSpace(data.NomeFantasia)
                            ? data.NomeFantasia
                            : data.RazaoSocial;

                        string moradaCompleta = $"{data.Logradouro}, {data.Numero} - {data.Bairro}, {data.Municipio} - {data.Uf}";

                        return new Fornecedor
                        {
                            Id = string.Empty,
                            Nome = nomeEmpresa,
                            Localizacao = moradaCompleta,
                            Cnpj = cnpjLimpo
                        };
                    }
                }

                // Se o código chegar aqui, imprime o erro real no terminal da API
                Console.WriteLine($"[FALHA BRASIL API]: HTTP {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERRO EXCEÇÃO BRASILAPI]: {ex.Message}");
                return null;
            }
        }
        // ==========================================
        // MÉTODOS FIREBASE: FORNECEDORES
        // ==========================================
        public async Task<List<Fornecedor>> ListarFornecedor()
        {
            CollectionReference collection = _firestoreDb.Db.Collection(_collectionFornecedor);
            QuerySnapshot snapshot = await collection.GetSnapshotAsync();
            List<Fornecedor> fornecedores = new List<Fornecedor>();

            foreach (DocumentSnapshot document in snapshot.Documents)
            {
                if (document.Exists)
                {
                    var fornecedor = document.ConvertTo<Fornecedor>();
                    fornecedor.Id = document.Id;
                    fornecedores.Add(fornecedor);
                }
            }
            return fornecedores;
        }

        public async Task<Fornecedor> CriarFornecedor(Fornecedor fornecedor)
        {
            int novoId = await GerarProximoId("contador_fornecedor");
            fornecedor.Id = novoId.ToString();

            DocumentReference docRef = _firestoreDb.Db.Collection(_collectionFornecedor).Document(fornecedor.Id);
            await docRef.SetAsync(fornecedor);
            return fornecedor;
        }

        public async Task ExcluirFornecedor(string id)
        {
            if (string.IsNullOrEmpty(id)) throw new ArgumentException("O ID do fornecedor não pode ser nulo ou vazio.");
            DocumentReference docRef = _firestoreDb.Db.Collection(_collectionFornecedor).Document(id);
            await docRef.DeleteAsync();
        }

        // ==========================================
        // MÉTODOS FIREBASE: LOTES MATÉRIA-PRIMA
        // ==========================================
        public async Task<List<LoteMateriaPrima>> ListarLoteMateriaPrima()
        {
            CollectionReference collection = _firestoreDb.Db.Collection(_collectionLote);
            QuerySnapshot snapshot = await collection.GetSnapshotAsync();
            List<LoteMateriaPrima> lotes = new List<LoteMateriaPrima>();
            foreach (DocumentSnapshot document in snapshot.Documents)
            {
                if (document.Exists)
                {
                    var lote = document.ConvertTo<LoteMateriaPrima>();
                    lote.Id = document.Id;
                    lotes.Add(lote);
                }
            }
            return lotes;
        }

        public async Task<LoteMateriaPrima> CriarLoteMateriaPrima(LoteMateriaPrima lote)
        {
            int novoId = await GerarProximoId("contador_lote");
            lote.Id = novoId.ToString();
            DocumentReference docRef = _firestoreDb.Db.Collection(_collectionLote).Document(lote.Id);
            await docRef.SetAsync(lote);
            return lote;
        }

        public async Task ExcluirLoteMateriaPrima(string id)
        {
            if (string.IsNullOrEmpty(id)) throw new ArgumentException("O ID não pode ser nulo ou vazio.");
            DocumentReference docRef = _firestoreDb.Db.Collection(_collectionLote).Document(id);
            await docRef.DeleteAsync();
        }

        // ==========================================
        // MÉTODOS FIREBASE: VEÍCULO COMPONENTES
        // ==========================================
        public async Task<List<VeiculoComponente>> ListarVeiculoComponente()
        {
            CollectionReference collection = _firestoreDb.Db.Collection(_collectionComponente);
            QuerySnapshot snapshot = await collection.GetSnapshotAsync();
            List<VeiculoComponente> componentes = new List<VeiculoComponente>();
            foreach (DocumentSnapshot document in snapshot.Documents)
            {
                if (document.Exists)
                {
                    var componente = document.ConvertTo<VeiculoComponente>();
                    componente.Id = document.Id;
                    componentes.Add(componente);
                }
            }
            return componentes;
        }

        public async Task<VeiculoComponente> CriarVeiculoComponente(VeiculoComponente componente)
        {
            int novoId = await GerarProximoId("contador_componente");
            componente.Id = novoId.ToString();
            DocumentReference docRef = _firestoreDb.Db.Collection(_collectionComponente).Document(componente.Id);
            await docRef.SetAsync(componente);
            return componente;
        }

        public async Task ExcluirVeiculoComponente(string id)
        {
            if (string.IsNullOrEmpty(id)) throw new ArgumentException("O ID não pode ser nulo ou vazio.");
            DocumentReference docRef = _firestoreDb.Db.Collection(_collectionComponente).Document(id);
            await docRef.DeleteAsync();
        }

        // ==========================================
        // MÉTODOS FIREBASE: VEÍCULOS
        // ==========================================
        public async Task<List<Veiculo>> ListarVeiculo()
        {
            CollectionReference collection = _firestoreDb.Db.Collection(_collectionVeiculo);
            QuerySnapshot snapshot = await collection.GetSnapshotAsync();
            List<Veiculo> veiculos = new List<Veiculo>();
            foreach (DocumentSnapshot document in snapshot.Documents)
            {
                if (document.Exists)
                {
                    var veiculo = document.ConvertTo<Veiculo>();
                    veiculo.Vin = document.Id;
                    veiculos.Add(veiculo);
                }
            }
            return veiculos;
        }

        public async Task<Veiculo> CriarVeiculo(Veiculo veiculo)
        {
            if (string.IsNullOrEmpty(veiculo.Vin)) throw new ArgumentException("O veículo deve possuir um VIN válido.");
            DocumentReference docRef = _firestoreDb.Db.Collection(_collectionVeiculo).Document(veiculo.Vin);
            await docRef.SetAsync(veiculo);
            return veiculo;
        }

        public async Task<Veiculo> ObterVeiculoPorVin(string vin)
        {
            var veiculos = await ListarVeiculo();
            return veiculos.FirstOrDefault(v => v.Vin.Equals(vin, StringComparison.OrdinalIgnoreCase));
        }

        public async Task ExcluirVeiculo(string vin)
        {
            if (string.IsNullOrEmpty(vin)) throw new ArgumentException("O VIN não pode ser nulo ou vazio.");
            DocumentReference docRef = _firestoreDb.Db.Collection(_collectionVeiculo).Document(vin);
            await docRef.DeleteAsync();
        }

        // ==========================================
        // MÉTODO AUXILIAR: GERADOR DE IDS NO FIREBASE
        // ==========================================
        private async Task<int> GerarProximoId(string nomeContador)
        {
            DocumentReference contadorId = _firestoreDb.Db.Collection("contadores").Document(nomeContador);
            return await _firestoreDb.Db.RunTransactionAsync(async transaction =>
            {
                DocumentSnapshot snapshot = await transaction.GetSnapshotAsync(contadorId);
                int idAtual = 0;
                if (snapshot.Exists) snapshot.TryGetValue("ultimoId", out idAtual);

                int proximoId = idAtual + 1;
                Dictionary<string, object> atualizacaoContador = new Dictionary<string, object> { { "ultimoId", proximoId } };
                transaction.Set(contadorId, atualizacaoContador, SetOptions.MergeAll);
                return proximoId;
            });
        }
    }
}