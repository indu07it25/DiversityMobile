
using DiversityPhone.Interface;
using DiversityPhone.Model;
using ReactiveUI;
using System;
using System.Reactive.Linq;
using System.Windows.Input;
namespace DiversityPhone.ViewModels
{
    public class CurrentElement<T> : ReactiveObject where T : IWriteableEntity
    {
        ObservableAsPropertyHelper<IElementVM<T>> _Current;
        public IElementVM<T> Current { get { return _Current.Value; } }
        public IObservable<IElementVM<T>> Observable { get { return _Current; } }


        private ICommand _ViewDetails;
        public ICommand ViewDetails { get; private set; }

        private ICommand _Delete;
        public ICommand Delete { get; private set; }



        private void ExecuteViewDetails()
        {
            var current = _Current.Value;
            if (_ViewDetails.CanExecute(current))
            {
                _ViewDetails.Execute(current);
            }
        }

        private void ExecuteDelete()
        {
            var current = _Current.Value;
            if (_ViewDetails.CanExecute(current))
            {
                _ViewDetails.Execute(current);
            }
        }

        public CurrentElement(DataVMServices Services)
            : this(Services, Services.Messenger.Listen<IElementVM<T>>(MessageContracts.VIEW))
        {

        }

        public CurrentElement(DataVMServices Services, IObservable<IElementVM<T>> vmStream)
        {
            _Current = this.ObservableToProperty(vmStream, x => x.Current);

            _ViewDetails = new ViewDetailsCommand<T>(Services);
            var viewDetails = new SwitchableCommand(ExecuteViewDetails);
            vmStream.Select(x => _ViewDetails.CanExecute(x))
                .Subscribe(x => viewDetails.IsExecutable = x);

            _Delete = new DeleteCommand<T>(Services);
            var delete = new SwitchableCommand(ExecuteDelete);
            vmStream.Select(x => _Delete.CanExecute(x))
                .Subscribe(x => delete.IsExecutable = x);
        }
    }
}
