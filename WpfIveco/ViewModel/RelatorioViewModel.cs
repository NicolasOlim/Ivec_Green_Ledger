using Microsoft.Win32;
using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using WpfIveco.ViewModels;

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
                /// Monta a URL com base no tipo selecionado
                var urlPdf = TipoRelatorio switch
                {
                    "Veiculos" => "api/dados/relatorios/veiculos/pdf",
                    _ => "api/dados/relatorios/veiculos/pdf"
                };

                var response = await _httpClient.GetAsync(urlPdf);

                if (response.IsSuccessStatusCode)
                {
                    byte[] fileBytes = await response.Content.ReadAsByteArrayAsync();

                    ///Abre o diálogo de guardar ficheiro
                    var saveDialog = new SaveFileDialog
                    {
                        Filter = "Ficheiro PDF (*.pdf)|*.pdf",
                        FileName = $"Relatorio_{TipoRelatorio}_Iveco_{DateTime.Now:yyyyMMdd}.pdf",
                        Title = "Guardar Relatório"
                    };

                    if (saveDialog.ShowDialog() == true)
                    {
                        File.WriteAllBytes(saveDialog.FileName, fileBytes);

                        MessageBox.Show("Relatório gerado e guardado com sucesso!", "Sucesso",
                            MessageBoxButton.OK, MessageBoxImage.Information);

                        /// Abre o PDF automaticamente no leitor padrão
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
                    Debug.WriteLine($"[ERRO GERAR PDF] HTTP {(int)response.StatusCode} -> {erro}");
                    MessageBox.Show("Não foi possível gerar o relatório.\nTente novamente.",
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