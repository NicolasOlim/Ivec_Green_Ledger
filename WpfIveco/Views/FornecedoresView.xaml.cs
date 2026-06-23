using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Input;

namespace WpfIveco.Views
{
    public partial class FornecedoresView : UserControl
    {
        public FornecedoresView() => InitializeComponent();

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

    }
}