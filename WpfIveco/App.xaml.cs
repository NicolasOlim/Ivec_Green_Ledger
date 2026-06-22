using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;
using WpfIveco.DTO;

namespace WpfIveco
{
    public partial class App : Application
    {
        private static readonly string LogFilePath;
        private static readonly string FallbackLogFilePath;
        private static readonly object LogLock = new object();

        static App()
        {
            // Tenta obter o caminho principal (dentro do projeto)
            LogFilePath = ObterCaminhoLogPrincipal();
            // Fallback: pasta do executável
            FallbackLogFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs.json");

            // Cria a pasta principal se necessário
            try
            {
                string dir = Path.GetDirectoryName(LogFilePath);
                if (!string.IsNullOrEmpty(dir))
                {
                    if (!Directory.Exists(dir))
                    {
                        Directory.CreateDirectory(dir);
                        Debug.WriteLine($"[LOGGER] Pasta de logs criada: {dir}");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[LOGGER] Falha ao criar pasta principal: {ex.Message}");
                // Se falhar, tenta usar o fallback
                try
                {
                    string fallbackDir = Path.GetDirectoryName(FallbackLogFilePath);
                    if (!string.IsNullOrEmpty(fallbackDir) && !Directory.Exists(fallbackDir))
                    {
                        Directory.CreateDirectory(fallbackDir);
                        Debug.WriteLine($"[LOGGER] Pasta fallback criada: {fallbackDir}");
                    }
                }
                catch
                {
                    Debug.WriteLine("[LOGGER] Falha ao criar pasta fallback.");
                }
            }

            Debug.WriteLine($"[LOGGER] Arquivo de log principal: {LogFilePath}");
            Debug.WriteLine($"[LOGGER] Arquivo de log fallback: {FallbackLogFilePath}");

            // Exibe o caminho para o usuário (opcional)
            MessageBox.Show($"Logs serão salvos em:\n{LogFilePath}", "Localização dos Logs", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        /// <summary>
        /// Tenta encontrar a raiz do projeto WPF para salvar os logs.
        /// </summary>
        private static string ObterCaminhoLogPrincipal()
        {
            // 1. Tenta usar o diretório do executável e subir até encontrar a raiz do projeto
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string projectDir = baseDir;

            for (int i = 0; i < 4; i++) // sobe até 4 níveis (ex: bin\Debug\net8.0-windows)
            {
                var parent = Directory.GetParent(projectDir);
                if (parent == null) break;
                projectDir = parent.FullName;
            }

            // Verifica se encontrou a raiz do projeto (procura pelo .csproj)
            if (File.Exists(Path.Combine(projectDir, "WpfIveco.csproj")))
            {
                return Path.Combine(projectDir, "Logs", "logs.json");
            }

            // 2. Se não encontrou, tenta usar o diretório atual (pode ser a pasta do executável)
            return Path.Combine(baseDir, "Logs", "logs.json");
        }

        // ============================================================
        // MÉTODOS PÚBLICOS DE LOG
        // ============================================================

        public static void LogInfo(string message, string source = null)
        {
            WriteLog("INFO", message, source);
        }

        public static void LogWarning(string message, string source = null)
        {
            WriteLog("WARN", message, source);
        }

        public static void LogError(string message, string source = null)
        {
            WriteLog("ERRO", message, source);
        }

        public static void LogException(string message, string source = null)
        {
            WriteLog("EXCP", message, source);
        }

        [Conditional("DEBUG")]
        public static void LogDebug(string message, string source = null)
        {
            WriteLog("DBUG", message, source);
        }

        public static string GetLogFilePath() => LogFilePath;

        // ============================================================
        // ESCRITA DO LOG (COM FALLBACK)
        // ============================================================

        private static void WriteLog(string level, string message, string source)
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            string sourceTag = string.IsNullOrEmpty(source) ? "APP" : source.ToUpper();

            Debug.WriteLine($"[{timestamp}] [{level}] [{sourceTag}] {message}");

            // Tenta escrever no arquivo principal
            bool escrito = false;
            try
            {
                lock (LogLock)
                {
                    string dir = Path.GetDirectoryName(LogFilePath);
                    if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                        Directory.CreateDirectory(dir);

                    var logEntry = new LogEntryDto
                    {
                        Timestamp = timestamp,
                        Level = level,
                        Source = sourceTag,
                        Message = message,
                        ThreadId = Thread.CurrentThread.ManagedThreadId
                    };
                    string jsonLine = JsonSerializer.Serialize(logEntry);
                    File.AppendAllText(LogFilePath, jsonLine + Environment.NewLine);
                    escrito = true;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[LOGGER] Falha ao escrever no principal: {ex.Message}");
                // Tenta o fallback
                try
                {
                    lock (LogLock)
                    {
                        string fallbackDir = Path.GetDirectoryName(FallbackLogFilePath);
                        if (!string.IsNullOrEmpty(fallbackDir) && !Directory.Exists(fallbackDir))
                            Directory.CreateDirectory(fallbackDir);

                        var logEntry = new LogEntryDto
                        {
                            Timestamp = timestamp,
                            Level = level,
                            Source = sourceTag,
                            Message = message,
                            ThreadId = Thread.CurrentThread.ManagedThreadId
                        };
                        string jsonLine = JsonSerializer.Serialize(logEntry);
                        File.AppendAllText(FallbackLogFilePath, jsonLine + Environment.NewLine);
                        escrito = true;
                    }
                }
                catch (Exception ex2)
                {
                    Debug.WriteLine($"[LOGGER] Falha também no fallback: {ex2.Message}");
                }
            }

            if (!escrito)
                Debug.WriteLine("[LOGGER] Log não persistido (ambos os destinos falharam).");

            // Tenta truncar
            if (escrito)
            {
                try { TruncateFile(LogFilePath); }
                catch { try { TruncateFile(FallbackLogFilePath); } catch { } }
            }
        }

        private static void TruncateFile(string filePath)
        {
            if (!File.Exists(filePath))
                return;
            var lines = File.ReadAllLines(filePath);
            if (lines.Length > 10000)
            {
                var keep = new List<string>(lines);
                keep.RemoveRange(0, keep.Count - 5000);
                File.WriteAllLines(filePath, keep);
            }
        }

        // ============================================================
        // LEITURA DE LOGS (COM TRATAMENTO DE ERROS)
        // ============================================================

        public static List<LogEntryDto> ReadLogs()
        {
            var logs = new List<LogEntryDto>();

            if (File.Exists(LogFilePath))
            {
                try { logs.AddRange(ReadLogFile(LogFilePath)); }
                catch { Debug.WriteLine("[LOGGER] Falha ao ler principal."); }
            }
            else if (File.Exists(FallbackLogFilePath))
            {
                try { logs.AddRange(ReadLogFile(FallbackLogFilePath)); }
                catch { Debug.WriteLine("[LOGGER] Falha ao ler fallback."); }
            }
            else
            {
                Debug.WriteLine("[LOGGER] Nenhum arquivo de log encontrado.");
            }

            return logs;
        }

        private static List<LogEntryDto> ReadLogFile(string path)
        {
            var result = new List<LogEntryDto>();
            if (!File.Exists(path)) return result;

            var lines = File.ReadAllLines(path);
            foreach (var line in lines)
            {
                try
                {
                    var entry = JsonSerializer.Deserialize<LogEntryDto>(line);
                    if (entry != null)
                        result.Add(entry);
                }
                catch
                {
                    Debug.WriteLine($"[LOGGER] Linha corrompida ignorada: {line.Substring(0, Math.Min(50, line.Length))}...");
                }
            }
            return result;
        }

        // ============================================================
        // MANIPULADORES DE EXCEÇÃO GLOBAL
        // ============================================================

        public App()
        {
            this.DispatcherUnhandledException += (s, e) =>
            {
                LogException($"Exceção não tratada na UI: {e.Exception.GetType().Name}", "UI");
                e.Handled = true;
                MessageBox.Show(
                    "Ocorreu um erro inesperado na interface.\nO sistema tentará continuar.\n\n" +
                    "Se o problema persistir, contacte o suporte.",
                    "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            };

            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                if (e.ExceptionObject is Exception ex)
                    LogException($"Exceção não tratada (APP): {ex.GetType().Name}", "APP");
                else
                    LogError($"Objeto não-Exception: {e.ExceptionObject}", "APP");
            };

            TaskScheduler.UnobservedTaskException += (s, e) =>
            {
                LogException($"Exceção não observada (TASK): {e.Exception.GetType().Name}", "TASK");
                e.SetObserved();
            };
        }
    }
}