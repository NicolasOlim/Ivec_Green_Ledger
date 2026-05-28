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
        public string Cnpj { get; internal set; }
        public string Nome { get; internal set; }
        public string Localizacao { get; internal set; }
    }
    public class PecaModel
    {
        public string NomePeca { get; set; }
        public string VinAssociado { get; set; }
    }
}