using DiversityPhone.Interface;
using DiversityPhone.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
namespace DiversityPhone.ViewModels
{
    public interface IChangeListener<TNotify, TItem>
    {
        ICollection<TItem> ItemCollection { get; set; }

        Func<TNotify, bool> NotificationFilter { get; set; }
    }

    public class ChangeListener<TNotify, TItem> : IChangeListener<TNotify, TItem>, IDisposable
    {
        private CompositeDisposable _Subscriptions;

        public ICollection<TItem> ItemCollection { get; set; }

        private Func<TNotify, bool> _NotificationFilter;
        public Func<TNotify, bool> NotificationFilter
        {
            get { return _NotificationFilter; }
            set { _NotificationFilter = value ?? (i => true); }
        }

        private Action<ICollection<TItem>, TNotify> _Add;
        private void Add(TNotify x)
        {
            var coll = ItemCollection;
            if (coll != null)
            {
                _Add(coll, x);
            }
        }

        private Action<ICollection<TItem>, TNotify> _Remove;
        private void Remove(TNotify x)
        {
            var coll = ItemCollection;
            if (coll != null)
            {
                _Remove(coll, x);
            }
        }

        public ChangeListener(
            IUseBaseServices Services,
            Action<ICollection<TItem>, TNotify> add,
            Action<ICollection<TItem>, TNotify> remove,
            ICollection<TItem> itemCollection = null,
            Func<TNotify, bool> itemFilter = null
            )
        {
            NotificationFilter = itemFilter;
            ItemCollection = itemCollection;

            _Subscriptions = new CompositeDisposable(
                Services.Messenger.Listen<TNotify>(MessageContracts.SAVE_NEW)
                    .Where(x => NotificationFilter(x))
                    .ObserveOn(Services.Dispatcher)
                    .Subscribe(Add),
                Services.Messenger.Listen<TNotify>(MessageContracts.DELETE)
                    .Where(x => NotificationFilter(x))
                    .ObserveOn(Services.Dispatcher)
                    .Subscribe(Remove)
            );
        }

        public void Dispose()
        {
            _Subscriptions.Dispose();
        }
    }

    public class SimpleChangeListener<TItem> : ChangeListener<TItem, TItem>
    {
        public SimpleChangeListener(
                IUseBaseServices Services,
                ICollection<TItem> ItemCollection = null,
                Func<TItem, bool> itemFilter = null
            )
            : base(
                Services,
                (c, i) => c.Add(i),
                (c, i) => c.Remove(i),
                ItemCollection,
                itemFilter
            )
        {

        }
    }

    public class VMChangeListener<TEntity> : ChangeListener<TEntity, IElementVM<TEntity>>
    {
        private static Action<ICollection<IElementVM<TEntity>>, TEntity> Add(ICreateViewModels VMFactory)
        {
            return (coll, x) => coll.Add(VMFactory.CreateVM(x));
        }

        private static Action<ICollection<IElementVM<TEntity>>, TEntity> Remove(ICreateViewModels VMFactory)
        {
            return (coll, x) =>
            {
                var vm = coll.FirstOrDefault(y => Object.ReferenceEquals(y.Model, x));
                if (vm != null)
                {
                    coll.Remove(vm);
                }
            };
        }

        public VMChangeListener(
                IEditPageServices Services,
                ICollection<IElementVM<TEntity>> ItemCollection = null,
                Func<TEntity, bool> itemFilter = null
            )
            : base(
                Services,
                Add(Services.VMFactory),
                Remove(Services.VMFactory),
                ItemCollection,
                itemFilter
            )
        {
        }
    }
}
