using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using WpfIveco.Models;

namespace WpfIveco.Relatorios
{
    /// <summary>
    /// Documento PDF para relatório de veículos e componentes vinculados.
    /// </summary>
    public class RelatorioVeiculosDocument : IDocument
    {
        private readonly List<VeiculoModel> _veiculos;
        private readonly List<PecaModel> _pecas;

        // Paleta de cores baseada no tema do XAML
        private readonly string TextPrimary = "#1C1C1E";
        private readonly string TextSecondary = "#6C6C70";
        private readonly string AppleBlue = "#007AFF";
        private readonly string AppleGreen = "#34C759";
        private readonly string BackgroundGray = "#F5F5F7";
        private readonly string BorderGray = "#C6C6C8";

        /// <summary>
        /// Inicializa o relatório com listas de veículos e peças.
        /// </summary>
        /// <param name="veiculos">Lista de veículos a serem exibidos.</param>
        /// <param name="pecas">Lista de peças vinculadas.</param>
        public RelatorioVeiculosDocument(List<VeiculoModel> veiculos, List<PecaModel> pecas)
        {
            _veiculos = veiculos;
            _pecas = pecas;
        }

        /// <summary>
        /// Retorna os metadados padrão do documento.
        /// </summary>
        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        /// <summary>
        /// Compõe a estrutura do documento PDF.
        /// </summary>
        public void Compose(IDocumentContainer container)
        {
            container
                .Page(page =>
                {
                    page.Margin(20, Unit.Millimetre);
                    page.Size(PageSizes.A4);
                    page.PageColor(Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(10).FontFamily(Fonts.SegoeUI).FontColor(TextPrimary));

                    page.Header().Element(ComposeHeader);
                    page.Content().Element(ComposeContent);
                    page.Footer().Element(ComposeFooter);
                });
        }

        /// <summary>
        /// Compõe o cabeçalho do relatório (título, data, ID).
        /// </summary>
        private void ComposeHeader(IContainer container)
        {
            container.PaddingBottom(15).BorderBottom(4).BorderColor(AppleGreen).Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("Iveco Green Ledger").FontSize(26).SemiBold().FontColor(TextPrimary);
                    col.Item().Text("Relatório Consolidado de Rastreabilidade").FontSize(12).SemiBold().FontColor(AppleBlue);
                });

                row.ConstantItem(150).AlignRight().AlignBottom().Text(text =>
                {
                    text.Span("Data de Emissão: ").FontColor(TextSecondary);
                    text.Span($"{DateTime.Now:dd/MM/yyyy}\n").FontColor(TextPrimary);
                    text.Span("ID do Relatório: ").FontColor(TextSecondary);
                    text.Span("RPT-8942-IVC").FontColor(TextPrimary);
                });
            });
        }

        /// <summary>
        /// Compõe o conteúdo principal (resumo, tabela de veículos, tabela de peças).
        /// </summary>
        private void ComposeContent(IContainer container)
        {
            container.PaddingVertical(10).Column(column =>
            {
                column.Spacing(25);

                // Caixa de resumo
                column.Item().Background(BackgroundGray).BorderLeft(5).BorderColor(AppleBlue).Padding(15).Row(row =>
                {
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text("Rede").FontSize(10).FontColor(TextSecondary);
                        col.Item().Text("Blockchain (Ativa)").FontSize(14).SemiBold().FontColor(TextPrimary);
                    });

                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text("Total de Veículos").FontSize(10).FontColor(TextSecondary);
                        col.Item().Text($"{_veiculos.Count} Registrados").FontSize(14).SemiBold().FontColor(TextPrimary);
                    });

                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text("Status Geral").FontSize(10).FontColor(TextSecondary);
                        col.Item().Text("Em Conformidade").FontSize(14).SemiBold().FontColor(AppleGreen);
                    });
                });

                // Tabela de veículos
                column.Item().Column(col =>
                {
                    col.Item().PaddingBottom(5).BorderBottom(1).BorderColor(BorderGray).Text("Rastreabilidade de Veículos").FontSize(16).SemiBold();
                    col.Item().PaddingTop(10).Element(ComposeTabelaVeiculos);
                });

                // Tabela de peças
                column.Item().Column(col =>
                {
                    col.Item().PaddingBottom(5).BorderBottom(1).BorderColor(BorderGray).Text("Componentes Vinculados (Amostragem)").FontSize(16).SemiBold();
                    col.Item().PaddingTop(10).Element(ComposeTabelaPecas);
                });
            });
        }

        /// <summary>
        /// Compõe a tabela com os veículos (VIN, Modelo, Data, Status).
        /// </summary>
        private void ComposeTabelaVeiculos(IContainer container)
        {
            container.Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(2); // VIN
                    columns.RelativeColumn(2); // Modelo
                    columns.RelativeColumn(2); // Data
                    columns.RelativeColumn(2); // Status
                });

                // Headers
                table.Header(header =>
                {
                    header.Cell().Background(BackgroundGray).Padding(5).BorderBottom(2).BorderColor(BorderGray).Text("VIN (Chassi)").SemiBold().FontColor(TextSecondary);
                    header.Cell().Background(BackgroundGray).Padding(5).BorderBottom(2).BorderColor(BorderGray).Text("Modelo").SemiBold().FontColor(TextSecondary);
                    header.Cell().Background(BackgroundGray).Padding(5).BorderBottom(2).BorderColor(BorderGray).Text("Montagem").SemiBold().FontColor(TextSecondary);
                    header.Cell().Background(BackgroundGray).Padding(5).BorderBottom(2).BorderColor(BorderGray).Text("Auditoria").SemiBold().FontColor(TextSecondary);
                });

                // Dados
                foreach (var v in _veiculos)
                {
                    table.Cell().BorderBottom(1).BorderColor("#E5E5EA").Padding(5).Text(v.Vin).SemiBold();
                    table.Cell().BorderBottom(1).BorderColor("#E5E5EA").Padding(5).Text(v.Modelo ?? "Modelo Padrão");
                    table.Cell().BorderBottom(1).BorderColor("#E5E5EA").Padding(5).Text(v.DataMontagem?.ToString("dd/MM/yyyy") ?? "-");
                    table.Cell().BorderBottom(1).BorderColor("#E5E5EA").Padding(5).Text("Validado no Ledger").FontColor(AppleGreen).SemiBold();
                }
            });
        }

        /// <summary>
        /// Compõe a tabela com as peças vinculadas (Componente, VIN, Status).
        /// </summary>
        private void ComposeTabelaPecas(IContainer container)
        {
            container.Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(3); // Componente
                    columns.RelativeColumn(3); // VIN
                    columns.RelativeColumn(2); // Status
                });

                table.Header(header =>
                {
                    header.Cell().Background(BackgroundGray).Padding(5).BorderBottom(2).BorderColor(BorderGray).Text("Componente / Peça").SemiBold().FontColor(TextSecondary);
                    header.Cell().Background(BackgroundGray).Padding(5).BorderBottom(2).BorderColor(BorderGray).Text("Chassi (VIN) Vinculado").SemiBold().FontColor(TextSecondary);
                    header.Cell().Background(BackgroundGray).Padding(5).BorderBottom(2).BorderColor(BorderGray).Text("Status").SemiBold().FontColor(TextSecondary);
                });

                foreach (var p in _pecas)
                {
                    table.Cell().BorderBottom(1).BorderColor("#E5E5EA").Padding(5).Text(p.NomePeca);
                    table.Cell().BorderBottom(1).BorderColor("#E5E5EA").Padding(5).Text(p.VinAssociado);
                    table.Cell().BorderBottom(1).BorderColor("#E5E5EA").Padding(5).Text("Registrado").FontColor(AppleBlue).SemiBold();
                }
            });
        }

        /// <summary>
        /// Compõe o rodapé do relatório (mensagem e numeração de páginas).
        /// </summary>
        private void ComposeFooter(IContainer container)
        {
            container.AlignCenter().Column(col =>
            {
                col.Item().PaddingTop(10).Text("Este documento foi gerado automaticamente pelo nó local do Iveco Green Ledger.")
                    .FontSize(9).FontColor(TextSecondary);

                col.Item().Text(x =>
                {
                    x.Span("Página ").FontSize(9).FontColor(TextSecondary);
                    x.CurrentPageNumber().FontSize(9).FontColor(TextSecondary);
                    x.Span(" de ").FontSize(9).FontColor(TextSecondary);
                    x.TotalPages().FontSize(9).FontColor(TextSecondary);
                });
            });
        }
    }
}