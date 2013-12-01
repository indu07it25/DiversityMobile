using DiversityPhone.Interface;
using DiversityPhone.Model;
using System;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;



namespace DiversityPhone.ViewModels.Utility
{
    public static class ObservableMixin
    {
        public static IObservable<T> StartWithCancellation<T>(Action<CancellationToken, IObserver<T>> task)
        {
            return Observable.Create<T>(obs =>
                       {
                           var cancelSource = new CancellationTokenSource();
                           Observable.Start(() => task(cancelSource.Token, obs))
                               .Subscribe(_2 => { }, obs.OnError, obs.OnCompleted);

                           return Disposable.Create(cancelSource.Cancel);
                       });
        }
    }

    public class FieldDataUploadVM : IUploadVM<IElementVM>
    {
        readonly IScheduler ThreadPool;
        readonly IDiversityServiceClient Service;
        readonly IFieldDataService Storage;

        public MultipleSelectionHelper<IElementVM> Items { get; private set; }

        public IObservable<Unit> Refresh()
        {
            return Observable.Create<Unit>(obs =>
                {
                    var cancelSource = new CancellationTokenSource();
                    var cancel = cancelSource.Token;
                    Items.OnNext(
                        Storage.CollectModifications()
                            .ToObservable(ThreadPool)
                            .TakeWhile(_ => !cancel.IsCancellationRequested)
                            .Finally(obs.OnCompleted)
                            );
                    return Disposable.Create(cancelSource.Cancel);
                });
        }

        public IObservable<Tuple<int, int>> Upload()
        {
            return Observable.Defer(() => Items.SelectedItems)
                       .SelectMany(elements => ObservableMixin.StartWithCancellation<Tuple<int, int>>((cancel, obs) =>
                       {
                           var totalCount = elements.Count();
                           int idx = 0;
                           foreach (var e in elements)
                           {
                               uploadTree(e)
                                   .TakeWhile(_ => cancel.IsCancellationRequested)
                                   .Do(_ =>
                                   {
                                       ++totalCount;
                                       ++idx;
                                       obs.OnNext(Tuple.Create(++idx, totalCount));
                                   }).LastOrDefault();
                               Items.Remove(e);
                               if (cancel.IsCancellationRequested) return;

                               obs.OnNext(Tuple.Create(++idx, totalCount));
                           }
                       })).Publish().RefCount();
        }

        public FieldDataUploadVM(
            IDiversityServiceClient Service,
            IFieldDataService Storage,
            [ThreadPool] IScheduler ThreadPool,
            [Dispatcher] IScheduler Dispatcher
            )
        {
            this.ThreadPool = ThreadPool;
            this.Service = Service;
            this.Storage = Storage;

            Items = new MultipleSelectionHelper<IElementVM>();
        }


        private IObservable<Unit> uploadES(EventSeries es)
        {
            return Service.InsertEventSeries(es, Storage.Get(es.Localizations()).Select(gp => gp as ILocalizable))
                    .SelectMany(_ => Storage.Get(es.Events()).Select(ev => uploadEV(ev)))
                    .SelectMany(obs => obs);
        }

        private IObservable<Unit> uploadEV(Event ev)
        {
            return Service.InsertEvent(ev, Storage.Get(ev.Properties()))
                    .SelectMany(_ => Storage.Get(ev.Specimen()).Select(s => uploadSpecimen(s)))
                    .SelectMany(x => x);
        }

        private IObservable<Unit> uploadSpecimen(Specimen s)
        {
            return Service.InsertSpecimen(s)
                .SelectMany(_ => Storage.Get(s.ToplevelUnits()).Select(iu => uploadIU(iu)))
                .SelectMany(x => x);
        }

        private IObservable<Unit> uploadIU(IdentificationUnit iu)
        {
            return Service.InsertIdentificationUnit(iu, Storage.Get(iu.Analyses()))
                .SelectMany(_ => Storage.Get(iu.SubUnits()).Select(sub => uploadIU(sub)))
                .SelectMany(x => x);
        }
        private IObservable<Unit> uploadTree(IElementVM vm)
        {
            var model = vm.Model;
            IObservable<Unit> res = Observable.Empty<Unit>();

            if (model is EventSeries)
                res = uploadES(model as EventSeries);
            if (model is Event)
                res = uploadEV(model as Event);
            if (model is Specimen)
                res = uploadSpecimen(model as Specimen);
            if (model is IdentificationUnit)
                res = uploadIU(model as IdentificationUnit);

            return res;
        }





    }
}
