using DiversityPhone.Interface;
using DiversityPhone.Model;

namespace DiversityPhone.Storage
{
    public class EntityEditPolicy : IEditPolicy
    {
        public bool CanEdit<T>(T entity)
        {
            if (entity is Event)
            {
                var ent = entity as IMappedEntity;
                ent.
            }
        }
    }
}
