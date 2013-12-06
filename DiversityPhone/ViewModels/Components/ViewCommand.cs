
using DiversityPhone.Model;
using System;
using System.Reactive.Linq;
namespace DiversityPhone.ViewModels
{
    class ViewCommand<T> : ReactiveCommand<IElementVM<T>>
    {
        private static Func<IElementVM<T>, bool> CanView(DataVMServices Services)
        {
            var policy = Services.EditPolicy;
            return vm => vm != null && policy.CanView(vm.Model);
        }

        public ViewCommand(DataVMServices Services)
            : base(CanView(Services))
        {
            Services.Messenger.RegisterMessageSource(
                this.Where(CanView(Services)),
                MessageContracts.VIEW
                );
        }
    }
}
