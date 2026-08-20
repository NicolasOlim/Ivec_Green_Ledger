using ApiIveco.Controllers;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Iveco.Testes.Controllers
{
    public class SuporteControllerTestes
    {
        private readonly SuporteController _controller = new SuporteController();

        [Fact]
        public void VerLogsDoDia_DeveRetornarActionResult()
        {
            var resultado = _controller.VerLogsDoDia();
            Assert.NotNull(resultado);
            Assert.IsAssignableFrom<IActionResult>(resultado);
        }

        [Fact]
        public void Diagnostico_DeveRetornarOk_ComInformacoesDosCaminhos()
        {
            var resultado = _controller.Diagnostico();
            var ok = Assert.IsType<OkObjectResult>(resultado);
            Assert.NotNull(ok.Value);

            var type = ok.Value.GetType();
            Assert.NotNull(type.GetProperty("AppContextBaseDirectory"));
            Assert.NotNull(type.GetProperty("DirectoryGetCurrentDirectory"));
            Assert.NotNull(type.GetProperty("Candidato1_AppContext"));
        }
    }
}