using ApiIveco.Controllers;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Iveco.Testes.Controllers
{
    public class SuporteControllerTestes
    {
        private readonly SuporteController _controller = new SuporteController();

        // ====================== TESTE DO DIAGNÓSTICO ======================

        [Fact]
        public void Diagnostico_DeveRetornarOk_ComInformacoesDosCaminhos()
        {
            // Act
            var resultado = _controller.Diagnostico();

            // Assert
            var ok = Assert.IsType<OkObjectResult>(resultado);
            Assert.NotNull(ok.Value);

            var type = ok.Value.GetType();
            Assert.NotNull(type.GetProperty("AppContextBaseDirectory"));
            Assert.NotNull(type.GetProperty("DirectoryGetCurrentDirectory"));
            Assert.NotNull(type.GetProperty("AppDomainBaseDirectory"));
            Assert.NotNull(type.GetProperty("Candidato1_AppContext"));
            Assert.NotNull(type.GetProperty("Candidato2_CurrentDir"));
            Assert.NotNull(type.GetProperty("Candidato3_AppDomain"));
        }

        // ====================== TESTE DOS LOGS ======================
        // Este método depende do sistema de arquivos.
        // Para testes unitários, seria necessário injetar uma abstração (IFileSystem).
        // Aqui testamos apenas que o retorno é um IActionResult.
        [Fact]
        public void VerLogsDoDia_DeveRetornarActionResult()
        {
            // Act
            var resultado = _controller.VerLogsDoDia();

            // Assert
            Assert.NotNull(resultado);
            Assert.IsAssignableFrom<IActionResult>(resultado);
        }
    }
}