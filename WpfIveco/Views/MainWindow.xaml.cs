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

            /// LIGA O XAML AO SEU MOTOR LÓGICO
            DataContext = new MainViewModel();
        }

        /// <summary>
        /// --- MÉTODOS DE CONTROLO DA JANELA ---
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
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


    }
}