using System.Windows;
using System.Windows.Controls;
using WpfIveco.ViewModel;

namespace WpfIveco.Views
{
    public partial class RelatoriosView : UserControl
    {
        public RelatoriosView()
        {
            InitializeComponent();
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            var radio = sender as RadioButton;
            var tipo = radio?.Tag?.ToString();
            if (!string.IsNullOrEmpty(tipo) && DataContext is MainViewModel mainVm)
            {
                mainVm.Relatorios.MudarTipoRelatorioCommand?.Execute(tipo);
            }
        }
    }
}