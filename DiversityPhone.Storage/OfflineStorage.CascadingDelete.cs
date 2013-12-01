using DiversityPhone.Interface;
using DiversityPhone.Model;

namespace DiversityPhone.Services
{

    public class CascadingDeleter : IDeletePolicy
    {
        readonly IStoreMultimedia MultimediaStore;

        public CascadingDeleter(IStoreMultimedia Multimedia)
        {
            MultimediaStore = Multimedia;
        }

        public void Enforce<T>(IDeleteOperation operation, T deletee) where T : class, IReadOnlyEntity
        {
            if (typeof(T) == typeof(EventSeries))
            {
                deleteSeries(operation, deletee as EventSeries);
            }
            else if (typeof(T) == typeof(Event))
            {
                deleteEvent(operation, deletee as Event);
            }
            else if (typeof(T) == typeof(Specimen))
            {
                deleteSpecimen(operation, deletee as Specimen);
            }
            else if (typeof(T) == typeof(IdentificationUnit))
            {
                deleteUnit(operation, deletee as IdentificationUnit);
            }
            else if (typeof(T) == typeof(MultimediaObject))
            {
                deleteMMO(operation, deletee as MultimediaObject);
            }

            //Nothing more to do for this type
        }

        private void deleteSeries(IDeleteOperation operation, EventSeries es)
        {
            operation.Delete(es.Events());
            operation.Delete(es.Localizations());
        }

        private void deleteEvent(IDeleteOperation operation, Event ev)
        {
            operation.Delete<Specimen>(Queries.Specimen(ev));
            operation.Delete<EventProperty>(Queries.Properties(ev));
            operation.Delete<MultimediaObject>(Queries.Multimedia(ev));
        }

        private void deleteSpecimen(IDeleteOperation operation, Specimen spec)
        {
            operation.Delete(Queries.Units(spec));
            operation.Delete(Queries.Multimedia(spec));
        }

        private void deleteUnit(IDeleteOperation operation, IdentificationUnit iu)
        {
            operation.Delete(Queries.Analyses(iu));
            operation.Delete(Queries.Multimedia(iu));
        }

        private void deleteMMO(IDeleteOperation operation, MultimediaObject mmo)
        {
            MultimediaStore.DeleteMultimedia(mmo.Uri);
        }
    }
}
