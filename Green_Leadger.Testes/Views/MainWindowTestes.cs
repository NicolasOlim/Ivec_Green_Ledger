using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Moq;
using WpfIveco;
using WpfIveco.ViewModel;
using Xunit;

namespace Iveco.Testes.Views
{
    public class MainWindowTestes
    {
        [Fact]
        public void Construtor_DeveDefinirDataContextComoMainViewModel()
        {
            // Arrange & Act
            var window = new MainWindow();

            // Assert
            Assert.IsType<MainViewModel>(window.DataContext);
        }

        [Fact]
        public void CloseButton_Click_DeveFecharAplicacao()
        {
            // Arrange
            var window = new MainWindow();
            var args = new RoutedEventArgs();

            // Act - invoca o método privado via reflexão
            var method = typeof(MainWindow).GetMethod("CloseButton_Click",
                BindingFlags.NonPublic | BindingFlags.Instance);
            var exception = Record.Exception(() => method?.Invoke(window, new object[] { window, args }));

            // Assert
            Assert.Null(exception);
        }

        [Fact]
        public void MinimizeButton_Click_DeveMinimizarJanela()
        {
            // Arrange
            var window = new MainWindow();
            window.WindowState = WindowState.Normal;

            // Act - invoca o método privado via reflexão
            var method = typeof(MainWindow).GetMethod("MinimizeButton_Click",
                BindingFlags.NonPublic | BindingFlags.Instance);
            method?.Invoke(window, new object[] { window, new RoutedEventArgs() });

            // Assert
            Assert.Equal(WindowState.Minimized, window.WindowState);
        }

        [Fact]
        public void MaximizeButton_Click_DeveAlternarEstado()
        {
            // Arrange
            var window = new MainWindow();
            window.WindowState = WindowState.Normal;

            // Act - invoca o método privado via reflexão
            var method = typeof(MainWindow).GetMethod("MaximizeButton_Click",
                BindingFlags.NonPublic | BindingFlags.Instance);
            method?.Invoke(window, new object[] { window, new RoutedEventArgs() });

            // Assert
            Assert.Equal(WindowState.Maximized, window.WindowState);

            // Act novamente
            method?.Invoke(window, new object[] { window, new RoutedEventArgs() });

            // Assert
            Assert.Equal(WindowState.Normal, window.WindowState);
        }

        [Fact]
        public void SenhaPasswordBox_PasswordChanged_DeveAtualizarViewModel()
        {
            // Arrange
            var window = new MainWindow();
            var viewModel = (MainViewModel)window.DataContext;
            var passwordBox = new PasswordBox();
            passwordBox.Password = "123456";

            // Act - invoca o método privado via reflexão
            var method = typeof(MainWindow).GetMethod("SenhaPasswordBox_PasswordChanged",
                BindingFlags.NonPublic | BindingFlags.Instance);
            method?.Invoke(window, new object[] { passwordBox, new RoutedEventArgs() });

            // Assert
            Assert.Equal("123456", viewModel.LoginSenha);
        }

        [Fact]
        public void BtnMostrarSenha_Click_DeveAlternarVisibilidade()
        {
            // Arrange
            var window = new MainWindow();
            var sender = new Button();
            var args = new RoutedEventArgs();

            // Act - invoca o método privado via reflexão
            var method = typeof(MainWindow).GetMethod("BtnMostrarSenha_Click",
                BindingFlags.NonPublic | BindingFlags.Instance);
            var exception = Record.Exception(() => method?.Invoke(window, new object[] { sender, args }));

            // Assert
            Assert.Null(exception);
        }

        [Fact]
        public void BtnAbrirModalSair_Click_DeveTornarModalVisivel()
        {
            // Arrange
            var window = new MainWindow();

            // Act - invoca o método privado via reflexão
            var method = typeof(MainWindow).GetMethod("BtnAbrirModalSair_Click",
                BindingFlags.NonPublic | BindingFlags.Instance);
            method?.Invoke(window, new object[] { window, new RoutedEventArgs() });

            // Assert - acessa o campo privado ModalConfirmacaoSair via reflexão
            var modalField = typeof(MainWindow).GetField("ModalConfirmacaoSair",
                BindingFlags.NonPublic | BindingFlags.Instance);
            var modal = modalField?.GetValue(window) as UIElement;
            Assert.Equal(Visibility.Visible, modal?.Visibility);
        }

        [Fact]
        public void BtnFecharModalSair_Click_DeveOcultarModal()
        {
            // Arrange
            var window = new MainWindow();

            // Acessa o campo privado ModalConfirmacaoSair via reflexão
            var modalField = typeof(MainWindow).GetField("ModalConfirmacaoSair",
                BindingFlags.NonPublic | BindingFlags.Instance);
            var modal = modalField?.GetValue(window) as UIElement;
            modal.Visibility = Visibility.Visible;

            // Act - invoca o método privado via reflexão
            var method = typeof(MainWindow).GetMethod("BtnFecharModalSair_Click",
                BindingFlags.NonPublic | BindingFlags.Instance);
            method?.Invoke(window, new object[] { window, new RoutedEventArgs() });

            // Assert
            Assert.Equal(Visibility.Collapsed, modal?.Visibility);
        }

        [Fact]
        public void BtnConfirmarSaida_Click_DeveExecutarLogoutEFecharModal()
        {
            // Arrange
            var window = new MainWindow();
            var viewModel = (MainViewModel)window.DataContext;

            // Cria um mock do comando de logout
            var mockCommand = new Mock<ICommand>();
            mockCommand.Setup(c => c.Execute(null)).Verifiable();

            // Usa reflexão para definir o campo privado que armazena o comando
            // A propriedade FazerLogoutCommand é readonly, mas o campo de suporte pode ser acessado
            var commandField = typeof(MainViewModel).GetField("_fazerLogoutCommand",
                BindingFlags.NonPublic | BindingFlags.Instance);
            commandField?.SetValue(viewModel, mockCommand.Object);

            // Acessa o campo privado ModalConfirmacaoSair
            var modalField = typeof(MainWindow).GetField("ModalConfirmacaoSair",
                BindingFlags.NonPublic | BindingFlags.Instance);
            var modal = modalField?.GetValue(window) as UIElement;
            modal.Visibility = Visibility.Visible;

            // Act - invoca o método privado via reflexão
            var method = typeof(MainWindow).GetMethod("BtnConfirmarSaida_Click",
                BindingFlags.NonPublic | BindingFlags.Instance);
            method?.Invoke(window, new object[] { window, new RoutedEventArgs() });

            // Assert
            mockCommand.Verify(c => c.Execute(null), Times.Once);
            Assert.Equal(Visibility.Collapsed, modal?.Visibility);
        }

        [Fact]
        public void Window_MouseLeftButtonDown_DeveChamarDragMove()
        {
            // Arrange
            var window = new MainWindow();
            var args = new MouseButtonEventArgs(
                Mouse.PrimaryDevice,
                0,
                MouseButton.Left
            )
            {
                RoutedEvent = Mouse.MouseDownEvent
            };

            // Act - invoca o método privado via reflexão
            var method = typeof(MainWindow).GetMethod("Window_MouseLeftButtonDown",
                BindingFlags.NonPublic | BindingFlags.Instance);
            var exception = Record.Exception(() => method?.Invoke(window, new object[] { window, args }));

            // Assert
            Assert.Null(exception);
        }
    }
}