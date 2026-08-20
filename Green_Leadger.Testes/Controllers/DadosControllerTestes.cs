using ApiIveco.Controllers;
using ApiIveco.Data;
using ApiIveco.DTO;
using ApiIveco.DTOs;          // GraficoEmissoesDto, AnalisesESGDto, LoginDto
using ApiIveco.Models;
using ApiIveco.Service;
using ApiIveco.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Iveco.Testes.Controllers
{
    public class DadosControllerTestes
    {
        private readonly Mock<DadosService> _dadosServiceMock;
        private readonly Mock<IEmailValidationService> _emailValidationMock;
        private readonly Mock<ILogger<DadosController>> _loggerMock;
        private readonly DadosController _controller;

        public DadosControllerTestes()
        {
            // Cria mocks das dependências do DadosService
            var loggerService = Mock.Of<ILogger<DadosService>>();
            var firebase = Mock.Of<FireBaseData>();
            var memoryCache = Mock.Of<IMemoryCache>();

            _dadosServiceMock = new Mock<DadosService>(loggerService, firebase, memoryCache)
            {
                CallBase = true
            };

            _emailValidationMock = new Mock<IEmailValidationService>();
            _loggerMock = new Mock<ILogger<DadosController>>();

            _controller = new DadosController(
                _dadosServiceMock.Object,
                _loggerMock.Object,
                _emailValidationMock.Object
            );
        }

        #region Veículos

        [Fact]
        public async Task GetVeiculos_DeveRetornarOk_ComListaDeVeiculos()
        {
            // Arrange
            var veiculosEsperados = new List<Veiculo>
            {
                new Veiculo { Vin = "ABC123", Modelo = "Daily" }
            };
            _dadosServiceMock.Setup(s => s.ListarVeiculo())
                .ReturnsAsync(veiculosEsperados);

            // Act
            var resultado = await _controller.GetVeiculos();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(resultado);
            Assert.Equal(veiculosEsperados, okResult.Value);
        }

        [Fact]
        public async Task GetVeiculoByVin_ComVinInvalido_DeveRetornarBadRequest()
        {
            // Arrange
            string vin = null;

            // Act
            var resultado = await _controller.GetVeiculoByVin(vin);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(resultado);
            Assert.Equal(new { Mensagem = "Os dados enviados são inválidos." }, badRequest.Value);
        }

        [Fact]
        public async Task GetVeiculoByVin_ComVinNaoEncontrado_DeveRetornarNotFound()
        {
            // Arrange
            string vin = "INEXISTENTE";
            _dadosServiceMock.Setup(s => s.ObterVeiculoPorVin(vin))
                .ReturnsAsync((Veiculo)null);

            // Act
            var resultado = await _controller.GetVeiculoByVin(vin);

            // Assert
            var notFound = Assert.IsType<NotFoundObjectResult>(resultado);
            Assert.Equal(new { Mensagem = "O recurso solicitado não foi encontrado." }, notFound.Value);
        }

        [Fact]
        public async Task GetVeiculoByVin_ComVinExistente_DeveRetornarOkComVeiculo()
        {
            // Arrange
            string vin = "ABC123";
            var veiculo = new Veiculo { Vin = vin, Modelo = "Daily" };
            _dadosServiceMock.Setup(s => s.ObterVeiculoPorVin(vin))
                .ReturnsAsync(veiculo);

            // Act
            var resultado = await _controller.GetVeiculoByVin(vin);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(resultado);
            var expected = new { mensagem = "Veículo encontrado", veiculo };
            Assert.Equal(expected, okResult.Value);
        }

        [Fact]
        public async Task PostVeiculo_ComVeiculoNulo_DeveRetornarBadRequest()
        {
            // Act
            var resultado = await _controller.PostVeiculo(null);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(resultado);
            Assert.Equal(new { Mensagem = "Os dados enviados são inválidos." }, badRequest.Value);
        }

        [Fact]
        public async Task PostVeiculo_ComVinDuplicado_DeveRetornarConflict()
        {
            // Arrange
            var veiculo = new Veiculo { Vin = "DUPLICADO" };
            var listaExistente = new List<Veiculo> { new Veiculo { Vin = "DUPLICADO" } };
            _dadosServiceMock.Setup(s => s.ListarVeiculo())
                .ReturnsAsync(listaExistente);

            // Act
            var resultado = await _controller.PostVeiculo(veiculo);

            // Assert
            var conflict = Assert.IsType<ConflictObjectResult>(resultado);
            Assert.Equal(new { Mensagem = "Veículo com VIN 'DUPLICADO' já cadastrado." }, conflict.Value);
        }

        [Fact]
        public async Task PostVeiculo_ComVinValido_DeveChamarCriarVeiculo_EGerarComponentes_ERetornarOk()
        {
            // Arrange
            var veiculo = new Veiculo { Vin = "NOVO" };
            var veiculoCriado = new Veiculo { Vin = "NOVO", Modelo = "Daily" };
            _dadosServiceMock.Setup(s => s.ListarVeiculo())
                .ReturnsAsync(new List<Veiculo>());
            _dadosServiceMock.Setup(s => s.CriarVeiculo(veiculo))
                .ReturnsAsync(veiculoCriado);
            _dadosServiceMock.Setup(s => s.GerarComponentesParaVeiculoAsync(veiculo.Vin))
                .Returns(Task.CompletedTask);

            // Act
            var resultado = await _controller.PostVeiculo(veiculo);

            // Assert
            _dadosServiceMock.Verify(s => s.CriarVeiculo(veiculo), Times.Once);
            _dadosServiceMock.Verify(s => s.GerarComponentesParaVeiculoAsync(veiculo.Vin), Times.Once);

            var okResult = Assert.IsType<OkObjectResult>(resultado);
            var expected = new { mensagem = "Veículo registrado e peças vinculadas com sucesso!", veiculo = veiculoCriado };
            Assert.Equal(expected, okResult.Value);
        }

        [Fact]
        public async Task PutVeiculo_ComVinDiferente_DeveRetornarBadRequest()
        {
            // Act
            var resultado = await _controller.PutVeiculo("URL", new Veiculo { Vin = "DIFERENTE" });

            // Assert
            Assert.IsType<BadRequestObjectResult>(resultado);
        }

        [Fact]
        public async Task PutVeiculo_ComVeiculoNaoEncontrado_DeveRetornarNotFound()
        {
            // Arrange
            string vin = "NAOEXISTE";
            _dadosServiceMock.Setup(s => s.AtualizarVeiculo(vin, It.IsAny<Veiculo>()))
                .ReturnsAsync((Veiculo)null);

            // Act
            var resultado = await _controller.PutVeiculo(vin, new Veiculo { Vin = vin });

            // Assert
            Assert.IsType<NotFoundObjectResult>(resultado);
        }

        [Fact]
        public async Task PutVeiculo_ComSucesso_DeveRetornarOk()
        {
            // Arrange
            string vin = "EXISTE";
            var veiculoAtualizado = new Veiculo { Vin = vin, Modelo = "Atualizado" };
            _dadosServiceMock.Setup(s => s.AtualizarVeiculo(vin, It.IsAny<Veiculo>()))
                .ReturnsAsync(veiculoAtualizado);

            // Act
            var resultado = await _controller.PutVeiculo(vin, veiculoAtualizado);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(resultado);
            var expected = new { mensagem = "Veículo atualizado com sucesso!", veiculo = veiculoAtualizado };
            Assert.Equal(expected, okResult.Value);
        }

        [Fact]
        public async Task DeleteVeiculo_ComVinNaoEncontrado_DeveRetornarNotFound()
        {
            // Arrange
            _dadosServiceMock.Setup(s => s.ObterVeiculoPorVin("INEXISTENTE"))
                .ReturnsAsync((Veiculo)null);

            // Act
            var resultado = await _controller.DeleteVeiculo("INEXISTENTE");

            // Assert
            Assert.IsType<NotFoundObjectResult>(resultado);
        }

        [Fact]
        public async Task DeleteVeiculo_ComVinExistente_DeveRetornarOk()
        {
            // Arrange
            string vin = "EXISTE";
            _dadosServiceMock.Setup(s => s.ObterVeiculoPorVin(vin))
                .ReturnsAsync(new Veiculo { Vin = vin });

            // Act
            var resultado = await _controller.DeleteVeiculo(vin);

            // Assert
            _dadosServiceMock.Verify(s => s.ExcluirVeiculo(vin), Times.Once);
            var okResult = Assert.IsType<OkObjectResult>(resultado);
            Assert.Equal(new { mensagem = "Veículo deletado com sucesso." }, okResult.Value);
        }

        [Fact]
        public async Task ValidarVinIveco_ComVinTamanhoInvalido_DeveRetornarBadRequest()
        {
            // Act
            var resultado = await _controller.ValidarVinIveco("123");

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(resultado);
            Assert.Equal(new { Mensagem = "Os dados enviados são inválidos. O VIN deve ter 17 caracteres." }, badRequest.Value);
        }

        [Fact]
        public async Task ValidarVinIveco_ComVinInexistente_DeveRetornarNotFound()
        {
            // Arrange
            string vin = "12345678901234567";
            _dadosServiceMock.Setup(s => s.BuscarEValidarVinIvecoAsync(vin))
                .ReturnsAsync((Veiculo)null);

            // Act
            var resultado = await _controller.ValidarVinIveco(vin);

            // Assert
            var notFound = Assert.IsType<NotFoundObjectResult>(resultado);
            Assert.Equal(new { Mensagem = "O recurso solicitado não foi encontrado na NHTSA." }, notFound.Value);
        }

        [Fact]
        public async Task ValidarVinIveco_ComVinIvecoValido_DeveRetornarOk()
        {
            // Arrange
            string vin = "ZCFA1E02008123456";
            var veiculo = new Veiculo { Vin = vin, Modelo = "Daily" };
            _dadosServiceMock.Setup(s => s.BuscarEValidarVinIvecoAsync(vin))
                .ReturnsAsync(veiculo);

            // Act
            var resultado = await _controller.ValidarVinIveco(vin);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(resultado);
            var expected = new { mensagem = "Veículo IVECO validado com sucesso!", veiculo };
            Assert.Equal(expected, okResult.Value);
        }

        [Fact]
        public async Task ValidarVinIveco_QuandoServicoLancaExcecao_DeveRetornarBadRequest()
        {
            // Arrange
            string vin = "INVALIDO";
            _dadosServiceMock.Setup(s => s.BuscarEValidarVinIvecoAsync(vin))
                .ThrowsAsync(new Exception("Não é IVECO"));

            // Act
            var resultado = await _controller.ValidarVinIveco(vin);

            // Assert
            var badRequest = Assert.IsType<BadRequestObjectResult>(resultado);
            Assert.Equal(new { Mensagem = "O VIN informado não pertence a um veículo IVECO válido." }, badRequest.Value);
        }

        #endregion

        #region Fornecedores

        [Fact]
        public async Task GetFornecedores_DeveRetornarOk_ComListaDeFornecedores()
        {
            // Arrange
            var fornecedores = new List<Fornecedor> { new Fornecedor { Id = "1", Nome = "Bosch" } };
            _dadosServiceMock.Setup(s => s.ListarFornecedor())
                .ReturnsAsync(fornecedores);

            // Act
            var resultado = await _controller.GetFornecedores();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(resultado);
            Assert.Equal(fornecedores, okResult.Value);
        }

        [Fact]
        public async Task GetFornecedorCnpj_ComCnpjVazio_DeveRetornarBadRequest()
        {
            // Act
            var resultado = await _controller.GetFornecedorCnpj("");

            // Assert
            Assert.IsType<BadRequestObjectResult>(resultado);
        }

        [Fact]
        public async Task GetFornecedorCnpj_ComCnpjNaoEncontrado_DeveRetornarNotFound()
        {
            // Arrange
            _dadosServiceMock.Setup(s => s.BuscarFornecedorPorCnpjAsync("00000000"))
                .ReturnsAsync((Fornecedor)null);

            // Act
            var resultado = await _controller.GetFornecedorCnpj("00000000");

            // Assert
            Assert.IsType<NotFoundObjectResult>(resultado);
        }

        [Fact]
        public async Task GetFornecedorCnpj_ComCnpjEncontrado_DeveRetornarOk()
        {
            // Arrange
            var fornecedor = new Fornecedor { Id = "1", Nome = "Bosch" };
            _dadosServiceMock.Setup(s => s.BuscarFornecedorPorCnpjAsync("12345678"))
                .ReturnsAsync(fornecedor);

            // Act
            var resultado = await _controller.GetFornecedorCnpj("12345678");

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(resultado);
            var expected = new { mensagem = "Fornecedor localizado com sucesso!", fornecedor };
            Assert.Equal(expected, okResult.Value);
        }

        [Fact]
        public async Task PostFornecedor_ComFornecedorNulo_DeveRetornarBadRequest()
        {
            // Act
            var resultado = await _controller.PostFornecedor(null);

            // Assert
            Assert.IsType<BadRequestObjectResult>(resultado);
        }

        [Fact]
        public async Task PostFornecedor_ComDadosValidos_DeveRetornarOk()
        {
            // Arrange
            var fornecedor = new Fornecedor { Nome = "Bosch" };
            var fornecedorCriado = new Fornecedor { Id = "1", Nome = "Bosch" };
            _dadosServiceMock.Setup(s => s.CriarFornecedor(fornecedor))
                .ReturnsAsync(fornecedorCriado);

            // Act
            var resultado = await _controller.PostFornecedor(fornecedor);

            // Assert
            _dadosServiceMock.Verify(s => s.CriarFornecedor(fornecedor), Times.Once);
            var okResult = Assert.IsType<OkObjectResult>(resultado);
            var expected = new { mensagem = "Fornecedor registrado com sucesso!", fornecedor = fornecedorCriado };
            Assert.Equal(expected, okResult.Value);
        }

        [Fact]
        public async Task DeleteFornecedor_DeveChamarExcluirFornecedor_ERetornarOk()
        {
            // Arrange
            string id = "1";

            // Act
            var resultado = await _controller.DeleteFornecedor(id);

            // Assert
            _dadosServiceMock.Verify(s => s.ExcluirFornecedor(id), Times.Once);
            var okResult = Assert.IsType<OkObjectResult>(resultado);
            Assert.Equal(new { mensagem = "Fornecedor deletado com sucesso." }, okResult.Value);
        }

        #endregion

        #region Lotes

        [Fact]
        public async Task GetLotes_DeveRetornarOk_ComListaDeLotes()
        {
            // Arrange
            var lotes = new List<LoteMateriaPrima> { new LoteMateriaPrima { Id = "1" } };
            _dadosServiceMock.Setup(s => s.ListarLoteMateriaPrima())
                .ReturnsAsync(lotes);

            // Act
            var resultado = await _controller.GetLotes();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(resultado);
            Assert.Equal(lotes, okResult.Value);
        }

        [Fact]
        public async Task PostLote_ComLoteNulo_DeveRetornarBadRequest()
        {
            // Act
            var resultado = await _controller.PostLote(null);

            // Assert
            Assert.IsType<BadRequestObjectResult>(resultado);
        }

        [Fact]
        public async Task PostLote_ComDadosValidos_DeveRetornarOk()
        {
            // Arrange
            var lote = new LoteMateriaPrima { QuantidadeKg = 100 };
            var loteCriado = new LoteMateriaPrima { Id = "1", QuantidadeKg = 100 };
            _dadosServiceMock.Setup(s => s.CriarLoteMateriaPrima(lote))
                .ReturnsAsync(loteCriado);

            // Act
            var resultado = await _controller.PostLote(lote);

            // Assert
            _dadosServiceMock.Verify(s => s.CriarLoteMateriaPrima(lote), Times.Once);
            var okResult = Assert.IsType<OkObjectResult>(resultado);
            var expected = new { mensagem = "Lote registrado com sucesso!", lote = loteCriado };
            Assert.Equal(expected, okResult.Value);
        }

        [Fact]
        public async Task DeleteLote_DeveChamarExcluirLote_ERetornarOk()
        {
            // Arrange
            string id = "1";

            // Act
            var resultado = await _controller.DeleteLote(id);

            // Assert
            _dadosServiceMock.Verify(s => s.ExcluirLoteMateriaPrima(id), Times.Once);
            var okResult = Assert.IsType<OkObjectResult>(resultado);
            Assert.Equal(new { mensagem = "Lote deletado com sucesso." }, okResult.Value);
        }

        #endregion

        #region Componentes

        [Fact]
        public async Task GetComponentes_DeveRetornarOk_ComListaDeComponentes()
        {
            // Arrange
            var componentes = new List<VeiculoComponente> { new VeiculoComponente { Id = "1" } };
            _dadosServiceMock.Setup(s => s.ListarVeiculoComponente())
                .ReturnsAsync(componentes);

            // Act
            var resultado = await _controller.GetComponentes();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(resultado);
            Assert.Equal(componentes, okResult.Value);
        }

        [Fact]
        public async Task PostComponente_ComComponenteNulo_DeveRetornarBadRequest()
        {
            // Act
            var resultado = await _controller.PostComponente(null);

            // Assert
            Assert.IsType<BadRequestObjectResult>(resultado);
        }

        [Fact]
        public async Task PostComponente_ComDadosValidos_DeveRetornarOk()
        {
            // Arrange
            var componente = new VeiculoComponente { NomePeca = "Motor" };
            var componenteCriado = new VeiculoComponente { Id = "1", NomePeca = "Motor" };
            _dadosServiceMock.Setup(s => s.CriarVeiculoComponente(componente))
                .ReturnsAsync(componenteCriado);

            // Act
            var resultado = await _controller.PostComponente(componente);

            // Assert
            _dadosServiceMock.Verify(s => s.CriarVeiculoComponente(componente), Times.Once);
            var okResult = Assert.IsType<OkObjectResult>(resultado);
            var expected = new { mensagem = "Componente registrado com sucesso!", componente = componenteCriado };
            Assert.Equal(expected, okResult.Value);
        }

        [Fact]
        public async Task DeleteComponente_DeveChamarExcluirComponente_ERetornarOk()
        {
            // Arrange
            string id = "1";

            // Act
            var resultado = await _controller.DeleteComponente(id);

            // Assert
            _dadosServiceMock.Verify(s => s.ExcluirVeiculoComponente(id), Times.Once);
            var okResult = Assert.IsType<OkObjectResult>(resultado);
            Assert.Equal(new { mensagem = "Componente deletado com sucesso." }, okResult.Value);
        }

        #endregion

        #region Autenticação

        [Fact]
        public async Task Cadastrar_ComEmailOuSenhaVazios_DeveRetornarBadRequest()
        {
            // Arrange
            var usuario = new Usuario { Email = "", Senha = "123" };

            // Act
            var resultado = await _controller.Cadastrar(usuario);

            // Assert
            Assert.IsType<BadRequestObjectResult>(resultado);
        }

        [Fact]
        public async Task Cadastrar_ComDadosValidos_DeveRetornarOk_ComSenhaLimpa()
        {
            // Arrange
            var usuario = new Usuario { Email = "teste@iveco.com", Senha = "123", Nome = "Teste" };
            var usuarioCriado = new Usuario { Id = "1", Email = "teste@iveco.com", Senha = "123", Nome = "Teste", Acesso = "Usuario" };
            _dadosServiceMock.Setup(s => s.CadastrarUsuario(usuario))
                .ReturnsAsync(usuarioCriado);

            // Act
            var resultado = await _controller.Cadastrar(usuario);

            // Assert
            _dadosServiceMock.Verify(s => s.CadastrarUsuario(usuario), Times.Once);
            var okResult = Assert.IsType<OkObjectResult>(resultado);
            var usuarioRetornado = okResult.Value.GetType().GetProperty("usuario")?.GetValue(okResult.Value) as Usuario;
            Assert.Empty(usuarioRetornado.Senha);
        }

        [Fact]
        public async Task Login_ComCredenciaisVazias_DeveRetornarBadRequest()
        {
            // Arrange
            var credenciais = new LoginDto { Email = "", Senha = "" };

            // Act
            var resultado = await _controller.Login(credenciais);

            // Assert
            Assert.IsType<BadRequestObjectResult>(resultado);
        }

        [Fact]
        public async Task Login_ComCredenciaisInvalidas_DeveRetornarUnauthorized()
        {
            // Arrange
            var credenciais = new LoginDto { Email = "invalido@iveco.com", Senha = "errada" };
            _dadosServiceMock.Setup(s => s.FazerLogin(credenciais.Email, credenciais.Senha))
                .ReturnsAsync((Usuario)null);

            // Act
            var resultado = await _controller.Login(credenciais);

            // Assert
            var unauthorized = Assert.IsType<UnauthorizedObjectResult>(resultado);
            Assert.Equal(new { Mensagem = "Credenciais incorretas. Verifique o e-mail e a senha." }, unauthorized.Value);
        }

        [Fact]
        public async Task Login_ComCredenciaisValidas_DeveRetornarOk_ComSenhaLimpa()
        {
            // Arrange
            var credenciais = new LoginDto { Email = "joao@iveco.com", Senha = "123" };
            var usuario = new Usuario { Id = "1", Email = "joao@iveco.com", Senha = "123", Nome = "João", Acesso = "Usuario" };
            _dadosServiceMock.Setup(s => s.FazerLogin(credenciais.Email, credenciais.Senha))
                .ReturnsAsync(usuario);

            // Act
            var resultado = await _controller.Login(credenciais);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(resultado);
            var usuarioRetornado = okResult.Value.GetType().GetProperty("usuario")?.GetValue(okResult.Value) as Usuario;
            Assert.Empty(usuarioRetornado.Senha);
        }

        [Fact]
        public async Task ValidarEmail_DeveRetornarResultadoDoServico()
        {
            // Arrange
            string email = "teste@iveco.com";
            _emailValidationMock.Setup(e => e.ValidateEmailAsync(email))
                .ReturnsAsync((true, "Válido"));

            // Act
            var resultado = await _controller.ValidarEmail(email);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(resultado);
            var expected = new { valido = true, mensagem = "Válido" };
            Assert.Equal(expected, okResult.Value);
        }

        #endregion

        #region Dashboard

        [Fact]
        public async Task GetPegadaMedia_DeveRetornarOk_ComValor()
        {
            // Arrange
            _dadosServiceMock.Setup(s => s.CalcularPegadaMediaAsync())
                .ReturnsAsync(42.5);

            // Act
            var resultado = await _controller.GetPegadaMedia();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(resultado);
            Assert.Equal(new { pegadaMedia = 42.5 }, okResult.Value);
        }

        [Fact]
        public async Task GetDadosGrafico_DeveRetornarOk()
        {
            // Arrange
            var dados = new GraficoEmissoesDto();
            _dadosServiceMock.Setup(s => s.ObterDadosGraficoAsync())
                .ReturnsAsync(dados);

            // Act
            var resultado = await _controller.GetDadosGrafico();

            // Assert
            Assert.IsType<OkObjectResult>(resultado);
        }

        [Fact]
        public async Task GetDadosAnalisesESG_DeveRetornarOk()
        {
            // Arrange
            _dadosServiceMock.Setup(s => s.ObterDadosAnalisesESGAsync())
                .ReturnsAsync(new AnalisesESGDto());

            // Act
            var resultado = await _controller.GetDadosAnalisesESG();

            // Assert
            Assert.IsType<OkObjectResult>(resultado);
        }

        [Fact]
        public async Task GetTotalEmissoes_DeveRetornarOk_ComTotal()
        {
            // Arrange
            _dadosServiceMock.Setup(s => s.CalcularTotalEmissoesAsync())
                .ReturnsAsync(1000.0);

            // Act
            var resultado = await _controller.GetTotalEmissoes();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(resultado);
            Assert.Equal(new { totalEmissoes = 1000.0 }, okResult.Value);
        }

        [Fact]
        public async Task GetPrecoCarbono_DeveRetornarOk_ComPreco()
        {
            // Arrange
            _dadosServiceMock.Setup(s => s.ObterPrecoCarbonoAsync())
                .ReturnsAsync(150.0);

            // Act
            var resultado = await _controller.GetPrecoCarbono();

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(resultado);
            Assert.Equal(new { preco = 150.0 }, okResult.Value);
        }

        #endregion
    }
}