using Microsoft.AspNetCore.Mvc;

namespace ApiIveco.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SuporteController : ControllerBase
    {
        [HttpGet("logs")]
        public IActionResult VerLogsDoDia()
        {
            var caminhoPasta = Path.Combine(Directory.GetCurrentDirectory(), "logs");

            if (!Directory.Exists(caminhoPasta))
                return NotFound("Nenhum log foi gerado ainda. A pasta está vazia.");

            var ultimoArquivoLog = Directory.GetFiles(caminhoPasta, "log-*.txt")
                                            .OrderByDescending(f => f)
                                            .FirstOrDefault();

            if (string.IsNullOrEmpty(ultimoArquivoLog))
                return NotFound("Nenhum arquivo de log foi encontrado.");

            using var stream = new FileStream(ultimoArquivoLog, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            using var leitor = new StreamReader(stream);
            return Content(leitor.ReadToEnd(), "text/plain; charset=utf-8");
        }

        // Endpoint de diagnóstico — mostra os caminhos reais do servidor
        // Acesse: GET /api/Suporte/diagnostico
        [HttpGet("diagnostico")]
        public IActionResult Diagnostico()
        {
            var nomeArquivo = Path.Combine("chave_Api", "firebase-key.json");

            // CORRECAO: usar System.IO.File explicitamente para evitar conflito
            // com o método File() herdado de ControllerBase
            var caminho1 = Path.Combine(AppContext.BaseDirectory, nomeArquivo);
            var caminho2 = Path.Combine(Directory.GetCurrentDirectory(), nomeArquivo);
            var caminho3 = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, nomeArquivo);

            var caminhos = new
            {
                AppContextBaseDirectory = AppContext.BaseDirectory,
                DirectoryGetCurrentDirectory = Directory.GetCurrentDirectory(),
                AppDomainBaseDirectory = AppDomain.CurrentDomain.BaseDirectory,

                Candidato1_AppContext = new
                {
                    Caminho = caminho1,
                    Existe = System.IO.File.Exists(caminho1)
                },
                Candidato2_CurrentDir = new
                {
                    Caminho = caminho2,
                    Existe = System.IO.File.Exists(caminho2)
                },
                Candidato3_AppDomain = new
                {
                    Caminho = caminho3,
                    Existe = System.IO.File.Exists(caminho3)
                },

                ArquivosNoDirAtual = Directory.Exists(Directory.GetCurrentDirectory())
                    ? Directory.GetFiles(Directory.GetCurrentDirectory()).Select(Path.GetFileName).Take(20)
                    : Enumerable.Empty<string?>(),

                PastasNoDirAtual = Directory.Exists(Directory.GetCurrentDirectory())
                    ? Directory.GetDirectories(Directory.GetCurrentDirectory()).Select(Path.GetFileName).Take(20)
                    : Enumerable.Empty<string?>(),
            };

            return Ok(caminhos);
        }
    }
}