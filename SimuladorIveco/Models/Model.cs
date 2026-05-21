using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SimuladorIveco.Models
{
    internal class Model
    {

        public class Fornecedor
        {
            public int Id { get; set; }
            public string Nome { get; set; }
            public string Localizacao { get; set; }
            public string Cnpj { get; set; }
        }

        public class LoteMateriaPrima
        {
            public int Id { get; set; }
            public string TipoMaterial { get; set; }
            public DateTime? DataProducao { get; set; }
            public double QuantidadeKg { get; set; }
            public double PegadaCarbonoPorKg { get; set; }
            public int fk_Fornecedor_Id { get; set; }
        }

        public class Veiculo
        {
            public string Vin { get; set; }
            public string Modelo { get; set; }
            public DateTime? DataMontagem { get; set; }
        }

        public class VeiculoComponente
        {
            public int Id { get; set; }
            public string fk_Veiculo_Vin { get; set; }
            public int fk_LoteMateriaPrima_Id { get; set; }
            public string NomePeca { get; set; }
        }
    }
}
