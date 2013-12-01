using DiversityPhone.Interface;
using DiversityPhone.Model;
using System;
using System.Reactive.Linq;

namespace DiversityPhone.ViewModels
{
    public class RefreshVocabularyVM
    {
        public RefreshVocabularyVM(
            OnlineVMServices Services,
            Func<IRefreshVocabularyTask> refreshVocabluaryTaskFactory
            )
        {
            Services.Activation.OnActivation()
                .SelectMany(_ => Services.Credentials.CurrentCredentials().Where(cred => cred != null).FirstAsync())
                .TakeUntil(Services.Activation.OnDeactivation())
                .Subscribe(login =>
                {
                    var refreshTask = refreshVocabluaryTaskFactory();
                    refreshTask.Start(login)
                        .Subscribe(_ => { }, () =>
                        {
                            Services.Messenger.SendMessage<EventMessage>(EventMessage.Default, MessageContracts.INIT);
                            Services.Messenger.SendMessage<Page>(Page.Home);
                        });
                });

        }

    }
}
