using Microsoft.Win32;
using QuestPDF.Fluent;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using WpfIveco.Models;
using WpfIveco.Relatorios;
using WpfIveco.ViewModels;

namespace WpfIveco.ViewModel
{
    /// <summary>
    /// ViewModel para a geração de relatórios PDF.
    /// Gerencia a seleção do tipo de relatório e a exportação de dados.
    /// </summary>
    public class RelatoriosViewModel : ViewModelBase
    {
        private readonly HttpClient _httpClient;

        // ============================================================
        // PROPRIEDADES
        // ============================================================

        private string _tipoRelatorio = "Veiculos";
        /// <summary>Tipo de relatório selecionado (Veiculos, Fornecedores, Pecas).</summary>
        public string TipoRelatorio
        {
            get => _tipoRelatorio;
            set { _tipoRelatorio = value; OnPropertyChanged(); }
        }

        private bool _isGerandoPdf = false;
        /// <summary>Indica se um PDF está sendo gerado (para bloquear múltiplas gerações).</summary>
        public bool IsGerandoPdf
        {
            get => _isGerandoPdf;
            set { _isGerandoPdf = value; OnPropertyChanged(); }
        }

        // ============================================================
        // COMANDOS
        // ============================================================

        /// <summary>Comando para gerar o relatório PDF.</summary>
        public ICommand GerarRelatorioPdfCommand { get; }

        /// <summary>Comando para alterar o tipo de relatório.</summary>
        public ICommand MudarTipoRelatorioCommand { get; }

        // ============================================================
        // CONSTRUTOR
        // ============================================================

        /// <summary>Inicializa o ViewModel com o HttpClient.</summary>
        public RelatoriosViewModel(HttpClient httpClient)
        {
            App.LogInfo("Construtor", "RELAT");
            _httpClient = httpClient;
            GerarRelatorioPdfCommand = new RelayCommand(async p => await BaixarRelatorioPdfAsync());
            MudarTipoRelatorioCommand = new RelayCommand(p => TipoRelatorio = p as string ?? "Veiculos");
        }

        // ============================================================
        // MÉTODO PÚBLICO
        // ============================================================

        /// <summary>
        /// Gera e baixa o relatório PDF.
        /// Obtém os dados da API, exibe um diálogo para salvar e gera o PDF.
        /// </summary>
        public async Task BaixarRelatorioPdfAsync()
        {
            App.LogInfo($"Gerando relatório PDF: {TipoRelatorio}", "RELAT");
            if (IsGerandoPdf)
            {
                App.LogWarning("Já está gerando um PDF", "RELAT");
                return;
            }

            IsGerandoPdf = true;

            try
            {
                var urlDados = "api/dados/veiculos";
                var response = await _httpClient.GetAsync(urlDados);
                App.LogInfo($"GET {urlDados} → {(int)response.StatusCode}", "RELAT");

                if (response.IsSuccessStatusCode)
                {
                    var json = await response.Content.ReadAsStringAsync();
                    var listaVeiculos = JsonSerializer.Deserialize<List<VeiculoModel>>(json,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<VeiculoModel>();

                    var listaPecas = new List<PecaModel>();

                    var saveDialog = new SaveFileDialog
                    {
                        Filter = "Ficheiro PDF (*.pdf)|*.pdf",
                        FileName = $"Relatorio_{TipoRelatorio}_Iveco_{DateTime.Now:yyyyMMdd}.pdf",
                        Title = "Guardar Relatório"
                    };

                    if (saveDialog.ShowDialog() == true)
                    {
                        var relatorio = new RelatorioVeiculosDocument(listaVeiculos, listaPecas);
                        relatorio.GeneratePdf(saveDialog.FileName);
                        App.LogInfo($"PDF gerado: {saveDialog.FileName}", "RELAT");
                        MessageBox.Show("Relatório gerado e guardado com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);

                        Process.Start(new ProcessStartInfo
                        {
                            FileName = saveDialog.FileName,
                            UseShellExecute = true
                        });
                    }
                }
                else
                {
                    var erro = await response.Content.ReadAsStringAsync();
                    App.LogError($"Falha ao obter dados: {erro}", "RELAT");
                    MessageBox.Show("Não foi possível obter os dados para o relatório.\nTente novamente.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch
            {
                App.LogError("Erro ao gerar relatório PDF", "RELAT");
                MessageBox.Show("Ocorreu um erro inesperado.\nTente novamente ou contacte o suporte.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsGerandoPdf = false;
            }
        }
    }
}