using ApiIveco.Data;
using ApiIveco.Models;
using ApiIveco.Service;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Threading.Tasks;
using Xunit;

namespace Iveco.Testes.Services
{
    public class DadosServiceTestes
    {
        private readonly Mock<ILogger<DadosService>> _loggerMock;
        private readonly Mock<IMemoryCache> _cacheMock;
        private readonly DadosService _dadosService;

        public DadosServiceTestes()
        {
            // Arrange base para todos os testes
            _loggerMock = new Mock<ILogger<DadosService>>();
            _cacheMock = new Mock<IMemoryCache>();

            // Passamos null para o FireBaseData nestes testes específicos, 
            // pois estamos testando as validações que ocorrem ANTES do acesso ao banco[cite: 6].
            _dadosService = new DadosService(_loggerMock.Object, null, _cacheMock.Object);
        }

        #region Validações de LoteMateriaPrima

        [Theory]
        [InlineData(0)]
        [InlineData(-10.5)]
        public async Task CriarLoteMateriaPrima_QuantidadeInvalida_DeveLancarArgumentException(double quantidadeInvalida)
        {
            // Arrange
            var lote = new LoteMateriaPrima
            {
                QuantidadeKg = quantidadeInvalida, // A quantidade deve ser maior que zero[cite: 6]
                PegadaCarbonoPorKg = 1.0
            };

            // Act & Assert
            var excecao = await Assert.ThrowsAsync<ArgumentException>(() => _dadosService.CriarLoteMateriaPrima(lote));
            Assert.Equal("A quantidade de matéria-prima (Kg) deve ser maior que zero.", excecao.Message);
        }

        [Fact]
        public async Task CriarLoteMateriaPrima_PegadaCarbonoNegativa_DeveLancarArgumentException()
        {
            // Arrange
            var lote = new LoteMateriaPrima
            {
                QuantidadeKg = 100,
                PegadaCarbonoPorKg = -0.5 // A pegada de carbono não pode ser negativa[cite: 6]
            };

            // Act & Assert
            var excecao = await Assert.ThrowsAsync<ArgumentException>(() => _dadosService.CriarLoteMateriaPrima(lote));
            Assert.Equal("O fator de Pegada de Carbono não pode ser um número negativo.", excecao.Message);
        }

        [Fact]
        public async Task CriarLoteMateriaPrima_DataNoFuturo_DeveLancarArgumentException()
        {
            // Arrange
            var lote = new LoteMateriaPrima
            {
                QuantidadeKg = 100,
                PegadaCarbonoPorKg = 1.0,
                DataProducao = DateTime.UtcNow.AddDays(1) // A data de produção não pode estar no futuro[cite: 6]
            };

            // Act & Assert
            var excecao = await Assert.ThrowsAsync<ArgumentException>(() => _dadosService.CriarLoteMateriaPrima(lote));
            Assert.Equal("Violação Temporal: A data de produção do lote não pode estar no futuro.", excecao.Message);
        }

        #endregion

        #region Validações de VeiculoComponente e Fornecedor

        [Theory]
        [InlineData(0)]
        [InlineData(-5)]
        public async Task CriarVeiculoComponente_PesoInvalido_DeveLancarArgumentException(double pesoInvalido)
        {
            // Arrange
            var componente = new VeiculoComponente
            {
                PesoKg = pesoInvalido // O peso da peça deve ser maior que zero[cite: 6]
            };

            // Act & Assert
            var excecao = await Assert.ThrowsAsync<ArgumentException>(() => _dadosService.CriarVeiculoComponente(componente));
            Assert.Equal("O peso da peça deve ser maior que zero.", excecao.Message);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public async Task ExcluirFornecedor_IdVazioOuNulo_DeveLancarArgumentException(string idInvalido)
        {
            // Act & Assert
            var excecao = await Assert.ThrowsAsync<ArgumentException>(() => _dadosService.ExcluirFornecedor(idInvalido));

            // O id do fornecedor é validado antes da consulta de lotes ativos[cite: 6]
            Assert.Equal("O ID do fornecedor não pode ser nulo ou vazio.", excecao.Message);
        }

        [Theory]
        [InlineData("")]
        [InlineData(null)]
        public async Task ExcluirVeiculo_VinVazioOuNulo_DeveLancarArgumentException(string vinInvalido)
        {
            // Act & Assert
            var excecao = await Assert.ThrowsAsync<ArgumentException>(() => _dadosService.ExcluirVeiculo(vinInvalido));

            // O VIN deve possuir um valor válido[cite: 6]
            Assert.Equal("O VIN não pode ser nulo ou vazio.", excecao.Message);
        }

        #endregion
    }
}