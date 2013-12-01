namespace DiversityPhone.Interface
{
    public interface IEditPolicy
    {
        bool CanEdit<T>(T entity);
    }
}
