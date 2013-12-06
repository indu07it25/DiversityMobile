
using DiversityPhone.Model;
using ReactiveUI;
using ReactiveUI.Xaml;
namespace DiversityPhone.ViewModels
{
    class EditPageVM<T> : IPageServices<DataVMServices>
    {
        public DataVMServices Services
        {
            get;
            private set;
        }

        public IReactiveCommand Save { get; private set; }
        public IReactiveCommand ToggleEditable { get; private set; }
        public IReactiveCommand Delete { get; private set; }


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

        public EditPageVM(DataVMServices Services, Page thisPage)
        {
            Services.Messenger.RegisterPageFor<IElementVM<T>>(thisPage, MessageContracts.NEW);
            Services.Messenger.RegisterPageFor<IElementVM<T>>(thisPage, MessageContracts.VIEW_DETAILS);

            this.Services = Services;            

            ToggleEditable = new ReactiveCommand(ModelByVisitObservable.Select(Services.EditPolicy.CanEdit));
            _IsEditable = this.ObservableToProperty(
                    Observable.Merge(
                        ModelByVisitObservable
                        .Select(m => m.IsNew()),
                        ToggleEditable.Select(_ => !IsEditable)
                    ),
                x => x.IsEditable);

            Delete = new ReactiveCommand(ModelByVisitObservable.Select(m => !m.IsNew()));
            Delete
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

        protected virtual void UpdateView(T model) { }
        protected virtual void UpdateModel(T model) { }

    }
}
