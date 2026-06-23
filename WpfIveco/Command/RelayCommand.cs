using System;
using System.Windows.Input;

namespace WpfIveco.ViewModels
{
    /// <summary>
    /// Implementação do padrão RelayCommand para MVVM.
    /// Encapsula ações e verificação de disponibilidade (CanExecute).
    /// </summary>
    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Predicate<object> _canExecute;

        /// <summary>
        /// Inicializa o comando com a ação a ser executada.
        /// </summary>
        /// <param name="execute">Ação a ser executada.</param>
        /// <param name="canExecute">Função que verifica se o comando pode ser executado (opcional).</param>
        public RelayCommand(Action<object> execute, Predicate<object> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        /// <summary>
        /// Evento acionado quando a disponibilidade do comando muda.
        /// </summary>
        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        /// <summary>
        /// Verifica se o comando pode ser executado.
        /// </summary>
        public bool CanExecute(object parameter) => _canExecute == null || _canExecute(parameter);

        /// <summary>
        /// Executa o comando.
        /// </summary>
        public void Execute(object parameter) => _execute(parameter);
    }
}