namespace DiversityPhone.ViewModels
{
    using DiversityPhone.Model;
    using ReactiveUI;
    using ReactiveUI.Xaml;
    using System;
    using System.Linq;
    using System.Reactive;
    using System.Reactive.Linq;

    public class HomeVM : ReactiveObject, IPageServices<MapVMServices>
    {
        public MapVMServices Services { get; set; }

        private ReactiveAsyncCommand getSeries = new ReactiveAsyncCommand();

        #region Commands
        public ReactiveCommand Settings { get; private set; }
        public ReactiveCommand Add { get; private set; }
        public ReactiveCommand Maps { get; private set; }
        public ReactiveCommand Help { get; private set; }

        public ReactiveCommand<IElementVM<EventSeries>> SelectSeries { get; private set; }
        public ReactiveCommand<IElementVM<EventSeries>> EditSeries { get; private set; }
        #endregion

        #region Properties
        public ReactiveCollection<EventSeriesVM> SeriesList
        {
            get;
            private set;
        }
        #endregion

        public HomeVM(
            MapVMServices Services
            )
        {
            this.Services = Services;


            //EventSeries
            SeriesList = new ReactiveCollection<EventSeriesVM>();

            getSeries.RegisterAsyncFunction(_ =>
                    Enumerable.Repeat(NoEventSeriesMixin.NoEventSeries, 1)
                    .Concat(
                        Services.Storage
                        .GetAll<EventSeries>()
                        )
                    .Select(es => new EventSeriesVM(es))
                )
                .SelectMany(vm => vm)
                .ObserveOn(Services.Dispatcher)
                .Subscribe(SeriesList.Add);

            SeriesList
                    .ListenToChanges<EventSeries, EventSeriesVM>();

            (SelectSeries = new ReactiveCommand<IElementVM<EventSeries>>())
                .ToMessage(Services.Messenger, MessageContracts.VIEW);

            (EditSeries = new ReactiveCommand<IElementVM<EventSeries>>(vm => vm.Model != NoEventSeriesMixin.NoEventSeries))
                .ToMessage(Services.Messenger, MessageContracts.EDIT);



            var openSeries = SeriesList.CollectionCountChanged.Select(_ => Unit.Default)
                .Merge(Services.Messenger.Listen<IElementVM<EventSeries>>(MessageContracts.SAVE).Select(_ => Unit.Default))
                .Select(_ => SeriesList.Where(s => s.Model.SeriesEnd == null))
                .Select(list => list.FirstOrDefault());

            openSeries
                .SelectMany(series => (series != null) ?
                    Services.Location.LocationByDistanceThreshold(20)
                    .Select(c =>
                    {
                        var gp = new Localization() { RelatedID = series.Model.SeriesID.Value };
                        gp.SetCoordinates(c);
                        return gp;
                    })
                    .TakeUntil(openSeries) : Observable.Empty<Localization>())
                .ObserveOn(Services.Dispatcher)
                .ToMessage(Services.Messenger, MessageContracts.SAVE);


            var noOpenSeries =
                openSeries
                .Select(openseries => openseries == null);

            Settings = new ReactiveCommand();
            Settings.Select(_ => Page.Settings)
                .ToMessage(Services.Messenger);

            Add = new ReactiveCommand(noOpenSeries);
            Add.Select(_ => new EventSeriesVM(new EventSeries()) as IElementVM<EventSeries>)
                .ToMessage(Services.Messenger, MessageContracts.EDIT);

            Maps = new ReactiveCommand();
            Maps.Select(_ => null as ILocalizable)
                .ToMessage(Services.Messenger, MessageContracts.VIEW);

            Help = new ReactiveCommand();
            Help.Select(_ => Page.Info)
               .ToMessage(Services.Messenger);


            Services.Messenger.Listen<EventMessage>(MessageContracts.INIT)
                .Do(_ => SeriesList.Clear())
                .Subscribe(_ => getSeries.Execute(null));
        }
    }
}