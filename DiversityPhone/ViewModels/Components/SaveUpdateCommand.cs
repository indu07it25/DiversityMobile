
using DiversityPhone.Interface;
using System;
namespace DiversityPhone.ViewModels
{
    class SaveUpdateCommand<T> : ReactiveCommand<T> where T : class, IWriteableEntity
    {
        public SaveUpdateCommand(IEditPageServices Services, Func<T, bool> canSave, Action<T> updateElement)
            : base(canSave, scheduler: Services.Dispatcher)
        {
            this.Subscribe(x =>
                {
                    Services.Storage.Update<T>(x, updateElement);
                });
        }
    }
}
