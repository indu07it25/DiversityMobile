
using DiversityPhone.Interface;
using DiversityPhone.Model;
using ReactiveUI;
using ReactiveUI.Xaml;
using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Windows.Input;
namespace DiversityPhone.ViewModels
{
    public abstract class EditPageVM<T> : ReactiveObject, IPageServices<IEditPageServices>, IEditPageVM where T : class, IWriteableEntity
    {
        public IEditPageServices Services
        {
            get;
            private set;
        }

        public ICommand Save { get; private set; }
        public ICommand ToggleEditable { get; private set; }
        public ICommand Delete { get; private set; }

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

        protected T Current { get { return _Current.Value; } }
        protected IObservable<T> CurrentObservable { get { return _Current; } }

        private ObservableAsPropertyHelper<bool> _IsEditable;
        private ObservableAsPropertyHelper<T> _Current;


        public EditPageVM(IEditPageServices Services, Page thisPage)
        {
            this.Services = Services;

            // Page Hiding, Display
            Services.Messenger.RegisterPageFor<T>(thisPage, MessageContracts.NEW);
            Services.Messenger.RegisterPageFor<T>(thisPage, MessageContracts.VIEW_DETAILS);

            // ToggleEditable
            var canToggleEditable = Observable.Merge(
               Services.Messenger.Listen<T>(MessageContracts.NEW).Select(_ => true),
               Services.Messenger.Listen<T>(MessageContracts.VIEW_DETAILS).Select(Services.EditPolicy.CanEdit)
               );
            var toggleEditable = new ReactiveCommand(canToggleEditable);
            ToggleEditable = toggleEditable;

            // IsEditable
            var isEditable = Observable.Merge(
                Services.Messenger.Listen<T>(MessageContracts.NEW).Select(_ => true),
                Services.Messenger.Listen<T>(MessageContracts.VIEW_DETAILS).Select(_ => false),
                toggleEditable.Select(_ => true)
                );

            _IsEditable = this.ObservableToProperty(isEditable, x => x.IsEditable);

            _Current = new ObservableAsPropertyHelper<T>(
                Observable.Merge(
                    Services.Messenger.Listen<T>(MessageContracts.NEW),
                    Services.Messenger.Listen<T>(MessageContracts.VIEW_DETAILS)
                    ),
                    x => { }
            );

            Observable.CombineLatest(
                _Current,
                Services.Activation.OnActivation(),
                (m, _) => m
            )
            .Do(x => Delete.CanExecute(x))
            .Subscribe(UpdateView);


            Delete = new ParameterlessCommand(
                new DeleteCommand<T>(Services),
                () => _Current.Value
            );

            var save = new DispatchingCommand<bool>();
            save.Commands = new Dictionary<bool, ICommand>()
            {
                {true, new ParameterlessCommand(
                    new SaveUpdateCommand<T>(Services, CanSave, UpdateModel),
                    () => _Current.Value
                    )},
                {false, new ParameterlessCommand(
                    new SaveNewCommand<T>(Services, CanSave, UpdateModel),
                    () => _Current.Value
                    )}
            };
            var saveIsUpdate = Observable.Merge(
               Services.Messenger.Listen<T>(MessageContracts.NEW).Select(_ => false),
               Services.Messenger.Listen<T>(MessageContracts.VIEW_DETAILS).Select(_ => true)
               );
            saveIsUpdate.Subscribe(x => save.CurrentKey = x);
            Save = save;
        }

        protected void UpdateCanSave()
        {
            Save.CanExecute(_Current.Value);
        }

        protected abstract bool CanSave(T model);

        protected virtual void UpdateView(T model) { }
        protected virtual void UpdateModel(T model) { }
    }
}
