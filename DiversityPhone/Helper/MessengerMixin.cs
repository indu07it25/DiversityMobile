namespace DiversityPhone.ViewModels
{
    using DiversityPhone.Model;
    using ReactiveUI;
    using System;
    using System.Diagnostics.Contracts;
    using System.Reactive.Linq;



    public static class MessengerMixin
    {
        public static IDisposable ToMessage<T>(this IObservable<T> This, IMessageBus Messenger, string messageContract = null)
        {
            Contract.Requires(This != null);
            Contract.Requires(Messenger != null);

            return This.Subscribe(x => Messenger.SendMessage(x, messageContract));
        }

        public static IDisposable RegisterPageFor<T>(this IMessageBus This, Page p, string contract = null)
        {
            return This.RegisterMessageSource(
                This.Listen<T>(contract)
                .Select(_ => p)
                );
        }
    }
}
