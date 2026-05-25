using Microsoft.EntityFrameworkCore;
using SimuladorIveco.Models; // Importa as classes do ficheiro Model.cs

namespace SimuladorIveco.Data
{
    public class AppDbContext : DbContext
    {
        // As tabelas que vão existir no banco do simulador
        public DbSet<Model.Fornecedor> Fornecedores { get; set; }
        public DbSet<Model.LoteMateriaPrima> Lotes { get; set; }
        public DbSet<Model.Veiculo> Veiculos { get; set; }
        public DbSet<Model.VeiculoComponente> Componentes { get; set; }

        // Configura o C# para criar um arquivo local chamado "dados_gerados.db"
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            string pastaDoProjeto = System.IO.Path.GetFullPath(System.IO.Path.Combine(System.AppContext.BaseDirectory, @"..\..\..\"));
            string caminhoCompleto = System.IO.Path.Combine(pastaDoProjeto, "dados_gerados.db");

            optionsBuilder.UseSqlite($"Data Source={caminhoCompleto}");
        }

        // Ensina o banco que o VIN é a chave principal da tabela Veículos
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Model.Veiculo>().HasKey(v => v.Vin);
        }
    }
}