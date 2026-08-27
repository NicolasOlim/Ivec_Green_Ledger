using Microsoft.Win32;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using WpfIveco.DTO;
using WpfIveco.Models;
using WpfIveco.Relatorios;
using WpfIveco.ViewModels;

namespace WpfIveco.ViewModel
{
    /// <summary>
    /// ViewModel para a geração de relatórios PDF.
    /// CORREÇÃO: Adicionados logs e validação de dados vazios.
    /// </summary>
    public class RelatoriosViewModel : ViewModelBase
    {
        // ============================================================
        // CAMPOS PRIVADOS
        // ============================================================

        private readonly HttpClient _httpClient;

        // ============================================================
        // PROPRIEDADES
        // ============================================================

        private string _tipoRelatorio = "Veiculos";
        public string TipoRelatorio
        {
            get => _tipoRelatorio;
            set { _tipoRelatorio = value; OnPropertyChanged(); }
        }

        private bool _isGerandoPdf = false;
        public bool IsGerandoPdf
        {
            get => _isGerandoPdf;
            set { _isGerandoPdf = value; OnPropertyChanged(); }
        }

        // ============================================================
        // COMANDOS
        // ============================================================

        public ICommand GerarRelatorioPdfCommand { get; }
        public ICommand MudarTipoRelatorioCommand { get; }

        // ============================================================
        // CONSTRUTOR
        // ============================================================

        public RelatoriosViewModel(HttpClient httpClient)
        {
            App.LogInfo("Construtor", "RELAT");
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            GerarRelatorioPdfCommand = new RelayCommand(async p => await BaixarRelatorioPdfAsync());
            MudarTipoRelatorioCommand = new RelayCommand(p => TipoRelatorio = p as string ?? "Veiculos");
        }

        // ============================================================
        // MÉTODO PRINCIPAL
        // ============================================================

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
                // 1. Define o endpoint conforme o tipo selecionado
                string endpoint = TipoRelatorio switch
                {
                    "Fornecedores" => "api/dados/fornecedores",
                    "Pecas" => "api/dados/componentes",
                    _ => "api/dados/veiculos"
                };

                App.LogInfo($"Chamando endpoint: {endpoint}", "RELAT");
                var response = await _httpClient.GetAsync(endpoint);
                App.LogInfo($"GET {endpoint} → {(int)response.StatusCode}", "RELAT");

                if (!response.IsSuccessStatusCode)
                {
                    var erro = await response.Content.ReadAsStringAsync();
                    App.LogError($"Falha ao obter dados: {erro}", "RELAT");
                    MessageBox.Show("Não foi possível obter os dados para o relatório.\nTente novamente.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var json = await response.Content.ReadAsStringAsync();
                App.LogInfo($"JSON recebido (primeiros 200 caracteres): {json.Substring(0, Math.Min(200, json.Length))}", "RELAT");

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                // 2. Desserializa e verifica se há dados
                IDocument documento = null;
                int quantidade = 0;

                switch (TipoRelatorio)
                {
                    case "Veiculos":
                        var veiculos = JsonSerializer.Deserialize<List<VeiculoModel>>(json, options) ?? new List<VeiculoModel>();
                        quantidade = veiculos.Count;
                        App.LogInfo($"Veículos obtidos: {quantidade}", "RELAT");
                        documento = new RelatorioVeiculosDocument(veiculos, new List<PecaModel>());
                        break;

                    case "Fornecedores":
                        var fornecedores = JsonSerializer.Deserialize<List<FornecedorModel>>(json, options) ?? new List<FornecedorModel>();
                        quantidade = fornecedores.Count;
                        App.LogInfo($"Fornecedores obtidos: {quantidade}", "RELAT");
                        documento = new RelatorioFornecedoresDocument(fornecedores);
                        break;

                    case "Pecas":
                        var pecas = JsonSerializer.Deserialize<List<VeiculoComponenteApiDto>>(json, options) ?? new List<VeiculoComponenteApiDto>();
                        quantidade = pecas.Count;
                        App.LogInfo($"Peças obtidas: {quantidade}", "RELAT");
                        documento = new RelatorioPecasDocument(pecas);
                        break;
                }

                if (documento == null)
                {
                    MessageBox.Show("Tipo de relatório inválido.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                // 3. Se não houver dados, avisa o usuário e permite continuar (gerará PDF com "Nenhum dado")
                if (quantidade == 0)
                {
                    var resultado = MessageBox.Show(
                        "Nenhum dado foi encontrado para o relatório selecionado.\nDeseja gerar o PDF mesmo assim (apenas com cabeçalhos)?",
                        "Aviso",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning);
                    if (resultado == MessageBoxResult.No)
                        return;
                }

                // 4. Diálogo para salvar o arquivo
                var saveDialog = new SaveFileDialog
                {
                    Filter = "Ficheiro PDF (*.pdf)|*.pdf",
                    FileName = $"Relatorio_{TipoRelatorio}_Iveco_{DateTime.Now:yyyyMMdd}.pdf",
                    Title = "Guardar Relatório"
                };

                if (saveDialog.ShowDialog() == true)
                {
                    documento.GeneratePdf(saveDialog.FileName);
                    App.LogInfo($"PDF gerado: {saveDialog.FileName}", "RELAT");
                    MessageBox.Show("Relatório gerado e guardado com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);

                    Process.Start(new ProcessStartInfo
                    {
                        FileName = saveDialog.FileName,
                        UseShellExecute = true
                    });
                }
            }
            catch (Exception ex)
            {
                App.LogError($"Erro ao gerar relatório PDF: {ex.Message}", "RELAT");
                MessageBox.Show($"Ocorreu um erro inesperado: {ex.Message}\nTente novamente ou contacte o suporte.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsGerandoPdf = false;
            }
        }
    }
}