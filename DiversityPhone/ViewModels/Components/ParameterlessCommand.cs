using ReactiveUI.Xaml;
using System;
using System.Windows.Input;

namespace DiversityPhone.ViewModels
{
    public static class CommandDefaultParameter
    {
        public static ICommand WithParameter(this ICommand cmd, Func<object> retrieveParameter)
        {
            return new ParameterlessCommand(cmd, retrieveParameter);
        }
    }

    public class ParameterlessCommand : IReactiveCommand
    {
        ICommand _Inner;
        Func<object> _RetrieveParameter;

        public ParameterlessCommand(
            ICommand inner,
            Func<object> retrieveParam
            )
        {
            _Inner = inner;
            _RetrieveParameter = retrieveParam;
        }


        public bool CanExecute(object parameter)
        {
            return _Inner.CanExecute(_RetrieveParameter());
        }

        public event System.EventHandler CanExecuteChanged
        {
            add { _Inner.CanExecuteChanged += value; }
            remove { _Inner.CanExecuteChanged -= value; }
        }

        public void Execute(object parameter)
        {
            _Inner.Execute(_RetrieveParameter());
        }

        public IObservable<bool> CanExecuteObservable
        {
            get { throw new NotImplementedException(); }
        }

        public IDisposable Subscribe(IObserver<object> observer)
        {
            throw new NotImplementedException();
        }

        public IObservable<Exception> ThrownExceptions
        {
            get { throw new NotImplementedException(); }
        }
    }
}
