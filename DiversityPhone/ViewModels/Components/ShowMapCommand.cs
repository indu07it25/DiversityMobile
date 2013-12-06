
using DiversityPhone.Model;
using ReactiveUI.Xaml;
using System;
using System.Reactive.Linq;
namespace DiversityPhone.ViewModels
{
    public class ShowMapCommand<T> : ReactiveCommand where T : ILocalizable
    {
        public ShowMapCommand(PageVMServices Services, IObservable<T> modelStream = null)
        {
            modelStream = modelStream ?? Observable.Return(default(T));

            Services.Messenger.RegisterMessageSource(
                modelStream.SampleMostRecent(this)
                    .Select(x => x as ILocalizable),
                MessageContracts.MAP
                );
        }
    }
}
