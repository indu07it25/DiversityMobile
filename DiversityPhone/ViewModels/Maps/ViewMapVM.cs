﻿using DiversityPhone.Interface;
using DiversityPhone.Model;
using DiversityPhone.Services;
using ReactiveUI;
using ReactiveUI.Xaml;
using System;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Media.Imaging;

namespace DiversityPhone.ViewModels
{
    public class ViewMapVM : ReactiveObject, ISavePageVM
    {
        public PageVMServices Services { get; private set; }

        public ReactiveCommand SelectMap { get; private set; }
        public IReactiveCommand ToggleEditable { get; private set; }
        public ReactiveCommand SetLocation { get; private set; }
        public IReactiveCommand Save { get; private set; }

        public IElementVM<Map> CurrentMap { get { return _CurrentMap.Value; } }
        private ObservableAsPropertyHelper<IElementVM<Map>> _CurrentMap;

        public double ImageScale { get; set; }

        public Point ImageOffset { get; set; }

        public bool IsEditable { get { return _IsEditable.Value; } }
        private ObservableAsPropertyHelper<bool> _IsEditable;

        private string _MapUri;
        public string MapUri
        {
            get { return _MapUri; }
            set { this.RaiseAndSetIfChanged(x => x.MapUri, ref _MapUri, value); }
        }



        private BitmapImage _MapImage;
        public BitmapImage MapImage
        {
            get
            {
                return _MapImage;
            }
            set
            {
                this.RaiseAndSetIfChanged(x => x.MapImage, ref _MapImage, value);
            }
        }


        private Point? _CurrentLocation = null;
        public Point? CurrentLocation
        {
            get
            {
                return _CurrentLocation;
            }
            private set
            {
                this.RaiseAndSetIfChanged(x => x.CurrentLocation, ref _CurrentLocation, value);
            }
        }

        private Point? _PrimaryLocalization = null;
        public Point? PrimaryLocalization
        {
            get
            {
                return _PrimaryLocalization;
            }
            private set
            {
                this.RaiseAndSetIfChanged(x => x.PrimaryLocalization, ref _PrimaryLocalization, value);
            }
        }

        private IObservable<IObservable<Point?>> _AdditionalLocalizations;
        public IObservable<IObservable<Point?>> AdditionalLocalizations
        {
            get
            {
                return _AdditionalLocalizations;
            }
        }

        public ViewMapVM(
            MapVMServices Services
            )
        {
            this.Services = Services;

            ImageScale = 1.0;
            ImageOffset = new Point();

            SelectMap = new ReactiveCommand();
            SelectMap
                .Select(_ => Page.MapManagement)
                .ToMessage(Services.Messenger);

            Services.Activation.FirstActivation()
                .Select(_ => Page.MapManagement)
                .ToMessage(Services.Messenger);


            _CurrentMap = this.ObservableToProperty(Services.Messenger.Listen<IElementVM<Map>>(MessageContracts.VIEW), x => x.CurrentMap);
            _CurrentMap
                .Where(vm => vm != null)
                .Select(vm => Observable.Start(() => Services.Maps.loadMap(vm.Model)))
                .Switch()
                .ObserveOnDispatcher()
                .Select(stream =>
                {
                    var img = new BitmapImage();
                    img.SetSource(stream);
                    stream.Close();
                    return img;
                })
                .Subscribe(x => MapImage = x);

            var current_series = Services.Messenger.Listen<ILocationOwner>(MessageContracts.VIEW);

            var current_localizable = Services.Messenger.Listen<ILocalizable>(MessageContracts.VIEW);

            var current_series_if_not_localizable = current_series.Merge(current_localizable.Select(_ => null as ILocationOwner));

            var current_localizable_if_not_series = current_localizable.Merge(current_series.Select(_ => null as ILocalizable));

            var series_and_map =
            current_series_if_not_localizable
                .CombineLatest(_CurrentMap.Where(x => x != null), (es, map) =>
                    new { Map = map.Model, Series = es })
                .Publish();


            var add_locs =
            series_and_map
                .Select(pair =>
                {
                    if (pair.Series != null)
                    {
                        var stream = Services.Storage.Get((pair.Series as EventSeries).Localizations()).ToObservable(ThreadPoolScheduler.Instance) //Fetch geopoints asynchronously on Threadpool thread
                                .Merge(Services.Messenger.Listen<Localization>(MessageContracts.SAVE).Where(gp => gp.RelatedID == pair.Series.EntityID)) //Listen to new Geopoints that are added to the current tour
                                .Select(gp => pair.Map.PercentilePositionOnMap(gp))
                                .TakeUntil(series_and_map)
                                .Replay();
                        stream.Connect();
                        return stream as IObservable<Point?>;
                    }
                    else
                        return Observable.Empty<Point?>();
                }).Replay(1);

            _AdditionalLocalizations = add_locs;
            add_locs.Connect();

            series_and_map.Connect();

            Observable.CombineLatest(
                current_localizable_if_not_series,
                _CurrentMap,
                (loc, map) =>
                {
                    if (map == null)
                        return null;
                    return map.Model.PercentilePositionOnMap(loc);
                })
                .Subscribe(c => PrimaryLocalization = c);



            ToggleEditable = new ReactiveCommand(current_localizable_if_not_series.Select(l => l != null));

            _IsEditable = this.ObservableToProperty(
                                current_localizable_if_not_series.Select(_ => false)
                                .Merge(ToggleEditable.Select(_ => true)),
                                x => x.IsEditable);

            SetLocation = new ReactiveCommand(_IsEditable);
            SetLocation
                .Select(loc => loc as Point?)
                .Where(loc => loc != null)
                .Subscribe(loc => PrimaryLocalization = loc);

            var valid_localization = this.ObservableForProperty(x => x.PrimaryLocalization).Value()
                .Select(loc => loc.HasValue);



            Save = new ReactiveCommand(_IsEditable.BooleanAnd(valid_localization));
            current_localizable_if_not_series
                .Where(loc => loc != null)
                .Select(loc =>
                    Save
                    .Select(_ => loc)
                    )
                .Switch()
                .Do(c => c.SetCoordinates(CurrentMap.Model.GPSFromPercentilePosition(PrimaryLocalization.Value)))
                .Do(_ => Services.Messenger.SendMessage(Page.Previous))
                .ToMessage(Services.Messenger, MessageContracts.SAVE);

            Services.Activation.OnActivation()
                .Where(_ => CurrentMap != null)
                .SelectMany(_ => Services.Location.Location().StartWith(null as Coordinate).TakeUntil(Services.Activation.OnDeactivation()))
                .Select(c => CurrentMap.Model.PercentilePositionOnMap(c))
                .Subscribe(c => CurrentLocation = c);

        }
    }
}
