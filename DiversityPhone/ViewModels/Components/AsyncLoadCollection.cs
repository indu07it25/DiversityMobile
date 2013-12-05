using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;

namespace DiversityPhone.ViewModels
{
    class AsyncLoadCollection<T> : ReactiveCollection<T>, IDisposable
    {
        readonly IScheduler LoadScheduler, AddScheduler;

        IDisposable StreamSubscription;
        SerialDisposable ContentSubscription = new SerialDisposable();

        /// <summary>
        /// Clears current contents and starts loading the new contents
        /// </summary>
        /// <param name="newContents"></param>
        private void SwapContents(IEnumerable<T> newContents)
        {
            this.Clear();
            ContentSubscription.Disposable = newContents
                .ToObservable(LoadScheduler)
                .ObserveOn(AddScheduler)
                .Subscribe(this.Add);
        }

        public AsyncLoadCollection(IObservable<IEnumerable<T>> contentStream, IScheduler loadScheduler, IScheduler addScheduler)
        {
            Contract.Requires(contentStream != null);
            Contract.Requires(loadScheduler != null);
            Contract.Requires(addScheduler != null);

            LoadScheduler = loadScheduler;
            AddScheduler = addScheduler;

            StreamSubscription = contentStream
                .ObserveOn(AddScheduler)
                .Subscribe(content =>
                {
                    ContentSubscription.Disposable = null;
                    // Let the last scheduled add operations finish, then execute swap
                    AddScheduler.Schedule(() => SwapContents(content));
                });
        }

        public virtual void Dispose()
        {
            StreamSubscription.Dispose();
            ContentSubscription.Dispose();
        }
    }
}
