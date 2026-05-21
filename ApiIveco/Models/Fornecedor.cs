using Google.Cloud.Firestore;

namespace ApiIveco.Models
{
    [FirestoreData]
    public class Fornecedor
    {
        [FirestoreProperty]
        public string Id { get; set; }
        [FirestoreProperty]
        public string Nome { get; set; }
        [FirestoreProperty]
        public string Localizacao { get; set; }
        [FirestoreProperty]
        public string Cnpj { get; set; }
        
    }
}
