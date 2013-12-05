
namespace DiversityPhone.Model
{
    public enum ModificationState
    {
        Unmodified = 1, // Persisted Remotely & Locally
        Modified = 2 // Persisted Locally
    }
    public interface IModifyable
    {
        /// <summary>
        /// Tracks the persistance status of an object.
        /// </summary>
        ModificationState ModificationState { get; set; }
    }
}
