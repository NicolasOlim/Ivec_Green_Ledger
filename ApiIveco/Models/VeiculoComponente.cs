using System.ComponentModel.DataAnnotations;

namespace ApiIveco.Models
{
    public class VeiculoComponente
    {
        public string Id { get; set; }

        [Required]
        public string NomePeca { get; set; }

        [Required]
        public string fk_Veiculo_Vin { get; set; }

        // Tornar opcional – remover [Required] ou usar nullable
        public string fk_LoteMateriaPrima_Id { get; set; } // agora nullable

        public string fk_Fornecedor_Id { get; set; }

        [Range(0.01, double.MaxValue)]
        public double PesoKg { get; set; }
    }
}