using System.Threading.Tasks;
using WpfIveco.ViewModel;
using Xunit;

namespace Iveco.Testes.ViewModel
{
    public class MainViewModelTestes
    {
        [Theory]
        [InlineData("1234567890123456")]   // 16 caracteres (Inválido)
        [InlineData("123456789012345678")] // 18 caracteres (Inválido)
        [InlineData("123456789I1234567")]  // Contém 'I' proibido (Inválido)
        [InlineData("123456789O1234567")]  // Contém 'O' proibido (Inválido)
        [InlineData("123456789Q1234567")]  // Contém 'Q' proibido (Inválido)
        public void CT20_CT21_ValidarVin_EntradasInvalidasELimites_DevemSerRejeitadas(string vin)
        {
            /// Arrange
            var viewModel = new MainViewModel(); // Ou RastreabilidadeViewModel, dependendo da sua injeção
            viewModel.Rastreabilidade.PesquisaVin = vin;

            /// Act
            bool podePesquisar = viewModel.Rastreabilidade.PesquisarVinCommand.CanExecute(null);

            /// Assert
            Assert.False(podePesquisar, $"A pesquisa deveria ser bloqueada para o VIN inválido: {vin}");
        }

        [Fact]
        public void CT23_FazerLogin_SenhaIncorreta_DeveSinalizarErroNaInterface()
        {
            /// Arrange
            var viewModel = new MainViewModel();
            viewModel.LoginEmail = "admin@iveco.com";
            viewModel.LoginSenha = "senhaErrada";

            /// Act
            viewModel.FazerLoginCommand.Execute(null);

            /// Assert
            Assert.True(viewModel.HasLoginError);
            Assert.False(string.IsNullOrEmpty(viewModel.LoginError));
            Assert.False(viewModel.IsLoggedIn);
        }
    }
}