using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WpfIveco.ViewModel;

namespace WpfIveco
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            /// LIGA O XAML AO SEU MOTOR L GICO
            DataContext = new MainViewModel();
        }

        /// <summary>
        /// --- M TODOS DE CONTROLO DA JANELA ---
        /// </summary>
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == System.Windows.Input.MouseButton.Left)
                this.DragMove();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Normal)
                this.WindowState = WindowState.Maximized;
            else
                this.WindowState = WindowState.Normal;

        }

        private void SenhaPasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is MainViewModel viewModel)
            {
                viewModel.LoginSenha = ((PasswordBox)sender).Password;
            }
        }


        /// <summary>
        /// --- M TODOS DE MÁSCARA E VALIDAÇÃO DE INPUT ---
        /// </summary>
        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void CnpjTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                string apenasDigitos = new string(textBox.Text.Where(char.IsDigit).ToArray());

                if (apenasDigitos.Length > 14)
                    apenasDigitos = apenasDigitos.Substring(0, 14);

                string cnpjFormatado = "";
                if (apenasDigitos.Length > 0)
                {
                    if (apenasDigitos.Length <= 2)
                        cnpjFormatado = apenasDigitos;
                    else if (apenasDigitos.Length <= 5)
                        cnpjFormatado = $"{apenasDigitos.Substring(0, 2)}.{apenasDigitos.Substring(2)}";
                    else if (apenasDigitos.Length <= 8)
                        cnpjFormatado = $"{apenasDigitos.Substring(0, 2)}.{apenasDigitos.Substring(2, 3)}.{apenasDigitos.Substring(5)}";
                    else if (apenasDigitos.Length <= 12)
                        cnpjFormatado = $"{apenasDigitos.Substring(0, 2)}.{apenasDigitos.Substring(2, 3)}.{apenasDigitos.Substring(5, 3)}/{apenasDigitos.Substring(8)}";
                    else
                        cnpjFormatado = $"{apenasDigitos.Substring(0, 2)}.{apenasDigitos.Substring(2, 3)}.{apenasDigitos.Substring(5, 3)}/{apenasDigitos.Substring(8, 4)}-{apenasDigitos.Substring(12)}";
                }

                textBox.TextChanged -= CnpjTextBox_TextChanged;

                int posicaoCursorAnterior = textBox.CaretIndex;
                int tamanhoAnterior = textBox.Text.Length;

                textBox.Text = cnpjFormatado;

                int tamanhoAtual = textBox.Text.Length;
                int diferencaTamanho = tamanhoAtual - tamanhoAnterior;
                textBox.CaretIndex = Math.Max(0, Math.Min(tamanhoAtual, posicaoCursorAnterior + diferencaTamanho));

                textBox.TextChanged += CnpjTextBox_TextChanged;
            }
        }


        private void BtnMostrarSenha_Click(object sender, RoutedEventArgs e)
        {
            if (SenhaVisivelTextBox.Visibility == Visibility.Collapsed)
            {
                // Alterna para MODO VISÍVEL (Mostra o texto)
                SenhaVisivelTextBox.Text = SenhaPasswordBox.Password;
                SenhaVisivelTextBox.Visibility = Visibility.Visible;
                SenhaPasswordBox.Visibility = Visibility.Collapsed;

                // Troca o ícone para "Olho Fechado/Riscado"
                IconeOlho.Text = "\uED1A";
            }
            else
            {
                // Alterna para MODO OCULTO (Mostra as bolinhas)
                SenhaPasswordBox.Password = SenhaVisivelTextBox.Text;
                SenhaVisivelTextBox.Visibility = Visibility.Collapsed;
                SenhaPasswordBox.Visibility = Visibility.Visible;

                // Troca o ícone de volta para "Olho Aberto"
                IconeOlho.Text = "\uE18B";
            }
        }

        private void BtnAbrirModalSair_Click(object sender, RoutedEventArgs e)
        {
            ModalConfirmacaoSair.Visibility = Visibility.Visible;
            // Fecha o popup de perfil (opcional)
            BtnMenuPerfil.IsChecked = false;
        }

        private void BtnFecharModalSair_Click(object sender, RoutedEventArgs e)
        {
            ModalConfirmacaoSair.Visibility = Visibility.Collapsed;
        }

        private void BtnConfirmarSaida_Click(object sender, RoutedEventArgs e)
        {
            // Executa o logout (chama o comando ou método do ViewModel)
            if (DataContext is MainViewModel viewModel)
            {
                viewModel.FazerLogoutCommand?.Execute(null);
            }
            // Fecha o modal
            ModalConfirmacaoSair.Visibility = Visibility.Collapsed;
        }
    }


}