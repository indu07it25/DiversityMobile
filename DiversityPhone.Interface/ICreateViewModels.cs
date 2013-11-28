
namespace DiversityPhone.Interface
{
    public interface ICreateViewModels
    {
        IElementVM<T> CreateVM<T>(T model);
    }
}
