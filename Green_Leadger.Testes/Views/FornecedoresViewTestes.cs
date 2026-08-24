using System;
using System.Reflection;
using System.Windows.Controls;
using System.Windows.Input;
using WpfIveco.Views;
using Xunit;

namespace Iveco.Testes.Views
{
    public class FornecedoresViewTestes
    {
        [Fact]
        public void Construtor_DeveInicializarComponentes()
        {
            var view = new FornecedoresView();
            Assert.NotNull(view);
        }

        [Fact]
        public void NumberValidationTextBox_ComNumeros_DevePermitir()
        {
            // Arrange
            var view = new FornecedoresView();
            var textBox = new TextBox();
            var args = new TextCompositionEventArgs(
                InputManager.Current.PrimaryKeyboardDevice,
                new TextComposition(InputManager.Current, textBox, "123")
            );
            args.RoutedEvent = TextCompositionManager.TextInputEvent;

            // Act - invoca o método privado via reflexão
            var method = typeof(FornecedoresView).GetMethod("NumberValidationTextBox",
                BindingFlags.NonPublic | BindingFlags.Instance);
            method?.Invoke(view, new object[] { textBox, args });

            // Assert
            Assert.False(args.Handled, "Números devem ser permitidos.");
        }

        [Fact]
        public void NumberValidationTextBox_ComLetras_DeveBloquear()
        {
            // Arrange
            var view = new FornecedoresView();
            var textBox = new TextBox();
            var args = new TextCompositionEventArgs(
                InputManager.Current.PrimaryKeyboardDevice,
                new TextComposition(InputManager.Current, textBox, "abc")
            );
            args.RoutedEvent = TextCompositionManager.TextInputEvent;

            // Act
            var method = typeof(FornecedoresView).GetMethod("NumberValidationTextBox",
                BindingFlags.NonPublic | BindingFlags.Instance);
            method?.Invoke(view, new object[] { textBox, args });

            // Assert
            Assert.True(args.Handled, "Letras devem ser bloqueadas.");
        }

        [Fact]
        public void CnpjTextBox_TextChanged_DeveFormatarCNPJ()
        {
            // Arrange
            var view = new FornecedoresView();
            var textBox = new TextBox();
            textBox.Text = "12345678000199";

            // Act
            var method = typeof(FornecedoresView).GetMethod("CnpjTextBox_TextChanged",
                BindingFlags.NonPublic | BindingFlags.Instance);
            var eventArgs = new TextChangedEventArgs(TextBox.TextChangedEvent, new UndoAction());
            method?.Invoke(view, new object[] { textBox, eventArgs });

            // Assert
            Assert.Equal("12.345.678/0001-99", textBox.Text);
        }

        [Fact]
        public void CnpjTextBox_TextChanged_ComTextoParcial_DeveFormatarParcialmente()
        {
            // Arrange
            var view = new FornecedoresView();
            var textBox = new TextBox();
            textBox.Text = "123456";

            // Act
            var method = typeof(FornecedoresView).GetMethod("CnpjTextBox_TextChanged",
                BindingFlags.NonPublic | BindingFlags.Instance);
            var eventArgs = new TextChangedEventArgs(TextBox.TextChangedEvent, new UndoAction());
            method?.Invoke(view, new object[] { textBox, eventArgs });

            // Assert
            Assert.Equal("12.345.6", textBox.Text);
        }

        [Fact]
        public void CnpjTextBox_TextChanged_ComTextoVazio_DeveManterVazio()
        {
            // Arrange
            var view = new FornecedoresView();
            var textBox = new TextBox();
            textBox.Text = "";

            // Act
            var method = typeof(FornecedoresView).GetMethod("CnpjTextBox_TextChanged",
                BindingFlags.NonPublic | BindingFlags.Instance);
            var eventArgs = new TextChangedEventArgs(TextBox.TextChangedEvent, new UndoAction());
            method?.Invoke(view, new object[] { textBox, eventArgs });

            // Assert
            Assert.Equal("", textBox.Text);
        }
    }
}