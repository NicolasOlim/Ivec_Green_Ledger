using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ApiIveco.Models
{
    public class MercadoLivreResponse
    {
        [JsonPropertyName("results")]
        public List<MercadoLivreItem> Results { get; set; }
    }

    public class MercadoLivreItem
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("title")]
        public string Title { get; set; } /// Aqui virá o nome real da peça (Ex: "Filtro De Ar Caminhão Iveco")
    }
}