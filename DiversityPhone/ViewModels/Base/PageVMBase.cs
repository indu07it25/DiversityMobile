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

    public interface IPageServices : IPageServices<PageVMServices> { }

    public interface IPageServices<T> where T : PageVMServices
    {
        T Services { get; }
    }

    public interface IPageActivation : IActivatable
    {
        IObservable<bool> ActivationObservable { get; }
    }

    public interface IActivatable
    {
        void Activate();
        void Deactivate();
    }

    public class PageVMServices
    {
        [Dispatcher]
        public IScheduler Dispatcher { get; set; }
        [ThreadPool]
        public IScheduler ThreadPool { get; set; }
        public IMessageBus Messenger { get; set; }
        public INotificationService Notifications { get; set; }
        public IPageActivation Activation { get; set; }
    }

    public class DataVMServices : PageVMServices
    {
        public IFieldDataService Storage { get; set; }
        public IVocabularyService Vocabulary { get; set; }
        public ITaxonService Taxa { get; set; }
        public IEditPolicy EditPolicy { get; set; }
    }

    public class MapVMServices : DataVMServices
    {
        public ILocationService Location { get; set; }
        public IMapStorageService Maps { get; set; }
    }


    public class OnlineVMServices : PageVMServices
    {
        public IConnectivityService Connectivity { get; set; }
        public IDiversityServiceClient Repository { get; set; }
        public IKeyMappingService Mappings { get; set; }
        public ICredentialsService Credentials { get; set; }
    }



    public sealed class PageActivator : IPageActivation
    {
        private ISubject<bool> ActivationSubject = new Subject<bool>();
        public IObservable<bool> ActivationObservable { get; private set; }

        public void Activate()
        {
            ActivationSubject.OnNext(true);
        }
        public void Deactivate()
        {
            ActivationSubject.OnNext(false);
        }

        public PageActivator()
        {
            ActivationObservable = ActivationSubject;
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

            return page.ActivationObservable.Where(active => active)
                .Select(_ => Unit.Default);
        }

        public static IObservable<Unit> OnDeactivation(this IPageActivation page)
        {
            if (page == null)
                throw new ArgumentNullException("page");

            return page.ActivationObservable.Where(active => !active)
                .Select(_ => Unit.Default);
        }
    }
}
