using Google.Cloud.Firestore;

namespace ApiIveco.Models
{
    [FirestoreData]
    public class LoteMateriaPrima
    {
        [FirestoreProperty]
        public string Id { get; set; }
        [FirestoreProperty]
        public string TipoMaterial { get; set; }
        [FirestoreProperty]
        public DateTime? DataProducao { get; set; }
        [FirestoreProperty]
        public double QuantidadeKg { get; set; }
        [FirestoreProperty]
        public Double PegadaCarbonoPorKg { get; set; }
        [FirestoreProperty]
        public string fk_Fornecedor_Id { get; set; }

    }

}
