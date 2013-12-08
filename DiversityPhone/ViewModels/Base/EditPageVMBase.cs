using DiversityPhone.Interface;
using DiversityPhone.Model;
using ReactiveUI;
using ReactiveUI.Xaml;
using System;
using System.Reactive;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows.Input;

namespace DiversityPhone.ViewModels
{
    public abstract class EditPageVMBase<T> : ElementPageVMBase<T>, IEditPageVM where T : IWriteableEntity, IReactiveNotifyPropertyChanged
    {
        public ICommand Save { get; private set; }
        public ICommand ToggleEditable { get; private set; }
        public ICommand Delete { get; private set; }


        private ObservableAsPropertyHelper<bool> _IsEditable;
        /// <summary>
        /// Shows, whether the current Object can be Edited
        /// </summary>
        public bool IsEditable
        {
            get
            {
                return _IsEditable.Value;
            }
        }

        protected ISubject<bool> CanSaveSubject { get; private set; }
        private ISubject<Unit> DeleteSubject = new Subject<Unit>();

        public EditPageVMBase(
            DataVMServices Services,
            Predicate<T> filter = null
            )
            : base(Services)
        {
            CanSaveSubject = new Subject<bool>();
            var save = new ReactiveCommand(CanSaveSubject);
            Save = save;
            save
                .Do(_ => UpdateModel())
                .Select(_ => Current)
                .Do(_ => Services.Messenger.SendMessage(Page.Previous))
                .ToMessage(Services.Messenger, MessageContracts.SAVE);

            var toggleEditable = new ReactiveCommand(ModelByVisitObservable.Select(Services.EditPolicy.CanEdit));
            ToggleEditable = toggleEditable;
            _IsEditable = this.ObservableToProperty(
                    Observable.Merge(
                        ModelByVisitObservable
                        .Select(m => m.IsNew()),
                        toggleEditable.Select(_ => !IsEditable)
                    ),
                x => x.IsEditable);

            var delete = new ReactiveCommand(ModelByVisitObservable.Select(m => !m.IsNew()));
            Delete = delete;
            delete
                .SelectMany(_ =>
                    Services.Notifications.showDecision(DiversityResources.Message_ConfirmDelete)
                    .Where(x => x)
                    .Select(_2 => Current)
                    .Do(_2 => Services.Messenger.SendMessage(Page.Previous))
                )
                .ToMessage(Services.Messenger, MessageContracts.DELETE);


            Services.Messenger.Listen<IElementVM<T>>(MessageContracts.VIEW_DETAILS)
                .Where(vm => vm != null)
                .Where(vm => filter == null || filter(vm.Model))
                .Subscribe(x => Current = x);
        }

        protected virtual void UpdateModel() { }
    }
}
