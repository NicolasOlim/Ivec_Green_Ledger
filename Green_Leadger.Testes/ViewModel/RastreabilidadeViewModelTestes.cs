using WpfIveco.ViewModel;
using WpfIveco.ViewModels;
using Xunit;

namespace Iveco.Testes.ViewModel
{
    public class RastreabilidadeViewModelTestes
    {
        [Theory]
        [InlineData("1234567890123456")]   /// 16 caracteres (Inválido)
        [InlineData("123456789012345678")] /// 18 caracteres (Inválido)
        [InlineData("123456789I1234567")]  /// Contém 'I' proibido (Inválido)
        [InlineData("123456789O1234567")]  /// Contém 'O' proibido (Inválido)
        [InlineData("123456789Q1234567")]  /// Contém 'Q' proibido (Inválido)
        [InlineData("ZCFA1E02008123456")]  /// Padrão válido IVECO (Válido)
        public void CT20_CT21_ValidarVin_EntradasInvalidasELimites_DevemSerRejeitadas(string vin)
        {
            // Arrange
            var viewModel = new RastreabilidadeViewModel(null);
            viewModel.PesquisaVin = vin;

            /// Act
            bool podePesquisar = viewModel.PesquisarVinCommand.CanExecute(null);

            /// Assert
            /// Se o VIN tiver exatamente 17 caracteres e não contiver I,O,Q, deve retornar true.
            bool ehValido = vin.Length == 17 && !vin.Contains("I") && !vin.Contains("O") && !vin.Contains("Q");
            Assert.Equal(ehValido, podePesquisar);
        }
    }
}