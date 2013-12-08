using DiversityPhone.Interface;
using DiversityPhone.Model;
using System.Reactive.Linq;

namespace DiversityPhone.ViewModels
{
    class DeleteCommand<T> : ReactiveCommand<T> where T : class, IWriteableEntity
    {
        private static bool CanDelete(T item)
        {
            return !item.IsNew();
        }

        public DeleteCommand(
            IUseCommunication Services
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
