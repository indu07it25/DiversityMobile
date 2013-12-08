namespace DiversityPhone.ViewModels
{
    using DiversityPhone.Interface;
    using DiversityPhone.Services;
    using ReactiveUI;
    using System;
    using System.Reactive;
    using System.Reactive.Concurrency;
    using System.Reactive.Linq;
    using System.Reactive.Subjects;

    public interface IPageServices : IPageServices<IUseActivation> { }

    public interface IPageServices<out T> where T : IUseActivation
    {
        T Services { get; }
    }

    public interface IPageActivation : IObservable<bool>, IActivatable { }

    public interface IActivatable
    {
        void Activate();
        void Deactivate();
    }

    public interface IUseActivation
    {
        IPageActivation Activation { get; }
    }

    public interface IUseCommunication : IUseActivation
    {
        IMessageBus Messenger { get; }
        INotificationService Notifications { get; }
    }

    public interface IUseThreading : IUseActivation
    {
        IScheduler Dispatcher { get; }
        IScheduler ThreadPool { get; }
    }

    public interface IUseFieldData
    {
        IFieldDataService Storage { get; }
        IVocabularyService Vocabulary { get; }
        ITaxonService Taxa { get; }
        ICreateViewModels VMFactory { get; }
        IViewEditPolicy EditPolicy { get; }
    }

    public interface IUseBaseServices : IUseActivation, IUseCommunication, IUseThreading { }

    public interface IEditPageServices : IUseBaseServices, IUseFieldData
    {
    }

    public class PageVMServices : IUseBaseServices
    {
        [Inject, Dispatcher]
        public IScheduler Dispatcher { get; set; }
        [Inject, ThreadPool]
        public IScheduler ThreadPool { get; set; }
        [Inject]
        public IMessageBus Messenger { get; set; }
        [Inject]
        public INotificationService Notifications { get; set; }
        [Inject]
        public IPageActivation Activation { get; set; }
        [Inject]
        public ICreateViewModels VMFactory { get; set; }
    }

    public class DataVMServices : PageVMServices, IEditPageServices
    {
        [Inject]
        public IFieldDataService Storage { get; set; }
        [Inject]
        public IVocabularyService Vocabulary { get; set; }
        [Inject]
        public ITaxonService Taxa { get; set; }
        [Inject]
        public IViewEditPolicy EditPolicy { get; set; }
    }

    public class MapVMServices : DataVMServices
    {
        [Inject]
        public ILocationService Location { get; set; }
        [Inject]
        public IMapStorageService Maps { get; set; }
    }


    public class OnlineVMServices : PageVMServices
    {
        [Inject]
        public IConnectivityService Connectivity { get; set; }
        [Inject]
        public IDiversityServiceClient Repository { get; set; }
        [Inject]
        public IKeyMappingService Mappings { get; set; }
        [Inject]
        public ICredentialsService Credentials { get; set; }
    }



    public sealed class PageActivator : ObservableBase<bool>, IPageActivation
    {
        Subject<bool> _Inner = new Subject<bool>();

        public void Activate()
        {
            _Inner.OnNext(true);
        }
        public void Deactivate()
        {
            _Inner.OnNext(false);
        }

        protected override IDisposable SubscribeCore(IObserver<bool> observer)
        {
            return _Inner.Subscribe(observer);
        }
    }

    public static class VMExtensions
    {
        public static IObservable<Unit> FirstActivation(this IPageActivation page)
        {
            return page.OnActivation()
                .Take(1);
        }

        public static IObservable<Unit> OnActivation(this IPageActivation page)
        {
            if (page == null)
                throw new ArgumentNullException("page");

            return page.Where(active => active)
                .Select(_ => Unit.Default);
        }

        public static IObservable<Unit> OnDeactivation(this IPageActivation page)
        {
            if (page == null)
                throw new ArgumentNullException("page");

            return page.Where(active => !active)
                .Select(_ => Unit.Default);
        }
    }
}
