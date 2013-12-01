using ReactiveUI.Xaml;

namespace DiversityPhone.ViewModels
{
    public interface IDeletePageVM : IPageServices
    {
        IReactiveCommand Delete { get; }
    }
}
