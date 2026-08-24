using WpfIveco.Views;
using Xunit;

namespace Iveco.Testes.Views
{
    public class PecasViewTestes
    {
        [Fact]
        public void Construtor_DeveInicializarComponentes()
        {
            // Arrange & Act
            var view = new PecasView();

            // Assert
            Assert.NotNull(view);
        }
    }
}