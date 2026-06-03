using Google.Cloud.Firestore;
using System;

namespace ApiIveco.Models
{
    [FirestoreData]
    public class Usuario
    {
        public Usuario() { }


        [FirestoreProperty]
        public string Id { get; set; } = "0";

        [FirestoreProperty] 
        public string Nome { get; set; }
        [FirestoreProperty]
        public string Email { get; set; }
        [FirestoreProperty]
        public string Senha { get; set; }
        [FirestoreProperty]
        public string Acesso { get; set; }
        [FirestoreProperty]
        public DateTime DataCriacao { get; set; }
    }
}