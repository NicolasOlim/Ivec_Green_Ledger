using System.Windows;
using System.Windows.Input;
using WpfIveco.ViewModel;

namespace WpfIveco
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            // LIGA O XAML AO SEU MOTOR LÓGICO
            DataContext = new MainViewModel();
        }

        // --- MÉTODOS DE CONTROLO DA JANELA ---
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left) DragMove();
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
            WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}