using Google.Cloud.Firestore;
using System.Text.Json;

namespace ApiIveco.Data
{
    public class FireBaseData
    {
        public FirestoreDb Db { get; private set; } = null!;

        public FireBaseData()
        {
            var nomeArquivo = Path.Combine("chave_Api", "firebase-key.json");

            // CORRECAO: Tenta múltiplos caminhos base para encontrar o arquivo de chave.
            // No IIS/runasp.net, AppContext.BaseDirectory e Directory.GetCurrentDirectory()
            // podem apontar para pastas diferentes dependendo de como o processo é iniciado.
            var candidatos = new[]
            {
                // 1. Caminho relativo ao diretório da aplicação (funciona localmente e no IIS em geral)
                Path.Combine(AppContext.BaseDirectory, nomeArquivo),

                // 2. Caminho relativo ao diretório de trabalho atual (funciona no runasp.net/IIS)
                Path.Combine(Directory.GetCurrentDirectory(), nomeArquivo),

                // 3. Caminho relativo ao diretório do executável
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, nomeArquivo),
            };

            string? fullPath = candidatos.FirstOrDefault(File.Exists);

            if (fullPath == null)
            {
                // Monta mensagem detalhada com todos os caminhos tentados para facilitar debug
                var tentativas = string.Join("\n  ", candidatos);
                throw new FileNotFoundException(
                    $"Arquivo firebase-key.json não encontrado. Caminhos tentados:\n  {tentativas}");
            }

            string jsonContent = File.ReadAllText(fullPath);
            using JsonDocument doc = JsonDocument.Parse(jsonContent);

            if (!doc.RootElement.TryGetProperty("project_id", out var projectIdElement))
                throw new Exception("O arquivo firebase-key.json não contém a propriedade 'project_id'.");

            string? projectId = projectIdElement.GetString();

            if (string.IsNullOrWhiteSpace(projectId))
                throw new Exception("A propriedade 'project_id' está vazia no firebase-key.json.");

            var builder = new FirestoreDbBuilder
            {
                ProjectId = projectId,
                CredentialsPath = fullPath
            };

            Db = builder.Build();
        }

        public static implicit operator FireBaseData(FirestoreDb v)
        {
            throw new NotImplementedException();
        }
    }
}