using ApiIveco.Services;
using System.Threading.Tasks;
using Xunit;

namespace Iveco.Testes.Services
{
    public class EmailValidationServiceTestes
    {
        [Theory]
        [InlineData("funcionario@iveco.com")]
        [InlineData("  ADMIN@IVECO.COM  ")] /// Testa a remoção de espaços (Trim) e case-insensitive
        [InlineData("teste.ponto@iveco.com")]
        public async Task ValidateEmailAsync_EmailComDominioIveco_DeveRetornarTrue(string email)
        {
            /// Arrange
            var service = new EmailValidationService();

            /// Act
            var resultado = await service.ValidateEmailAsync(email);

            /// Assert
            Assert.True(resultado.isValid);
            Assert.Equal("E-mail válido (domínio IVECO).", resultado.message);
        }

        [Theory]
        [InlineData("usuario@gmail.com")]
        [InlineData("funcionario@iveco.com.br")]
        [InlineData("iveco.com@yahoo.com")]
        public async Task ValidateEmailAsync_EmailSemDominioIveco_DeveRetornarFalse(string email)
        {
            /// Arrange
            var service = new EmailValidationService();

            /// Act
            var resultado = await service.ValidateEmailAsync(email);

            /// Assert
            Assert.False(resultado.isValid);
            Assert.Equal("Apenas e-mails com domínio @iveco.com são permitidos.", resultado.message);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData(null)]
        public async Task ValidateEmailAsync_EmailNuloOuVazio_DeveRetornarFalse(string email)
        {
            /// Arrange
            var service = new EmailValidationService();

            /// Act
            var resultado = await service.ValidateEmailAsync(email);

            /// Assert
            Assert.False(resultado.isValid);
            Assert.Equal("O e-mail é obrigatório.", resultado.message);
        }
    }
}