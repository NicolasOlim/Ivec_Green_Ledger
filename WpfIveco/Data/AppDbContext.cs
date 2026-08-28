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
        public DbSet<PecaEntity> Pecas { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Pasta da aplicação (onde o .exe está a correr)
            string appFolder = AppDomain.CurrentDomain.BaseDirectory;
            string dbFolder = Path.Combine(appFolder, "Database");
            Directory.CreateDirectory(dbFolder);

            string dbPath = Path.Combine(dbFolder, "local.db");
            optionsBuilder.UseSqlite($"Data Source={dbPath}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<VeiculoEntity>()
                .HasIndex(v => v.Vin)
                .IsUnique();
        }
    }
}