
namespace DiversityPhone.Model
{
    public enum ModificationState
    {
        New, // Persisted Nowhere
        Unmodified, // Persisted Remotely & Locally
        Modified // Persisted Locally
    }
    public interface IModifyable
    {
        /// <summary>
        /// Tracks the persistance status of an object.
        /// </summary>
        ModificationState ModificationState { get; set; }
    }
}
