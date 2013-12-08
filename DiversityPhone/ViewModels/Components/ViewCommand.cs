
using DiversityPhone.Model;
using System.Reactive.Linq;
namespace DiversityPhone.ViewModels
{
    class ViewCommand<T> : ReactiveCommand<T> where T : class
    {
        public ViewCommand(DataVMServices Services)
            : base(Services.EditPolicy.CanView)
        {
            Services.Messenger.RegisterMessageSource(
                this.Where(Services.EditPolicy.CanView),
                MessageContracts.VIEW
                );
        }
    }
}
