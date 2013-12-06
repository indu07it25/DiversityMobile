using DiversityPhone.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Windows.Input;
namespace DiversityPhone.ViewModels
{
    public class ChildListVM<T, TChild> : AsyncLoadCollection<IElementVM<TChild>> where TChild : class, IReadOnlyEntity
    {
        DataVMServices Services;
        VMChangeListener<IElementVM<TChild>> _ChangeListener;

        private void UpdateListenerFilter(Expression<Func<TChild, bool>> predicateExpression)
        {
            var predicate = predicateExpression.Compile();
            _ChangeListener.ItemFilter = vm => vm != null && predicate(vm.Model);
        }


        private IEnumerable<IElementVM<TChild>> ChildCollection(
            Expression<Func<TChild, bool>> childPredicate
            )
        {
            return Services.Storage.Get<TChild>(childPredicate)
                .Select(Services.VMFactory.CreateVM);
        }

        public ICommand View { get; private set; }

        public ChildListVM(
            DataVMServices Services,
            IObservable<IElementVM<T>> vmStream,
            Func<T, Expression<Func<TChild, bool>>> childPredicateFactory
            )
            : base(Services.ThreadPool, Services.Dispatcher)
        {
            this.Services = Services;
            _ChangeListener = new VMChangeListener<IElementVM<TChild>>(Services, this);

            vmStream.Model()
                .Select(childPredicateFactory)
                .Do(UpdateListenerFilter)
                .Select(ChildCollection)
                .Subscribe(this.ContentObserver);

            View = new ViewCommand<TChild>(Services);
        }

    }
}
