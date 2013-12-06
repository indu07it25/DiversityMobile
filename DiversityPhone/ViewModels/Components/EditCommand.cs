using DiversityPhone.Interface;
using DiversityPhone.Model;
using System;
using System.Reactive.Linq;

namespace DiversityPhone.ViewModels
{
    class ViewDetailsCommand<T> : ReactiveCommand<IElementVM<T>>
    {
        private static Func<IElementVM<T>, bool> CanViewDetails(IViewEditPolicy policy)
        {
            return vm => vm != null && policy.CanViewDetails(vm.Model);
        }

        public ViewDetailsCommand(DataVMServices Services)
            : base(CanViewDetails(Services.EditPolicy))
        {
            Services.Messenger.RegisterMessageSource(
                this.Where(CanViewDetails(Services.EditPolicy)),
                MessageContracts.VIEW_DETAILS
                );
        }
    }
}
