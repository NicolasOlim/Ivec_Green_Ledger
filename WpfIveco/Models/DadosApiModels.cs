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
        public string Cnpj { get; set; }
        public string Nome { get; set; }
        public string Localizacao { get; set; }
    }

    public class PecaModel
    {
        public string NomePeca { get; set; }
        public string VinAssociado { get; set; }
        public double PesoKg { get; set; }
        public string FornecedorId { get; set; } // NOVO
    }

    /// Esta classe mapeia o JSON retornado pela API em /api/dados/componentes
    public class VeiculoComponenteApi
    {
        public string Id { get; set; }
        public string NomePeca { get; set; }
        public string Fk_Veiculo_Vin { get; set; }
        public string Fk_LoteMateriaPrima_Id { get; set; }
        public double PesoKg { get; set; }
    }
}