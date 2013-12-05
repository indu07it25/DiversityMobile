using DiversityPhone.Interface;
using DiversityPhone.Model;
using System.Diagnostics;

namespace DiversityPhone.Storage
{
    public class EntityEditPolicy : IViewEditPolicy
    {
        public bool CanEdit<T>(T entity)
        {
            if (entity is IModifyable)
            {
                return (entity as IModifyable).ModificationState != ModificationState.Unmodified;
            }

            Debugger.Break();

            return false;
        }

        public bool CanView<T>(T entity)
        {
            if (entity is EventSeries)
            {
                return !(entity as EventSeries).IsNoEventSeries();
            }

            return true;
        }
    }
}
