
namespace DiversityPhone.ViewModels
{
    public class ListeningCollection<TNotify, TItem> : AsyncLoadCollection<TItem>
    {
        public IChangeListener<TNotify, TItem> ChangeListener { get; private set; }

        public ListeningCollection(IUseBaseServices Services, IChangeListener<TNotify, TItem> ChangeListener)
            : base(Services.ThreadPool, Services.Dispatcher)
        {
            this.ChangeListener = ChangeListener;
            ChangeListener.ItemCollection = this;
        }
    }
}
