namespace WpfIveco.DTO
{
    /// <summary>
    /// DTO para comunicação com a API de componentes (peças)
    /// </summary>
    public class VeiculoComponenteApiDto
    {
        public string Id { get; set; }
        public string NomePeca { get; set; }
        public string Fk_Veiculo_Vin { get; set; }
        public string Fk_LoteMateriaPrima_Id { get; set; }
        public string Fk_Fornecedor_Id { get; set; }
        public double PesoKg { get; set; }
    }
}