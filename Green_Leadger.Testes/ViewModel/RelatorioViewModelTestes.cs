using System;
using System.Net.Http;
using WpfIveco.ViewModel;
using Xunit;

namespace Iveco.Testes.ViewModel
{
    public class RelatorioViewModelTestes
    {
        [Fact]
        public void MudarTipoRelatorio_DeveAtualizarATagDeContextoCorretamente()
        {
            /// Arrange
            var httpClient = new HttpClient { BaseAddress = new Uri("https://apiivecogreenledger.runasp.net/") };
            var viewModel = new RelatoriosViewModel(httpClient);
            string tipoSelecionado = "Fornecedores";

            /// Act
            viewModel.MudarTipoRelatorioCommand.Execute(tipoSelecionado);

            // Assert
            Assert.Equal(tipoSelecionado, viewModel.TipoRelatorio);
        }

        [Fact]
        public void GerarRelatorioPdfCommand_DeveEstarDisponivelSempre()
        {
            /// Arrange
            var httpClient = new HttpClient { BaseAddress = new Uri("https://apiivecogreenledger.runasp.net/") };
            var viewModel = new RelatoriosViewModel(httpClient);

            /// Act
            bool podeGerar = viewModel.GerarRelatorioPdfCommand.CanExecute(null);

            /// Assert
            Assert.True(podeGerar, "O botão de gerar relatório nunca deve estar desabilitado na view.");
        }
    }
}