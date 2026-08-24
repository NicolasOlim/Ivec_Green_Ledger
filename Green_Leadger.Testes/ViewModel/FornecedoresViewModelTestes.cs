using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Iveco.Testes.Helpers;
using WpfIveco.ViewModels;
using Xunit;

namespace Iveco.Testes.ViewModel
{
    public class FornecedoresViewModelTestes
    {
        [Fact]
        public void CT13_ConsultarCnpj_ComCnpjInvalido_NaoDevePermitirBusca()
        {
            /// Arrange
            var viewModel = new FornecedorViewModel(null);
            viewModel.CnpjBusca = "123AB/0001"; /// Inválido (letras inseridas)

            /// Act
            bool podeExecutar = viewModel.ConsultarCnpjCommand.CanExecute(null);

            /// Assert
            Assert.False(podeExecutar, "O comando de consulta não deve ser permitido para um CNPJ fora do formato numérico.");
        }

        [Fact]
        public void CT15_SalvarFornecedor_SemCategoriaEsg_DeveBloquearComando()
        {
            /// Arrange
            var viewModel = new FornecedorViewModel(null);
            viewModel.NomeFornecedorEncontrado = "Iveco Parceiro";
            viewModel.CategoriaEsg = string.Empty; /// Vazio (Obrigatório para o Ledger)

            /// Act
            bool podeExecutar = viewModel.SalvarFornecedorCommand.CanExecute(null);

            /// Assert
            Assert.False(podeExecutar, "O salvamento no Ledger não deve ocorrer sem a atribuição prévia de uma categoria ESG.");
        }

        [Fact]
        public async Task CT12_ConsultarCnpj_ComSucesso_DevePreencherDados()
        {
            /// Arrange
            var mockHandler = new MockHttpMessageHandler();
            mockHandler.SendAsyncFunc = request => new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("{\"razao_social\": \"BOSCH LTDA\", \"municipio\": \"Curitiba\", \"uf\": \"SP\"}")
            };
            var httpClient = new HttpClient(mockHandler) { BaseAddress = new Uri("https://brasilapi.com.br/") };
            var viewModel = new FornecedorViewModel(httpClient);
            viewModel.CnpjBusca = "00000000000191";

            /// Act
            viewModel.ConsultarCnpjCommand.Execute(null);
            /// Aguarda um pequeno delay para a Task assíncrona do Command concluir (caso não use await direto)
            await Task.Delay(100);

            /// Assert
            Assert.Contains("BOSCH", viewModel.NomeFornecedorEncontrado);
        }
    }
}