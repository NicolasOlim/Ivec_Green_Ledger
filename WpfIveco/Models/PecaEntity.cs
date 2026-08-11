using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfIveco.Models
{
    public class PecaEntity
    {
        public int Id { get; set; }
        public string NomePeca { get; set; }
        public string VinAssociado { get; set; }
        public double PesoKg { get; set; }
        public string FornecedorId { get; set; }
        public DateTime DataSincronizacao { get; set; }
    }
}
