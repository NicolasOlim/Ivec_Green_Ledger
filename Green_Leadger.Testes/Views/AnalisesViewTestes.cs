using WpfIveco.Views;
using Xunit;

namespace Iveco.Testes.Views
{
    public class AnalisesViewTestes
    {
        [Fact]
        public void Construtor_DeveInicializarComponentes()
        {
            // Arrange & Act
            var view = new AnalisesView();

            // Assert
            Assert.NotNull(view);
        }
    }
}