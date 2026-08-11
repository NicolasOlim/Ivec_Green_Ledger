using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WpfIveco.Models;

namespace WpfIveco.Data
{
    /// <summary>
    /// Serviço que gerencia as operações de leitura/escrita no banco de dados local SQLite.
    /// Funciona como um cache persistente para dados obtidos da API.
    /// </summary>
    public class LocalDatabaseService
    {
        private readonly AppDbContext _context;

        /// <summary>
        /// Construtor – inicializa o contexto e garante que o banco de dados exista.
        /// </summary>
        public LocalDatabaseService()
        {
            _context = new AppDbContext();
            _context.Database.EnsureCreated(); // Cria o banco e as tabelas se não existirem
        }

        // ============================================================
        // MÉTODOS PARA VEÍCULOS
        // ============================================================

        /// <summary>
        /// Obtém a lista de todos os veículos armazenados localmente.
        /// </summary>
        /// <returns>Lista de VeiculoModel (mapeados a partir das entidades).</returns>
        public async Task<List<VeiculoModel>> GetVeiculosAsync()
        {
            var entities = await _context.Veiculos.ToListAsync();
            return entities.Select(e => new VeiculoModel
            {
                Vin = e.Vin,
                Modelo = e.Modelo,
                DataMontagem = e.DataMontagem
            }).ToList();
        }

        /// <summary>
        /// Salva (ou substitui) a lista de veículos no banco local.
        /// Remove todos os registros antigos e insere os novos.
        /// </summary>
        /// <param name="veiculos">Lista de veículos obtidos da API.</param>
        public async Task SalvarVeiculosAsync(List<VeiculoModel> veiculos)
        {
            // Limpa a tabela para evitar duplicatas
            _context.Veiculos.RemoveRange(_context.Veiculos);

            // Converte para entidades e adiciona
            var entities = veiculos.Select(v => new VeiculoEntity
            {
                Vin = v.Vin,
                Modelo = v.Modelo,
                DataMontagem = v.DataMontagem,
                DataSincronizacao = DateTime.Now // Marca a data atual
            });
            await _context.Veiculos.AddRangeAsync(entities);
            await _context.SaveChangesAsync();
        }

        // ============================================================
        // MÉTODOS PARA FORNECEDORES
        // ============================================================

        /// <summary>
        /// Obtém a lista de todos os fornecedores armazenados localmente.
        /// </summary>
        public async Task<List<FornecedorModel>> GetFornecedoresAsync()
        {
            var entities = await _context.Fornecedores.ToListAsync();
            return entities.Select(e => new FornecedorModel
            {
                Id = e.FornecedorId,
                Cnpj = e.Cnpj,
                Nome = e.Nome,
                Localizacao = e.Localizacao,
                CategoriaEsg = e.CategoriaEsg
            }).ToList();
        }

        /// <summary>
        /// Salva (ou substitui) a lista de fornecedores no banco local.
        /// </summary>
        public async Task SalvarFornecedoresAsync(List<FornecedorModel> fornecedores)
        {
            _context.Fornecedores.RemoveRange(_context.Fornecedores);
            var entities = fornecedores.Select(f => new FornecedorEntity
            {
                FornecedorId = f.Id,
                Cnpj = f.Cnpj,
                Nome = f.Nome,
                Localizacao = f.Localizacao,
                CategoriaEsg = f.CategoriaEsg,
                DataSincronizacao = DateTime.Now
            });
            await _context.Fornecedores.AddRangeAsync(entities);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Salva um único fornecedor no banco local (usado em operações offline).
        /// </summary>
        /// <param name="fornecedor">Objeto FornecedorModel a ser salvo.</param>
        /// <returns>True se salvou com sucesso; False em caso de erro.</returns>
        public async Task<bool> SalvarFornecedorOfflineAsync(FornecedorModel fornecedor)
        {
            try
            {
                var entity = new FornecedorEntity
                {
                    FornecedorId = fornecedor.Id ?? Guid.NewGuid().ToString(),
                    Cnpj = fornecedor.Cnpj,
                    Nome = fornecedor.Nome,
                    Localizacao = fornecedor.Localizacao,
                    CategoriaEsg = fornecedor.CategoriaEsg,
                    DataSincronizacao = DateTime.Now
                };
                await _context.Fornecedores.AddAsync(entity);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        // ============================================================
        // MÉTODOS PARA PEÇAS
        // ============================================================

        /// <summary>
        /// Obtém a lista de todas as peças armazenadas localmente.
        /// </summary>
        public async Task<List<PecaModel>> GetPecasAsync()
        {
            var entities = await _context.Pecas.ToListAsync();
            return entities.Select(e => new PecaModel
            {
                NomePeca = e.NomePeca,
                VinAssociado = e.VinAssociado,
                PesoKg = e.PesoKg,
                FornecedorId = e.FornecedorId
            }).ToList();
        }

        /// <summary>
        /// Salva (ou substitui) a lista de peças no banco local.
        /// </summary>
        public async Task SalvarPecasAsync(List<PecaModel> pecas)
        {
            _context.Pecas.RemoveRange(_context.Pecas);
            var entities = pecas.Select(p => new PecaEntity
            {
                NomePeca = p.NomePeca,
                VinAssociado = p.VinAssociado,
                PesoKg = p.PesoKg,
                FornecedorId = p.FornecedorId,
                DataSincronizacao = DateTime.Now
            });
            await _context.Pecas.AddRangeAsync(entities);
            await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Salva uma única peça no banco local (usado em operações offline).
        /// </summary>
        public async Task<bool> SalvarPecaOfflineAsync(PecaModel peca)
        {
            try
            {
                var entity = new PecaEntity
                {
                    NomePeca = peca.NomePeca,
                    VinAssociado = peca.VinAssociado,
                    PesoKg = peca.PesoKg,
                    FornecedorId = peca.FornecedorId,
                    DataSincronizacao = DateTime.Now
                };
                await _context.Pecas.AddAsync(entity);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        // ============================================================
        // MÉTODOS AUXILIARES
        // ============================================================

        /// <summary>
        /// Verifica se há algum dado armazenado localmente (qualquer tabela).
        /// Usado para decidir se deve mostrar dados ou exibir mensagem de "sem dados".
        /// </summary>
        public async Task<bool> TemDadosAsync()
        {
            return await _context.Veiculos.AnyAsync() ||
                   await _context.Fornecedores.AnyAsync() ||
                   await _context.Pecas.AnyAsync();
        }

        /// <summary>
        /// Obtém a contagem total de veículos no banco local.
        /// </summary>
        public async Task<int> GetTotalVeiculosAsync()
        {
            return await _context.Veiculos.CountAsync();
        }

        /// <summary>
        /// Obtém a contagem total de fornecedores no banco local.
        /// </summary>
        public async Task<int> GetTotalFornecedoresAsync()
        {
            return await _context.Fornecedores.CountAsync();
        }

        /// <summary>
        /// Obtém a contagem total de peças no banco local.
        /// </summary>
        public async Task<int> GetTotalPecasAsync()
        {
            return await _context.Pecas.CountAsync();
        }
    }
}