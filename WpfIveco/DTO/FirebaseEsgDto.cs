using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using WpfIveco.Models;

namespace WpfIveco.DTO
{
    /// <summary>
    /// Classe que mapeia com precisão as chaves do JSON guardado no Firebase.
    /// </summary>
    public class FirebaseEsgDto
    {
        [JsonPropertyName("totalEmissoes")]
        public int TotalEmissoes { get; set; }

        [JsonPropertyName("fornecedoresVerdes")]
        public int FornecedoresVerdes { get; set; }

        [JsonPropertyName("pecasReaproveitadas")]
        public int PecasReaproveitadas { get; set; }

        [JsonPropertyName("economiaGerada")]
        public string EconomiaGerada { get; set; }

        [JsonPropertyName("topFornecedores")]
        public List<FornecedorSustentavel> TopFornecedores { get; set; }
    }
}
