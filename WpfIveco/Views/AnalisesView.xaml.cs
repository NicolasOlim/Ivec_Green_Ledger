using System.Windows.Controls;
using WpfIveco.ViewModels; // Necessário para encontrar o seu ViewModel

namespace WpfIveco.Views
{
    public partial class AnalisesView : UserControl
    {
        public AnalisesView()
        {
            InitializeComponent();

            // Liga esta tela (View) aos dados que criamos (ViewModel)
            DataContext = new AnalisesViewModel();
        }
    }
}