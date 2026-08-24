using WpfIveco.Views;
using Xunit;

namespace Iveco.Testes.Views
{
    public class RastreabilidadeViewTestes
    {
        [Fact]
        public void Construtor_DeveInicializarComponentes()
        {
            // Arrange & Act
            var view = new RastreabilidadeView();

            // Assert
            Assert.NotNull(view);
        }
    }
}