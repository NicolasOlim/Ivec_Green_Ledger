using System.Collections.Generic;

namespace ApiIveco.Models
{
    // Classes auxiliares apenas para ler o JSON que vem da API do Google
    public class GooglePlacesResponse
    {
        public List<PlaceResult> results { get; set; }
        public string status { get; set; }
    }

    public class PlaceResult
    {
        public string name { get; set; }
        public string formatted_address { get; set; }
    }
}