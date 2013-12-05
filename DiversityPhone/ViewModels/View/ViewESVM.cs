namespace DiversityPhone.ViewModels
{
    using DiversityPhone.Interface;
    using DiversityPhone.Model;
    using ReactiveUI;
    using ReactiveUI.Xaml;
    using System;
    using System.Linq;
    using System.Reactive.Concurrency;
    using System.Reactive.Linq;

    public class ViewESVM : ViewPageVMBase<EventSeries>
    {
        private ReactiveAsyncCommand getEvents = new ReactiveAsyncCommand();

        #region Commands
        public ReactiveCommand AddEvent { get; private set; }
        public ReactiveCommand Maps { get; private set; }
        public ReactiveCommand<IElementVM<EventSeries>> EditSeries { get; private set; }
        public ReactiveCommand<IElementVM<Event>> SelectEvent { get; private set; }
        #endregion

        public ReactiveCollection<IElementVM<Event>> EventList { get; private set; }

        public ViewESVM(DataVMServices Services)
            : base(Services)
        {

            EditSeries = new ReactiveCommand<IElementVM<EventSeries>>(vm => !vm.Model.IsNoEventSeries());
            EditSeries
                .ToMessage(Services.Messenger, MessageContracts.EDIT);

            EventList = new ReactiveCollection<IElementVM<Event>>();
            EventList
                .ListenToChanges<Event>(ev => ev.SeriesID == Current.Model.EventSeriesID());

            CurrentModelObservable
                .Merge(
                    from refresh in Services.Messenger.Listen<EventMessage>(MessageContracts.INIT)
                    from activation in Services.Activation.OnActivation().TakeUntil(CurrentModelObservable)
                    select Current.Model
                    )
                .Do(_ => EventList.Clear())
                .SelectMany(m =>
                    Services.Storage.Get<Event>(ev => ev.SeriesID == m.EventSeriesID())
                    .Select(ev => new EventVM(ev))
                    .ToObservable(ThreadPoolScheduler.Instance)
                    .TakeUntil(CurrentModelObservable)
                    )
                .ObserveOnDispatcher()
                .Subscribe(EventList.Add);

            SelectEvent = new ReactiveCommand<IElementVM<Event>>();
            SelectEvent
                .ToMessage(Services.Messenger, MessageContracts.VIEW);

            AddEvent = new ReactiveCommand();
            AddEvent
                .Select(_ => new EventVM(
                    new Event()
                    {
                        SeriesID = Current.Model.IsNoEventSeries() ? null : Current.Model.SeriesID as int?
                    }) as IElementVM<Event>)
                .ToMessage(Services.Messenger, MessageContracts.EDIT);

            Maps = new ReactiveCommand(CurrentModelObservable.Select(es => !es.IsNoEventSeries()));
            Maps
                .Select(_ => Current.Model as ILocationOwner)
                .ToMessage(Services.Messenger, MessageContracts.VIEW);
        }
    }
}
