using System.ComponentModel;
using WpfIveco.ViewModels;
using Xunit;

namespace Iveco.Testes.ViewModel
{
    /// <summary>
    /// Classe "Mock" temporária para testar a base
    /// </summary>
    public class ViewModelMock : ViewModelBase
    {
        private string _minhaPropriedade;
        public string MinhaPropriedade
        {
            get => _minhaPropriedade;
            set { _minhaPropriedade = value; OnPropertyChanged(); }
        }
    }

    public class ViewModelBaseTestes
    {
        [Fact]
        public void OnPropertyChanged_DeveDispararEvento_ComONomeDaPropriedadeCorreta()
        {
            /// Arrange
            var viewModel = new ViewModelMock();
            string propriedadeAlterada = null;

            viewModel.PropertyChanged += (sender, args) =>
            {
                propriedadeAlterada = args.PropertyName;
            };

            /// Act
            viewModel.MinhaPropriedade = "Teste Green Ledger";

            /// Assert
            Assert.Equal(nameof(ViewModelMock.MinhaPropriedade), propriedadeAlterada);
        }
    }
}