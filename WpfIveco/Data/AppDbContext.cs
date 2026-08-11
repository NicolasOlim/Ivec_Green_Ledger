using Microsoft.EntityFrameworkCore;
using System;
using System.IO;
using WpfIveco.Models;

namespace WpfIveco.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<VeiculoEntity> Veiculos { get; set; }
        public DbSet<FornecedorEntity> Fornecedores { get; set; }
        public DbSet<PecaEntity> Pecas { get; set; }  // Corrigido: sem acento

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Caminho fixo para a raiz do projeto
            string projectDirectory = @"C:\Users\Nicolas\source\repos\Ivec_Green_Ledger\WpfIveco";

            string dbFolder = Path.Combine(projectDirectory, "Database");
            Directory.CreateDirectory(dbFolder);

            string dbPath = Path.Combine(dbFolder, "local.db");
            optionsBuilder.UseSqlite($"Data Source={dbPath}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Índice único para VIN
            modelBuilder.Entity<VeiculoEntity>()
                .HasIndex(v => v.Vin)
                .IsUnique();
        }
    }
}