using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfIveco.Models
{
    public class FornecedorEntity
    {
        public int Id { get; set; }
        public string FornecedorId { get; set; } // Id original da API
        public string Cnpj { get; set; }
        public string Nome { get; set; }
        public string Localizacao { get; set; }
        public string CategoriaEsg { get; set; }
        public DateTime DataSincronizacao { get; set; }
    }
}
