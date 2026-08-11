using System;

namespace WpfIveco.Data
{
    public class VeiculoEntity
    {
        public int Id { get; set; } // chave primária auto increment
        public string Vin { get; set; }
        public string Modelo { get; set; }
        public DateTime? DataMontagem { get; set; }
        public DateTime DataSincronizacao { get; set; } // para controle
    }
}