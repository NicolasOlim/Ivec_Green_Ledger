using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WpfIveco.ViewModels
{
    // Classe base que avisa a tela (XAML) toda vez que uma variável for atualizada
    public class ViewModelBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}