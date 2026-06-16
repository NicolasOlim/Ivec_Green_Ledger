using Microsoft.Win32;
using QuestPDF.Fluent;  
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Text.Json; 
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using WpfIveco.ViewModels;
using WpfIveco.Models;
using WpfIveco.Relatorios;

namespace WpfIveco.ViewModel
{
    public class RelatoriosViewModel : ViewModelBase
    {
        private readonly HttpClient _httpClient;

        
        /// PROPRIEDADES
       
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

        
        /// COMANDOS
        
        public ICommand GerarRelatorioPdfCommand { get; }
        public ICommand MudarTipoRelatorioCommand { get; }

        
        /// <summary>
        /// CONSTRUTOR
        /// </summary>
        /// <param name="httpClient"></param>
        
        public RelatoriosViewModel(HttpClient httpClient)
        {
            _httpClient = httpClient;
            GerarRelatorioPdfCommand = new RelayCommand(async p => await BaixarRelatorioPdfAsync());
            MudarTipoRelatorioCommand = new RelayCommand(p => TipoRelatorio = p as string ?? "Veiculos");
        }


        /// <summary>
        /// DOWNLOAD DO PDF
        /// </summary>
        /// <returns></returns>

        public async Task BaixarRelatorioPdfAsync()
        {
            if (IsGerandoPdf)
                return;

            IsGerandoPdf = true;

            try
            {
                // 1. Opcional: Se os dados já estiverem guardados no ViewModel, 
                // ignore o HttpClient e use as suas listas locais. 
                // Caso contrário, peça os dados à API em formato JSON:
                var urlDados = "api/dados/veiculos"; // Endpoint que devolve a lista de veículos
                var response = await _httpClient.GetAsync(urlDados);

                if (response.IsSuccessStatusCode)
                {
                    // 2. Lê os dados e converte para as suas classes C#
                    var json = await response.Content.ReadAsStringAsync();
                    var listaVeiculos = JsonSerializer.Deserialize<List<VeiculoModel>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<VeiculoModel>();

                    // (Faça o mesmo para buscar a listaPecas, ou passe uma lista vazia se não precisar)
                    var listaPecas = new List<PecaModel>();

                    // 3. Abre o diálogo de guardar ficheiro
                    var saveDialog = new SaveFileDialog
                    {
                        Filter = "Ficheiro PDF (*.pdf)|*.pdf",
                        FileName = $"Relatorio_{TipoRelatorio}_Iveco_{DateTime.Now:yyyyMMdd}.pdf",
                        Title = "Guardar Relatório"
                    };

                    if (saveDialog.ShowDialog() == true)
                    {
                        // 4. GERA O PDF COM A NOVA CLASSE DE DESIGN E GUARDA O FICHEIRO
                        var relatorio = new RelatorioVeiculosDocument(listaVeiculos, listaPecas);
                        relatorio.GeneratePdf(saveDialog.FileName);

                        MessageBox.Show("Relatório gerado e guardado com sucesso!", "Sucesso",
                            MessageBoxButton.OK, MessageBoxImage.Information);

                        // Abre o PDF automaticamente no leitor padrão
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
                    Debug.WriteLine($"[ERRO OBTER DADOS] HTTP {(int)response.StatusCode} -> {erro}");
                    MessageBox.Show("Não foi possível obter os dados para o relatório.\nTente novamente.",
                        "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (HttpRequestException ex)
            {
                Debug.WriteLine($"[ERRO CONEXÃO PDF] {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}");
                MessageBox.Show("Não foi possível conectar ao servidor.\nVerifique a sua ligação.",
                    "Erro de Ligação", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[ERRO INESPERADO PDF] {ex.GetType().Name}: {ex.Message}\n{ex.StackTrace}");
                MessageBox.Show("Ocorreu um erro inesperado.\nTente novamente ou contacte o suporte.",
                    "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsGerandoPdf = false;
            }
        }
    }
}