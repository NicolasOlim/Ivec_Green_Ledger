using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Linq;

namespace ApiIveco.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SuporteController : ControllerBase
    {
        [HttpGet("logs")]
        public IActionResult VerLogsDoDia()
        {
            /// Mapeia a pasta "logs" que já existe no seu projeto
            var caminhoPasta = Path.Combine(Directory.GetCurrentDirectory(), "logs");

            if (!Directory.Exists(caminhoPasta))
                return NotFound("Nenhum log foi gerado ainda. A pasta está vazia.");

            /// Pega o arquivo de log mais recente da pasta
            var ultimoArquivoLog = Directory.GetFiles(caminhoPasta, "log-*.txt")
                                            .OrderByDescending(f => f)
                                            .FirstOrDefault();

            if (string.IsNullOrEmpty(ultimoArquivoLog))
                return NotFound("Nenhum arquivo de log foi encontrado.");

            /// Lê o arquivo de forma "segura" (FileShare.ReadWrite) 
            /// Isso evita que dê erro caso o Serilog esteja tentando escrever no arquivo ao mesmo tempo
            using (var stream = new FileStream(ultimoArquivoLog, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            using (var leitor = new StreamReader(stream))
            {
                var conteudoDosLogs = leitor.ReadToEnd();

                /// Retorna como texto puro para o navegador ler de forma amigável
                return Content(conteudoDosLogs, "text/plain; charset=utf-8");
            }
        }
    }
}