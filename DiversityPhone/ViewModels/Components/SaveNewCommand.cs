
using DiversityPhone.Interface;
using DiversityPhone.Model;
using System;
using System.Reactive.Linq;
namespace DiversityPhone.ViewModels
{
    public class SaveNewCommand<T> : ReactiveCommand<T> where T : class, IWriteableEntity
    {
        public SaveNewCommand(IEditPageServices Services, Func<T, bool> canSave, Action<T> updateEntity)
            : base(canSave)
        {
            var repo = Services.Storage;
            Services.Messenger.RegisterMessageSource(
                this.Do(updateEntity)
                    .Do(repo.Add),
                MessageContracts.SAVE_NEW
            );
        }
    }
}
