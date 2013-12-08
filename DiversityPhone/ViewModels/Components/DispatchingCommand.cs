using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Windows.Input;

namespace DiversityPhone.ViewModels
{
    public class DispatchingCommand<TKey> : ICommand
    {
        private IDictionary<TKey, ICommand> _Commands;
        public IDictionary<TKey, ICommand> Commands
        {
            get { return _Commands; }
            set { _Commands = value; UpdateDispatch(); }
        }

        private TKey _CurrentKey;
        public TKey CurrentKey
        {
            get { return _CurrentKey; }
            set { _CurrentKey = value; UpdateDispatch(); }
        }

        public ICommand CurrentCommand { get; private set; }

        private SerialDisposable CurrentConnection = new SerialDisposable();

        private void UpdateDispatch()
        {
            var commands = Commands;
            var key = CurrentKey;

            ICommand selectedCommand = null;
            if (commands != null &&
                commands.TryGetValue(key, out selectedCommand) &&
                selectedCommand != null)
            {
                ConnectCommand(selectedCommand);
            }
            else
            {
                CurrentConnection.Disposable = null;
            }
            OnCanExecuteChanged(this, GestureEventArgs.Empty);
        }

        private void ConnectCommand(ICommand cmd)
        {
            CurrentConnection.Disposable = null;

            cmd.CanExecuteChanged += OnCanExecuteChanged;
            CurrentCommand = cmd;

            CurrentConnection.Disposable = Disposable.Create(ResetCommand);
        }

        private void ResetCommand()
        {
            var cmd = CurrentCommand;
            CurrentCommand = null;
            if (cmd != null)
            {
                cmd.CanExecuteChanged -= OnCanExecuteChanged;
            }
        }

        void OnCanExecuteChanged(object sender, System.EventArgs e)
        {
            var listeners = this.CanExecuteChanged;
            if (listeners != null)
            {
                listeners(this, e);
            }
        }

        public DispatchingCommand()
        {

        }

        public bool CanExecute(object parameter)
        {
            var cmd = CurrentCommand;
            return (cmd != null) && cmd.CanExecute(parameter);
        }

        public event System.EventHandler CanExecuteChanged;

        public void Execute(object parameter)
        {
            var cmd = CurrentCommand;
            if (cmd != null)
            {
                cmd.Execute(parameter);
            }
        }
    }
}
