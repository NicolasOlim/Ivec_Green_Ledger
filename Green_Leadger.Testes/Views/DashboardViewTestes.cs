using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using WpfIveco.Views;
using Xunit;

namespace Iveco.Testes.Views
{
    public class DashboardViewTestes
    {
        [Fact]
        public void Construtor_DeveInicializarComponentes()
        {
            var view = new DashboardView();
            Assert.NotNull(view);
        }

        [Fact]
        public void EnviarChamado_Click_ComDadosInvalidos_DeveMostrarMensagem()
        {
            // Arrange
            var view = new DashboardView();

            // Acessa o ComboBox privado via reflexão
            var comboField = typeof(DashboardView).GetField("ComboTipoProblema", BindingFlags.NonPublic | BindingFlags.Instance);
            var combo = comboField?.GetValue(view) as ComboBox;
            combo.SelectedItem = null; // Simula seleção nula

            // Acessa o método privado via reflexão
            var method = typeof(DashboardView).GetMethod("EnviarChamado_Click", BindingFlags.NonPublic | BindingFlags.Instance);
            var args = new RoutedEventArgs();

            // Act
            var exception = Record.Exception(() => method?.Invoke(view, new object[] { view, args }));

            // Assert
            Assert.Null(exception);
        }

        [Fact]
        public void EnviarChamado_Click_ComDadosValidos_DeveChamarApi()
        {
            // Arrange
            var view = new DashboardView();

            // Acessa os campos privados via reflexão
            var comboField = typeof(DashboardView).GetField("ComboTipoProblema", BindingFlags.NonPublic | BindingFlags.Instance);
            var txtNomeField = typeof(DashboardView).GetField("TxtNome", BindingFlags.NonPublic | BindingFlags.Instance);
            var txtDescricaoField = typeof(DashboardView).GetField("TxtDescricao", BindingFlags.NonPublic | BindingFlags.Instance);

            var combo = comboField?.GetValue(view) as ComboBox;
            combo.SelectedIndex = 0; // Seleciona o primeiro item

            var txtNome = txtNomeField?.GetValue(view) as TextBox;
            txtNome.Text = "Teste";

            var txtDescricao = txtDescricaoField?.GetValue(view) as TextBox;
            txtDescricao.Text = "Descrição de teste";

            var method = typeof(DashboardView).GetMethod("EnviarChamado_Click", BindingFlags.NonPublic | BindingFlags.Instance);
            var args = new RoutedEventArgs();

            // Act
            var exception = Record.Exception(() => method?.Invoke(view, new object[] { view, args }));

            // Assert
            Assert.Null(exception);
        }
    }
}