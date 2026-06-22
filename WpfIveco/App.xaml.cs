using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace WpfIveco
{
    public partial class App : Application
    {
        public App()
        {
            // Captura exceções na thread da UI
            this.DispatcherUnhandledException += (s, e) =>
            {
                Debug.WriteLine($"[EXCEÇÃO UI] {e.Exception.GetType().Name}: {e.Exception.Message}");
                Debug.WriteLine(e.Exception.StackTrace);
                e.Handled = true;
                MessageBox.Show("Ocorreu um erro inesperado. O sistema tentará continuar.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            };

            // Captura exceções em outras threads
            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                if (e.ExceptionObject is Exception ex)
                {
                    Debug.WriteLine($"[EXCEÇÃO APP] {ex.GetType().Name}: {ex.Message}");
                    Debug.WriteLine(ex.StackTrace);
                }
                else
                {
                    Debug.WriteLine($"[EXCEÇÃO APP] Objeto não-Exception: {e.ExceptionObject}");
                }
            };

            // Captura exceções em tasks não observadas
            TaskScheduler.UnobservedTaskException += (s, e) =>
            {
                Debug.WriteLine($"[EXCEÇÃO TASK] {e.Exception.GetType().Name}: {e.Exception.Message}");
                Debug.WriteLine(e.Exception.StackTrace);
                e.SetObserved();
            };
        }
    }
}