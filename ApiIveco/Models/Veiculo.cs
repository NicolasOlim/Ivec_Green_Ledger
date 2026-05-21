using Google.Cloud.Firestore;

namespace ApiIveco.Models
{
    [FirestoreData]
    public class Veiculo
    {
        [FirestoreProperty]
        public string Vin { get; set; }
        [FirestoreProperty]
        public string Modelo { get; set; }
        [FirestoreProperty]
        public DateTime? DataMontagem { get; set; }

    }
}
