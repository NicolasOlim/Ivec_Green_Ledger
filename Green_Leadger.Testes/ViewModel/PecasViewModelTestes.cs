using WpfIveco.ViewModel;
using Xunit;

namespace Iveco.Testes.ViewModel
{
    public class PecasViewModelTestes
    {
        [Theory]
        [InlineData(-5.00, false)] /// Negativo (Inválido)
        [InlineData(0.00, true)]   /// Limite inferior (Válido)
        [InlineData(65.50, true)]  /// Caso comum (Válido)
        public void CT17_CT18_AdicionarPeca_ValidacaoDePeso(double peso, bool esperado)
        {
            /// Arrange
            var viewModel = new PecasViewModel(null);
            viewModel.VinSelecionado = "ZCFA1E02008123456";
            viewModel.FornecedorSelecionado = new WpfIveco.Models.FornecedorModel { Nome = "Bosch" };
            viewModel.NovaPecaNome = "Motor";
            viewModel.NovaPecaPesoKg = peso;

            /// Act
            bool podeExecutar = viewModel.AdicionarPecaManualCommand.CanExecute(null);

            /// Assert
            Assert.Equal(esperado, podeExecutar);
        }

        [Fact]
        public void CT19_AdicionarPeca_FaltandoVinOuFornecedor_DeveBloquear()
        {
            /// Arrange
            var viewModel = new PecasViewModel(null);
            viewModel.VinSelecionado = null; // Faltando VIN
            viewModel.FornecedorSelecionado = null; // Faltando Fornecedor
            viewModel.NovaPecaNome = "Filtro de Ar";
            viewModel.NovaPecaPesoKg = 2.5;

            /// Act
            bool podeExecutar = viewModel.AdicionarPecaManualCommand.CanExecute(null);

            /// Assert
            Assert.False(podeExecutar, "O comando deve ser bloqueado se faltar vínculos obrigatórios (VIN/Fornecedor).");
        }
    }
}