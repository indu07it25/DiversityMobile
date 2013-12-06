using DiversityPhone.Model;
using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace DiversityPhone.ViewModels
{
    public class VMChangeListener<T> : IDisposable
    {
        private CompositeDisposable _Subscriptions;

        private ICollection<T> _ItemCollection;

        private Func<T, bool> _ItemFilter;
        public Func<T, bool> ItemFilter
        {
            get { return _ItemFilter; }
            set { _ItemFilter = value ?? (i => true); }
        }

        public VMChangeListener(PageVMServices Services, ICollection<T> ItemCollection, Func<T, bool> itemFilter = null)
        {
            ItemFilter = itemFilter;
            _ItemCollection = ItemCollection;

            _Subscriptions = new CompositeDisposable(
                Services.Messenger.Listen<T>(MessageContracts.SAVE_NEW)
                    .Where(x => ItemFilter(x))
                    .ObserveOn(Services.Dispatcher)
                    .Subscribe(ItemCollection.Add),
                Services.Messenger.Listen<T>(MessageContracts.DELETE)
                    .Where(x => ItemFilter(x))
                    .ObserveOn(Services.Dispatcher)
                    .Subscribe(i => ItemCollection.Remove(i))
            );
        }

        public void Dispose()
        {
            _Subscriptions.Dispose();
        }
    }
}
