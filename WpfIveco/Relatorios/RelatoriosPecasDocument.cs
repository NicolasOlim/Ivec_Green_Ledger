using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using WpfIveco.DTO;
using WpfIveco.Models;

namespace WpfIveco.Relatorios
{
    /// <summary>
    /// Documento PDF para relatório de peças e componentes.
    /// CORREÇÃO: Exibe mensagem "Nenhuma peça encontrada" se a lista estiver vazia.
    /// </summary>
    public class RelatorioPecasDocument : IDocument
    {
        // ============================================================
        // CAMPOS PRIVADOS
        // ============================================================

        private readonly List<VeiculoComponenteApiDto> _pecas;

        // Paleta de cores
        private readonly string TextPrimary = "#1C1C1E";
        private readonly string TextSecondary = "#6C6C70";
        private readonly string AppleBlue = "#007AFF";
        private readonly string AppleGreen = "#34C759";
        private readonly string BackgroundGray = "#F5F5F7";
        private readonly string BorderGray = "#C6C6C8";

        // ============================================================
        // CONSTRUTOR
        // ============================================================

        /// <summary>
        /// Inicializa o relatório com a lista de peças.
        /// </summary>
        /// <param name="pecas">Lista de peças (DTO da API) a serem exibidas.</param>
        public RelatorioPecasDocument(List<VeiculoComponenteApiDto> pecas)
        {
            _pecas = pecas ?? new List<VeiculoComponenteApiDto>();
        }

        // ============================================================
        // MÉTODOS OBRIGATÓRIOS (IDocument)
        // ============================================================

        public DocumentMetadata GetMetadata() => DocumentMetadata.Default;

        public void Compose(IDocumentContainer container)
        {
            container.Page(page =>
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

        // ============================================================
        // COMPOSIÇÃO DO CABEÇALHO
        // ============================================================

        private void ComposeHeader(IContainer container)
        {
            container.PaddingBottom(15).BorderBottom(4).BorderColor(AppleGreen).Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("Iveco Green Ledger").FontSize(26).SemiBold().FontColor(TextPrimary);
                    col.Item().Text("Relatório de Componentes e Peças").FontSize(12).SemiBold().FontColor(AppleBlue);
                });

                row.ConstantItem(150).AlignRight().AlignBottom().Text(text =>
                {
                    text.Span("Data de Emissão: ").FontColor(TextSecondary);
                    text.Span($"{DateTime.Now:dd/MM/yyyy}\n").FontColor(TextPrimary);
                    text.Span("ID do Relatório: ").FontColor(TextSecondary);
                    text.Span("RPT-PECA-001").FontColor(TextPrimary);
                });
            });
        }

        // ============================================================
        // COMPOSIÇÃO DO CONTEÚDO
        // ============================================================

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
                        col.Item().Text("Total de Peças").FontSize(10).FontColor(TextSecondary);
                        col.Item().Text($"{_pecas.Count} Registradas").FontSize(14).SemiBold().FontColor(TextPrimary);
                    });

                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text("Status Geral").FontSize(10).FontColor(TextSecondary);
                        col.Item().Text(_pecas.Count > 0 ? "Vinculadas" : "Sem dados").FontSize(14).SemiBold()
                            .FontColor(_pecas.Count > 0 ? AppleGreen : TextSecondary);
                    });
                });

                // Tabela de peças
                column.Item().Column(col =>
                {
                    col.Item().PaddingBottom(5).BorderBottom(1).BorderColor(BorderGray)
                        .Text("Inventário de Componentes").FontSize(16).SemiBold();
                    col.Item().PaddingTop(10).Element(ComposeTabelaPecas);
                });
            });
        }

        // ============================================================
        // TABELA DE PEÇAS
        // ============================================================

        private void ComposeTabelaPecas(IContainer container)
        {
            container.Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(3); // Nome da Peça
                    columns.RelativeColumn(2); // VIN Associado
                    columns.RelativeColumn(1); // Peso (kg)
                    columns.RelativeColumn(2); // Fornecedor
                });

                // Headers
                table.Header(header =>
                {
                    header.Cell().Background(BackgroundGray).Padding(5).BorderBottom(2).BorderColor(BorderGray)
                        .Text("Nome / Descrição").SemiBold().FontColor(TextSecondary);
                    header.Cell().Background(BackgroundGray).Padding(5).BorderBottom(2).BorderColor(BorderGray)
                        .Text("VIN (Chassi)").SemiBold().FontColor(TextSecondary);
                    header.Cell().Background(BackgroundGray).Padding(5).BorderBottom(2).BorderColor(BorderGray)
                        .Text("Peso (kg)").SemiBold().FontColor(TextSecondary);
                    header.Cell().Background(BackgroundGray).Padding(5).BorderBottom(2).BorderColor(BorderGray)
                        .Text("Fornecedor").SemiBold().FontColor(TextSecondary);
                });

                // CORREÇÃO: Se não houver dados, exibe uma linha com mensagem
                if (_pecas.Count == 0)
                {
                    table.Cell().ColumnSpan(4).Padding(10).AlignCenter()
                        .Text("Nenhuma peça ou componente encontrado no sistema.").FontColor(TextSecondary).Italic();
                }
                else
                {
                    foreach (var p in _pecas)
                    {
                        table.Cell().BorderBottom(1).BorderColor("#E5E5EA").Padding(5).Text(p.NomePeca ?? "N/A").SemiBold();
                        table.Cell().BorderBottom(1).BorderColor("#E5E5EA").Padding(5).Text(p.Fk_Veiculo_Vin ?? "N/A");
                        table.Cell().BorderBottom(1).BorderColor("#E5E5EA").Padding(5).Text($"{p.PesoKg:F2}");
                        table.Cell().BorderBottom(1).BorderColor("#E5E5EA").Padding(5)
                            .Text(!string.IsNullOrEmpty(p.Fk_Fornecedor_Id) ? p.Fk_Fornecedor_Id : "N/A")
                            .FontColor(AppleBlue);
                    }
                }
            });
        }

        // ============================================================
        // RODAPÉ
        // ============================================================

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