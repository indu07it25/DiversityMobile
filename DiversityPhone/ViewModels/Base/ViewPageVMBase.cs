using DiversityPhone.Model;
using System;
using System.Reactive.Linq;

namespace DiversityPhone.ViewModels
{
    public class ViewPageVMBase<T> : ElementPageVMBase<T>
    {
        public ViewPageVMBase(
            PageVMServices Services,
            Predicate<T> filter = null
            )
            : base(Services)
        {
            Services.Messenger.Listen<IElementVM<T>>(MessageContracts.VIEW)
                .Where(vm => vm != null && vm.Model != null)
                .Where(vm => filter == null || filter(vm.Model))
                .Subscribe(x => Current = x);

            //If the current element has been deleted in the meantime, navigate back.
            Observable.CombineLatest(
                Services.Activation
                .Select(active => active ? Current : null),
                Services.Messenger.Listen<IElementVM<T>>(MessageContracts.DELETE),
                (current, deleted) => current == deleted
            )
                .Where(current_deleted => current_deleted)
                .Select(_ => Page.Previous)
                .ToMessage(Services.Messenger);
        }
    }
}
