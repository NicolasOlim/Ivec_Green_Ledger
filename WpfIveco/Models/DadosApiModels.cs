using System;

namespace WpfIveco.Models
{
    public class VeiculoModel
    {
        public string Vin { get; set; }
        public string Modelo { get; set; }
        public DateTime? DataMontagem { get; set; }
    }

    public class FornecedorModel
    {
        public string Id { get; set; }
    }
}