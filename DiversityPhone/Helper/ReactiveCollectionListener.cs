namespace DiversityPhone.ViewModels
{
    using DiversityPhone.Model;
    using ReactiveUI;
    using System;
    using System.Reactive.Disposables;
    using System.Reactive.Linq;

    public static class ReactiveCollectionListenerMixin
    {
        public static IDisposable ListenToChanges<T>(this ReactiveCollection<IElementVM<T>> This, Func<T, bool> filter = null)
        {
            if (This == null)
                throw new ArgumentNullException("This");
            if (filter == null)
                filter = (a) => true;

            var messenger = MessageBus.Current;

            return new CompositeDisposable(
                   messenger.Listen<IElementVM<T>>(MessageContracts.SAVE)
                        .Where(vm => filter(vm.Model))
                        .Where(vm => vm != null && !This.Contains(vm))
                       .Subscribe(This.Add),
                   messenger.Listen<IElementVM<T>>(MessageContracts.DELETE)
                        .Where(vm => filter(vm.Model))
                        .Where(vm => vm != null)
                       .Subscribe(i => This.Remove(i))
                   );
        }
    }
}
