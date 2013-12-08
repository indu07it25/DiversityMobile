using DiversityPhone.Model;
using System.Reactive.Linq;

namespace DiversityPhone.ViewModels
{
    class ViewDetailsCommand<T> : ReactiveCommand<T> where T : class
    {
        public ViewDetailsCommand(IEditPageServices Services)
            : base(Services.EditPolicy.CanViewDetails)
        {
            Services.Messenger.RegisterMessageSource(
                this.Where(Services.EditPolicy.CanViewDetails),
                MessageContracts.VIEW_DETAILS
                );
        }
    }
}
