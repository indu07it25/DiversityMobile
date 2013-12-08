
using DiversityPhone.Interface;
using System;
namespace DiversityPhone.ViewModels
{
    class SaveUpdateCommand<T> : ReactiveCommand<IElementVM<T>> where T : class, IWriteableEntity
    {
        public SaveUpdateCommand(IEditPageServices Services, Func<T, bool> canSave, Action<T> updateElement)
            : base(vm => vm != null && canSave(vm.Model), scheduler: Services.Dispatcher)
        {
            this.Model()
                .Subscribe(x =>
                {
                    Services.Storage.Update<T>(x, updateElement);
                });
        }
    }
}
