using ReactiveUI;
using System.Windows.Input;

namespace DiversityPhone.ViewModels
{
    public interface ISavePageVM : IReactiveNotifyPropertyChanged
    {
        ICommand Save { get; }
        ICommand ToggleEditable { get; }
        bool IsEditable { get; }
    }
}
