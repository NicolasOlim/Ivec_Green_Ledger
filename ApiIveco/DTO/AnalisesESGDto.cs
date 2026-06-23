using System.Collections.Generic;

namespace ApiIveco.DTOs
{
    public class AnalisesESGDto
    {
        
        public List<EscopoEmissaoDto> DistribuicaoEmissoes { get; set; } = new();

        public List<FornecedorVerdeDto> TopFornecedoresVerdes { get; set; } = new();
    }
}