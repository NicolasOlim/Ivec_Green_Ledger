using WpfIveco.Views;
using Xunit;

namespace Iveco.Testes.Views
{
    public class AjustesViewTestes
    {
        [Fact]
        public void Construtor_DeveInicializarComponentes()
        {
            // Arrange & Act
            var view = new AjustesView();

            // Assert
            Assert.NotNull(view);
        }
    }
}