
using DiversityPhone.Interface;
using DiversityPhone.Model;
using ReactiveUI;
using System;
using System.Windows.Input;
namespace DiversityPhone.ViewModels
{
    public class CurrentElement<T> : ReactiveObject where T : IWriteableEntity
    {
        ObservableAsPropertyHelper<IElementVM<T>> _Current;
        public IElementVM<T> Current { get { return _Current.Value; } }
        public IObservable<IElementVM<T>> Observable { get { return _Current; } }

        public ICommand ViewDetails { get; private set; }

        public CurrentElement(DataVMServices Services)
            : this(Services, Services.Messenger.Listen<IElementVM<T>>(MessageContracts.VIEW))
        {

        }

        public CurrentElement(DataVMServices Services, IObservable<IElementVM<T>> vmStream)
        {
            _Current = this.ObservableToProperty(vmStream, x => x.Current);

            ViewDetails = new ParameterlessCommand(
                new ViewDetailsCommand<T>(Services),
                () => _Current.Value
            );
            vmStream.Subscribe(x => ViewDetails.CanExecute(x));
        }
    }
}
