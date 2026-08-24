using Iveco.Testes.Helpers;
using Moq;
using System;
using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using WpfIveco.ViewModels;
using Xunit;

namespace Iveco.Testes.ViewModel
{
    public class DashboardViewModelTestes
    {
        [Fact]
        public async Task CT01_AtualizarPegadaMedia_ComSucesso_DeveAtualizarPropriedades()
        {
            /// Arrange
            var mockHandler = new MockHttpMessageHandler();
            mockHandler.SendAsyncFunc = request => new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(JsonSerializer.Serialize(new { pegadaMedia = 590.4 }))
            };
            var httpClient = new HttpClient(mockHandler) { BaseAddress = new Uri("https://api.fake.com/") };
            var viewModel = new DashboardViewModel(httpClient);

            /// Act
            await viewModel.AtualizarPegadaMediaAsync();

            /// Assert
            Assert.Contains("590", viewModel.PegadaMediaFormatada);
        }

        [Fact]
        public async Task CT03_AtualizarPegadaMedia_ComFalhaDeRede_NaoDeveQuebrarAcesso()
        {
            /// Arrange
            var mockHandler = new MockHttpMessageHandler();
            mockHandler.SendAsyncFunc = request => throw new HttpRequestException("Sem internet");
            var httpClient = new HttpClient(mockHandler) { BaseAddress = new Uri("https://api.fake.com/") };
            var viewModel = new DashboardViewModel(httpClient);

            /// Act
            await viewModel.AtualizarPegadaMediaAsync();

            /// Assert
            Assert.Equal("Indisponível", viewModel.PegadaMediaFormatada);
        }
    }
}