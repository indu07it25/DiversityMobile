namespace DiversityPhone.Interface
{
    public interface IViewEditPolicy
    {
        bool CanView<T>(T entity);
        bool CanEdit<T>(T entity);
    }
}
