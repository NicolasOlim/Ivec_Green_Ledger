using Iveco.Testes.Helpers;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using WpfIveco.ViewModels;
using Xunit;

namespace Iveco.Testes.ViewModel
{
    public class AnalisesViewModelTestes
    {
        [Theory]
        [InlineData(1000.0, "R$ 150,0K")]
        [InlineData(1000000.0, "R$ 150,0M")]
        public async Task CT06_CT07_CarregarTotalEmissoes_DeveFormatarEconomiaGeradaCorretamente(double totalEmissoes, string formatacaoEsperada)
        {
            /// Arrange
            var mockHandler = new MockHttpMessageHandler();
            mockHandler.SendAsyncFunc = request =>
            {
                if (request.RequestUri.AbsolutePath.Contains("total-emissoes"))
                    return new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent($"{{\"totalEmissoes\": {totalEmissoes * 1000}}}") };
                if (request.RequestUri.AbsolutePath.Contains("preco-carbono"))
                    return new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent("{\"preco\": 150.0}") };

                return new HttpResponseMessage { StatusCode = HttpStatusCode.NotFound };
            };

            var httpClient = new HttpClient(mockHandler) { BaseAddress = new Uri("https://api.fake.com/") };
            var viewModel = new AnalisesViewModel(httpClient);

            /// Act
            await viewModel.AtualizarAsync();

            /// Assert
            Assert.Equal(formatacaoEsperada, viewModel.EconomiaGerada);
        }

        [Fact]
        public async Task CT09_PrecoCarbonoFalha_DeveUsarFallbackCorretamente()
        {
            /// Arrange
            var mockHandler = new MockHttpMessageHandler();
            mockHandler.SendAsyncFunc = request =>
            {
                if (request.RequestUri.AbsolutePath.Contains("total-emissoes"))
                    return new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent("{\"totalEmissoes\": 1000000}") }; // 1000 ton
                if (request.RequestUri.AbsolutePath.Contains("preco-carbono"))
                    return new HttpResponseMessage { StatusCode = HttpStatusCode.InternalServerError }; // Simulando falha

                return new HttpResponseMessage { StatusCode = HttpStatusCode.OK, Content = new StringContent("{}") };
            };

            var httpClient = new HttpClient(mockHandler) { BaseAddress = new Uri("https://api.fake.com/") };
            var viewModel = new AnalisesViewModel(httpClient);

            /// Act
            await viewModel.AtualizarAsync();

            /// Assert
            /// Fallback é 150.0. 1000 ton * 150 = 150.000 (150K)
            Assert.Equal("R$ 150,0K", viewModel.EconomiaGerada);
        }
    }
}