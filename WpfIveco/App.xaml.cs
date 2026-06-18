using System.Configuration;
using System.Data;
using System.Windows;

namespace WpfIveco
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            
            QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
        }
    }

}
