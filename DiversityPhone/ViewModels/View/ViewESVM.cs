namespace DiversityPhone.ViewModels
{
    using DiversityPhone.Interface;
    using DiversityPhone.Model;
    using ReactiveUI;
    using ReactiveUI.Xaml;
    using System.Reactive.Linq;

    public class ViewESVM
    {
        public CurrentElement<EventSeries> Current { get; private set; }

        #region Commands
        public ReactiveCommand AddEvent { get; private set; }
        public ReactiveCommand Maps { get; private set; }
        #endregion

        public ReactiveCollection<IElementVM<Event>> Events { get; private set; }

        public ViewESVM(DataVMServices Services)
        {
            Services.Messenger.RegisterPageFor<EventSeries>(Page.ViewES, MessageContracts.VIEW);

            Current = new CurrentElement<EventSeries>(Services);

            Events = new ChildListVM<EventSeries, Event>(Services, Current.Observable, Queries.Events);

            AddEvent = new ReactiveCommand();
            AddEvent
                .Select(_ => new Event()
                    {
                        SeriesID = Current.Current.Model.SeriesID
                    })
                .ToMessage(Services.Messenger, MessageContracts.NEW);

            Maps = new ReactiveCommand(Current.Observable.Model().Select(es => !es.IsNoEventSeries()));
            Maps
                .Select(_ => Current.Current.Model as ILocationOwner)
                .ToMessage(Services.Messenger, MessageContracts.MAP);
        }
    }
}
