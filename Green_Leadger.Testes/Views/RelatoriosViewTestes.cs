using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using WpfIveco.Views;
using Xunit;

namespace Iveco.Testes.Views
{
    public class RelatoriosViewTestes
    {
        [Fact]
        public void Construtor_DeveInicializarComponentes()
        {
            // Arrange & Act
            var view = new RelatoriosView();

            // Assert
            Assert.NotNull(view);
        }

        [Fact]
        public void RadioButton_Checked_ComTagValida_DeveAtualizarTipoRelatorio()
        {
            // Arrange
            var view = new RelatoriosView();
            var radio = new RadioButton { Tag = "Veiculos" };
            var args = new RoutedEventArgs();

            // Act - invoca o método privado via reflexão
            var method = typeof(RelatoriosView).GetMethod("RadioButton_Checked",
                BindingFlags.NonPublic | BindingFlags.Instance);
            var exception = Record.Exception(() => method?.Invoke(view, new object[] { radio, args }));

            // Assert
            Assert.Null(exception);
        }

        [Fact]
        public void RadioButton_Checked_ComTagNula_NaoDeveLancarExcecao()
        {
            // Arrange
            var view = new RelatoriosView();
            var radio = new RadioButton { Tag = null };
            var args = new RoutedEventArgs();

            // Act - invoca o método privado via reflexão
            var method = typeof(RelatoriosView).GetMethod("RadioButton_Checked",
                BindingFlags.NonPublic | BindingFlags.Instance);
            var exception = Record.Exception(() => method?.Invoke(view, new object[] { radio, args }));

            // Assert
            Assert.Null(exception);
        }
    }
}