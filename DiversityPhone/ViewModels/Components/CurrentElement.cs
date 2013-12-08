using DiversityPhone.Interface;
using DiversityPhone.Model;
using ReactiveUI;
using System;
using System.Reactive.Linq;
using System.Windows.Input;

namespace DiversityPhone.ViewModels
{
    public class CurrentElement<T> : ReactiveObject where T : class, IWriteableEntity
    {
        ObservableAsPropertyHelper<IElementVM<T>> _Current;
        public IElementVM<T> Current { get { return _Current.Value; } }
        private T CurrentModel { get { return (_Current.Value != null) ? _Current.Value.Model : null; } }
        public IObservable<IElementVM<T>> Observable { get { return _Current; } }

        public ICommand ViewDetails { get; private set; }

        public CurrentElement(DataVMServices Services)
            : this(Services, Services.Messenger.Listen<T>(MessageContracts.VIEW))
        {

        }

        public CurrentElement(IEditPageServices Services, IObservable<T> elementStream)
        {
            _Current = this.ObservableToProperty(
                elementStream.Select(Services.VMFactory.CreateVM),
                x => x.Current);

            ViewDetails = new ViewDetailsCommand<T>(Services).WithParameter(() => CurrentModel);
            elementStream.Subscribe(x => ViewDetails.CanExecute(null));
        }
    }
}
