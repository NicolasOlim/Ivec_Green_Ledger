using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfIveco.Models
{
    /// <summary>
    /// Classe auxiliar para armazenar dados em cache.
    /// </summary>
    internal class DashboardCacheData
    {
        public int ConsultasHoje { get; set; }
        public double FalhasIntegracao { get; set; }
        public int TempoRespostaMs { get; set; }
        public int UsoServidor { get; set; }
        public string VariacaoConsultas { get; set; }
        public string PegadaMediaFormatada { get; set; }
    }
}
