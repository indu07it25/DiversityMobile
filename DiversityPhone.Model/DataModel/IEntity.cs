using DiversityPhone.Model;
namespace DiversityPhone.Interface
{
    public interface IReadOnlyEntity
    {
    }

    public interface IWriteableEntity : IReadOnlyEntity
    {
        int? EntityID { get; set; }
    }

    public interface IMappedEntity : IWriteableEntity
    {
        DBObjectType EntityType { get; }
    }

    public interface IMultimediaOwner : IMappedEntity
    {

    }

    public interface ILocationOwner : IMappedEntity
    {
    }

    public static class EntityExtensions
    {
        public static bool IsNew(this IWriteableEntity This)
        {
            return !This.EntityID.HasValue;
        }
    }
}