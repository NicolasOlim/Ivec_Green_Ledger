using ApiIveco.Data;
using ApiIveco.Models;
using Google.Cloud.Firestore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace ApiIveco.Service
{
    public class DadosService
    {
        private readonly ILogger<DadosService> _logger;
        private readonly FireBaseData _firestoreDb;
        private readonly HttpClient _httpClient;

        // Definindo os nomes das coleções no Firebase
        private readonly string _collectionFornecedor = "fornecedores";
        private readonly string _collectionLote = "lotes_materia_prima";
        private readonly string _collectionVeiculo = "veiculos";
        private readonly string _collectionComponente = "veiculo_componentes";

        public DadosService(ILogger<DadosService> logger, FireBaseData firestoreDb, HttpClient httpClient)
        {
            _logger = logger;
            _firestoreDb = firestoreDb;
            _httpClient = httpClient;
        }

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
                    // Agora o Id da model recebe a string diretamente do documento do Firebase
                    fornecedor.Id = document.Id;
                    fornecedores.Add(fornecedor);
                }
            }
            return fornecedores;
        }

        public async Task<Fornecedor> CriarFornecedor(Fornecedor fornecedor)
        {
            int novoId = await GerarProximoId("contador_fornecedor");
            // Converte o número gerado para string e atribui ao Id da model
            fornecedor.Id = novoId.ToString();

            DocumentReference docRef = _firestoreDb.Db.Collection(_collectionFornecedor).Document(fornecedor.Id);
            await docRef.SetAsync(fornecedor);

            return fornecedor;
        }

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
            if (string.IsNullOrEmpty(veiculo.Vin))
            {
                throw new ArgumentException("O veículo deve possuir um VIN válido.");
            }

            DocumentReference docRef = _firestoreDb.Db.Collection(_collectionVeiculo).Document(veiculo.Vin);
            await docRef.SetAsync(veiculo);

            return veiculo;
        }

        public async Task<Veiculo> ObterVeiculoPorVin(string vin)
        {
            // Pega a lista completa de veículos usando o método que você já tem
            var veiculos = await ListarVeiculo();

            // Procura na lista o veículo que tem o VIN exato (ignorando letras maiúsculas/minúsculas)
            // Se não encontrar, ele retorna "null" automaticamente
            var veiculoEncontrado = veiculos.FirstOrDefault(v => v.Vin.Equals(vin, StringComparison.OrdinalIgnoreCase));

            return veiculoEncontrado;
        }

        // O contador ainda funciona com "int" para poder somar + 1 matematicamente no banco.
        // A conversão para string ocorre apenas na hora de aplicar à model e criar o documento acima.
        private async Task<int> GerarProximoId(string nomeContador)
        {
            DocumentReference contadorId = _firestoreDb.Db.Collection("contadores").Document(nomeContador);

            return await _firestoreDb.Db.RunTransactionAsync(async transaction =>
            {
                DocumentSnapshot snapshot = await transaction.GetSnapshotAsync(contadorId);
                int idAtual = 0;

                if (snapshot.Exists)
                {
                    snapshot.TryGetValue("ultimoId", out idAtual);
                }

                int proximoId = idAtual + 1;
                Dictionary<string, object> atualizacaoContador = new Dictionary<string, object>
                {
                    { "ultimoId", proximoId }
                };

                transaction.Set(contadorId, atualizacaoContador, SetOptions.MergeAll);
                return proximoId;
            });
        }
        public async Task ExcluirFornecedor(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                _logger.LogWarning("Tentativa de excluir fornecedor com ID nulo ou vazio.");
                throw new ArgumentException("O ID do fornecedor não pode ser nulo ou vazio.");
            }

            DocumentReference docRef = _firestoreDb.Db.Collection(_collectionFornecedor).Document(id);
            await docRef.DeleteAsync();
            _logger.LogInformation("Fornecedor com ID {Id} excluído com sucesso.", id);
        }

        public async Task ExcluirLoteMateriaPrima(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                _logger.LogWarning("Tentativa de excluir lote com ID nulo ou vazio.");
                throw new ArgumentException("O ID do lote não pode ser nulo ou vazio.");
            }

            DocumentReference docRef = _firestoreDb.Db.Collection(_collectionLote).Document(id);
            await docRef.DeleteAsync();
            _logger.LogInformation("Lote com ID {Id} excluído com sucesso.", id);
        }

        public async Task ExcluirVeiculoComponente(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                _logger.LogWarning("Tentativa de excluir componente com ID nulo ou vazio.");
                throw new ArgumentException("O ID do componente não pode ser nulo ou vazio.");
            }

            DocumentReference docRef = _firestoreDb.Db.Collection(_collectionComponente).Document(id);
            await docRef.DeleteAsync();
            _logger.LogInformation("Componente com ID {Id} excluído com sucesso.", id);
        }

        public async Task ExcluirVeiculo(string vin)
        {
            if (string.IsNullOrEmpty(vin))
            {
                _logger.LogWarning("Tentativa de excluir veículo com VIN nulo ou vazio.");
                throw new ArgumentException("O VIN do veículo não pode ser nulo ou vazio.");
            }

            DocumentReference docRef = _firestoreDb.Db.Collection(_collectionVeiculo).Document(vin);
            await docRef.DeleteAsync();
            _logger.LogInformation("Veículo com VIN {Vin} excluído com sucesso.", vin);
        }

    }
}