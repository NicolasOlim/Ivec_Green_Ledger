using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfIveco.Models
{
    public class AvaliacaoFornecedor
    {
        public string Fornecedor { get; set; }
        public string Material { get; set; }
        public double PegadaCarbono { get; set; }
        public DateTime DataAvaliacao { get; set; }
        public string Status { get; set; }
    }
}
