using DiversityPhone.Interface;
using DiversityPhone.Model;
using System.Reactive.Linq;

namespace DiversityPhone.ViewModels
{
    class DeleteCommand<T> : ReactiveCommand<IElementVM<T>> where T : IWriteableEntity
    {
        private static bool CanDelete(IElementVM<T> item)
        {
            return (item != null) && !item.Model.IsNew();
        }

        public DeleteCommand(
            PageVMServices Services
            )
            : base(CanDelete)
        {
            Services.Messenger.RegisterMessageSource(
                this.SelectMany(toBeDeleted => Services.Notifications.showDecision(DiversityResources.Message_ConfirmDelete)
                        .Where(x => x)
                        .Select(_ => toBeDeleted)
                    ),
                MessageContracts.DELETE
            );
        }
    }
}
