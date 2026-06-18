using Google.Cloud.Firestore;

namespace ApiIveco.Models
{
    [FirestoreData]
    public class VeiculoComponente
    {
        [FirestoreProperty]
        public string Id { get; set; }
        [FirestoreProperty]
        public string fk_Veiculo_Vin { get; set; }
        [FirestoreProperty]
        public string fk_LoteMateriaPrima_Id { get; set; }
        [FirestoreProperty]
        public string NomePeca { get; set; }
        [FirestoreProperty]
        public double PesoKg { get; set; }
    }
}
