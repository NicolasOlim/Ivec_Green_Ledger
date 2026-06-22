using System.Collections.Generic;

namespace WpfIveco.DTO
{
    public class AnalisesESGDto
    {
        public List<EscopoEmissaoDto> DistribuicaoEmissoes { get; set; }
        public List<FornecedorVerdeDto> TopFornecedoresVerdes { get; set; }
    }

    public class EscopoEmissaoDto
    {
        public string Escopo { get; set; }
        public double Porcentagem { get; set; }
    }

    public class FornecedorVerdeDto
    {
        public string Id { get; set; }
        public string Nome { get; set; }
        public string Localizacao { get; set; }
        public int TotalPecas { get; set; }
        public double PegadaMedia { get; set; }
        public double ScoreVerde { get; set; }
        public string Certificado { get; set; }
    }
}