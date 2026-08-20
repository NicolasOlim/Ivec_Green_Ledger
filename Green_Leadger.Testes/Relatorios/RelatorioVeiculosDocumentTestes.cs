using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using WpfIveco.Models;
using WpfIveco.Relatorios;
using Xunit;

namespace Iveco.Testes.Relatorios
{
    public class RelatorioVeiculosDocumentTestes
    {
        public RelatorioVeiculosDocumentTestes()
        {
            // Define a licença do QuestPDF para permitir a geração de PDF em testes
            QuestPDF.Settings.License = LicenseType.Community;
        }

        #region Testes de Construtor e Metadados

        [Fact]
        public void Construtor_DeveArmazenarListasDeVeiculosEPecas()
        {
            // Arrange
            var veiculos = new List<VeiculoModel>
            {
                new VeiculoModel { Vin = "ABC123", Modelo = "Daily", DataMontagem = DateTime.Now }
            };
            var pecas = new List<PecaModel>
            {
                new PecaModel { NomePeca = "Motor", VinAssociado = "ABC123", PesoKg = 150 }
            };

            // Act
            var documento = new RelatorioVeiculosDocument(veiculos, pecas);

            // Assert
            Assert.NotNull(documento);
            // Verifica se a geração do PDF funciona com essas listas
            var pdf = documento.GeneratePdf();
            Assert.NotNull(pdf);
            Assert.True(pdf.Length > 0);
        }

        [Fact]
        public void GetMetadata_DeveRetornarMetadataPadrao()
        {
            // Arrange
            var documento = new RelatorioVeiculosDocument(new List<VeiculoModel>(), new List<PecaModel>());

            // Act
            var metadata = documento.GetMetadata();

            // Assert
            Assert.Equal(DocumentMetadata.Default, metadata);
        }

        #endregion

        #region Testes de Geração de PDF

        [Fact]
        public void Compose_ComListasNaoVazias_DeveGerarPdfSemExcecao()
        {
            // Arrange
            var veiculos = new List<VeiculoModel>
            {
                new VeiculoModel { Vin = "ABC123", Modelo = "Daily", DataMontagem = DateTime.Now },
                new VeiculoModel { Vin = "DEF456", Modelo = "Eurocargo", DataMontagem = DateTime.Now.AddDays(-5) }
            };
            var pecas = new List<PecaModel>
            {
                new PecaModel { NomePeca = "Motor", VinAssociado = "ABC123", PesoKg = 150 },
                new PecaModel { NomePeca = "Caixa de Velocidade", VinAssociado = "DEF456", PesoKg = 80 }
            };
            var documento = new RelatorioVeiculosDocument(veiculos, pecas);

            // Act & Assert - Apenas chamar o método e garantir que não lance exceção
            var exception = Record.Exception(() => documento.GeneratePdf());
            Assert.Null(exception);
        }

        [Fact]
        public void Compose_ComListaDeVeiculosVazia_DeveGerarPdfSemExcecao()
        {
            // Arrange
            var veiculos = new List<VeiculoModel>();
            var pecas = new List<PecaModel>
            {
                new PecaModel { NomePeca = "Motor", VinAssociado = "ABC123", PesoKg = 150 }
            };
            var documento = new RelatorioVeiculosDocument(veiculos, pecas);

            var exception = Record.Exception(() => documento.GeneratePdf());
            Assert.Null(exception);
        }

        [Fact]
        public void Compose_ComListaDePecasVazia_DeveGerarPdfSemExcecao()
        {
            // Arrange
            var veiculos = new List<VeiculoModel>
            {
                new VeiculoModel { Vin = "ABC123", Modelo = "Daily", DataMontagem = DateTime.Now }
            };
            var pecas = new List<PecaModel>();
            var documento = new RelatorioVeiculosDocument(veiculos, pecas);

            var exception = Record.Exception(() => documento.GeneratePdf());
            Assert.Null(exception);
        }

        [Fact]
        public void Compose_ComListasVazias_DeveGerarPdfSemExcecao()
        {
            // Arrange
            var documento = new RelatorioVeiculosDocument(new List<VeiculoModel>(), new List<PecaModel>());

            var exception = Record.Exception(() => documento.GeneratePdf());
            Assert.Null(exception);
        }

        [Fact]
        public void GeneratePdf_DeveRetornarArrayDeBytesNaoVazio()
        {
            // Arrange
            var veiculos = new List<VeiculoModel>
            {
                new VeiculoModel { Vin = "ABC123", Modelo = "Daily", DataMontagem = DateTime.Now }
            };
            var pecas = new List<PecaModel>();
            var documento = new RelatorioVeiculosDocument(veiculos, pecas);

            // Act
            byte[] pdfBytes = documento.GeneratePdf();

            // Assert
            Assert.NotNull(pdfBytes);
            Assert.True(pdfBytes.Length > 0);
        }

        [Fact]
        public void GeneratePdf_ComDadosReais_DeveGerarPdfComConteudoEsperado()
        {
            // Arrange
            var veiculos = new List<VeiculoModel>
            {
                new VeiculoModel { Vin = "ZCFA1E02008123456", Modelo = "Daily 50C14", DataMontagem = new DateTime(2024, 1, 15) }
            };
            var pecas = new List<PecaModel>
            {
                new PecaModel { NomePeca = "Bloco do Motor", VinAssociado = "ZCFA1E02008123456", PesoKg = 350 }
            };
            var documento = new RelatorioVeiculosDocument(veiculos, pecas);

            // Act
            byte[] pdfBytes = documento.GeneratePdf();

            // Assert - Verifica se o PDF contém algo (não validamos o conteúdo textual, mas podemos verificar o tamanho mínimo)
            Assert.NotNull(pdfBytes);
            // Um PDF simples costuma ter mais de 1 KB
            Assert.True(pdfBytes.Length > 1024, "O PDF gerado parece estar muito pequeno; pode estar vazio.");
        }

        #endregion

        #region Testes de Cenários com Dados Nulos

        [Fact]
        public void Construtor_ComListasNulas_DeveLancarExcecao()
        {
            // Arrange
            var documento = new RelatorioVeiculosDocument(null, null);

            // Act & Assert - Espera NullReferenceException ao tentar gerar o PDF
            Assert.Throws<NullReferenceException>(() => documento.GeneratePdf());
        }

        [Fact]
        public void Construtor_ComListaDeVeiculosNula_DeveLancarExcecaoAoGerarPdf()
        {
            var documento = new RelatorioVeiculosDocument(null, new List<PecaModel>());
            Assert.Throws<NullReferenceException>(() => documento.GeneratePdf());
        }

        [Fact]
        public void Construtor_ComListaDePecasNula_DeveLancarExcecaoAoGerarPdf()
        {
            var documento = new RelatorioVeiculosDocument(new List<VeiculoModel>(), null);
            Assert.Throws<NullReferenceException>(() => documento.GeneratePdf());
        }

        #endregion
    }
}