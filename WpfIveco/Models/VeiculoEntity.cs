using System;

namespace WpfIveco.Models
{
    public class VeiculoEntity
    {
        public int Id { get; set; } // Auto-increment no SQLite
        public string Vin { get; set; }
        public string Modelo { get; set; }
        public DateTime? DataMontagem { get; set; }
        public DateTime DataSincronizacao { get; set; }
        public bool Sincronizado { get; set; } // Indica se já foi enviado à API
    }
}