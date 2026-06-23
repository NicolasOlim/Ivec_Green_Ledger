namespace ApiIveco.DTOs
{
    
    public class GraficoEmissoesDto
    {
        
        public string[] Meses { get; set; } = System.Array.Empty<string>();

        
        public double[] ValoresFabrica { get; set; } = System.Array.Empty<double>();

       
        public double[] ValoresCadeia { get; set; } = System.Array.Empty<double>();
    }
}