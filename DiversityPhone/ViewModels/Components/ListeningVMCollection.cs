using DiversityPhone.Interface;
using DiversityPhone.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

namespace DiversityPhone.ViewModels
{
    class ListeningVMCollection<T> : AsyncLoadCollection<IElementVM<T>>
    {
        private static IObservable<IEnumerable<IElementVM<T>>> GetVMStream(ICreateViewModels VMFactory, IObservable<IEnumerable<T>> contentStream)
        {
            return contentStream
                .Select(content => content.Select(VMFactory.CreateVM));
        }

        public ListeningVMCollection(PageVMServices Services, IObservable<IEnumerable<T>> contentStream, Predicate<T> itemFilter = null)
            : base(GetVMStream(Services.VMFactory, contentStream), Services.ThreadPool, Services.Dispatcher)
        {
            itemFilter = itemFilter ?? (i => true);

            Services.Messenger.Listen<IElementVM<T>>(MessageContracts.SAVE_NEW)
                .Where(vm => itemFilter(vm.Model))
                .ObserveOn(Services.Dispatcher)
                .Subscribe(this.Add);

            Services.Messenger.Listen<IElementVM<T>>(MessageContracts.DELETE)
                .Where(vm => itemFilter(vm.Model))
                .ObserveOn(Services.Dispatcher)
                .Subscribe(i => this.Remove(i));
        }
    }
}
