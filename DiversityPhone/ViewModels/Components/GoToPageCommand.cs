using DiversityPhone.Model;
using ReactiveUI;
using ReactiveUI.Xaml;
using System;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Windows.Input;

namespace DiversityPhone.ViewModels
{
    class GoToPageCommand : ICommand, IDisposable
    {
        ReactiveCommand Inner;
        CompositeDisposable Subscriptions;

        public GoToPageCommand(IMessageBus Messenger, Page targetPage, IObservable<bool> canExecute = null)
        {
            Inner = new ReactiveCommand(canExecute);
            var registration = Messenger.RegisterMessageSource(
                Inner.Select(_ => targetPage)
            );

            Subscriptions = new CompositeDisposable(registration, Inner);
        }

        public bool CanExecute(object parameter)
        {
            return Inner.CanExecute(parameter);
        }

        public event EventHandler CanExecuteChanged { add { Inner.CanExecuteChanged += value; } remove { Inner.CanExecuteChanged -= value; } }

        public void Execute(object parameter)
        {
            Inner.Execute(parameter);
        }

        public void Dispose()
        {
            if (!Subscriptions.IsDisposed)
            {
                Subscriptions.Dispose();
            }
        }
    }
}
