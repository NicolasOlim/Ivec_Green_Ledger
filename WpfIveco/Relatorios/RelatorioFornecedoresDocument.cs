using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using WpfIveco.Models;

namespace WpfIveco.Relatorios
{
    /// <summary>
    /// Documento PDF para relatório de fornecedores.
    /// CORREÇÃO: Exibe mensagem "Nenhum fornecedor encontrado" se a lista estiver vazia.
    /// </summary>
    public class RelatorioFornecedoresDocument : IDocument
    {
        // ============================================================
        // CAMPOS PRIVADOS
        // ============================================================

        private readonly List<FornecedorModel> _fornecedores;

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
        /// Inicializa o relatório com a lista de fornecedores.
        /// </summary>
        /// <param name="fornecedores">Lista de fornecedores a serem exibidos.</param>
        public RelatorioFornecedoresDocument(List<FornecedorModel> fornecedores)
        {
            _fornecedores = fornecedores ?? new List<FornecedorModel>();
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
                    col.Item().Text("Relatório de Fornecedores").FontSize(12).SemiBold().FontColor(AppleBlue);
                });

                row.ConstantItem(150).AlignRight().AlignBottom().Text(text =>
                {
                    text.Span("Data de Emissão: ").FontColor(TextSecondary);
                    text.Span($"{DateTime.Now:dd/MM/yyyy}\n").FontColor(TextPrimary);
                    text.Span("ID do Relatório: ").FontColor(TextSecondary);
                    text.Span("RPT-FORN-001").FontColor(TextPrimary);
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
                        col.Item().Text("Total de Fornecedores").FontSize(10).FontColor(TextSecondary);
                        col.Item().Text($"{_fornecedores.Count} Registrados").FontSize(14).SemiBold().FontColor(TextPrimary);
                    });

                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text("Status Geral").FontSize(10).FontColor(TextSecondary);
                        col.Item().Text(_fornecedores.Count > 0 ? "Ativo" : "Sem dados").FontSize(14).SemiBold()
                            .FontColor(_fornecedores.Count > 0 ? AppleGreen : TextSecondary);
                    });
                });

                // Tabela de fornecedores
                column.Item().Column(col =>
                {
                    col.Item().PaddingBottom(5).BorderBottom(1).BorderColor(BorderGray)
                        .Text("Lista de Fornecedores Cadastrados").FontSize(16).SemiBold();
                    col.Item().PaddingTop(10).Element(ComposeTabelaFornecedores);
                });
            });
        }

        // ============================================================
        // TABELA DE FORNECEDORES
        // ============================================================

        private void ComposeTabelaFornecedores(IContainer container)
        {
            container.Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(2); // Nome
                    columns.RelativeColumn(2); // CNPJ
                    columns.RelativeColumn(2); // Localização
                    columns.RelativeColumn(1); // Categoria ESG
                });

                // Headers
                table.Header(header =>
                {
                    header.Cell().Background(BackgroundGray).Padding(5).BorderBottom(2).BorderColor(BorderGray)
                        .Text("Nome / Razão Social").SemiBold().FontColor(TextSecondary);
                    header.Cell().Background(BackgroundGray).Padding(5).BorderBottom(2).BorderColor(BorderGray)
                        .Text("CNPJ").SemiBold().FontColor(TextSecondary);
                    header.Cell().Background(BackgroundGray).Padding(5).BorderBottom(2).BorderColor(BorderGray)
                        .Text("Localização").SemiBold().FontColor(TextSecondary);
                    header.Cell().Background(BackgroundGray).Padding(5).BorderBottom(2).BorderColor(BorderGray)
                        .Text("Categoria ESG").SemiBold().FontColor(TextSecondary);
                });

                // CORREÇÃO: Se não houver dados, exibe uma linha com mensagem
                if (_fornecedores.Count == 0)
                {
                    table.Cell().ColumnSpan(4).Padding(10).AlignCenter()
                        .Text("Nenhum fornecedor encontrado no sistema.").FontColor(TextSecondary).Italic();
                }
                else
                {
                    foreach (var f in _fornecedores)
                    {
                        table.Cell().BorderBottom(1).BorderColor("#E5E5EA").Padding(5).Text(f.Nome ?? "N/A").SemiBold();
                        table.Cell().BorderBottom(1).BorderColor("#E5E5EA").Padding(5).Text(f.Cnpj ?? "N/A");
                        table.Cell().BorderBottom(1).BorderColor("#E5E5EA").Padding(5).Text(f.Localizacao ?? "N/A");
                        table.Cell().BorderBottom(1).BorderColor("#E5E5EA").Padding(5).Text(f.CategoriaEsg ?? "Não avaliado")
                            .FontColor(AppleBlue).SemiBold();
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