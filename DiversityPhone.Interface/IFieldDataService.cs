namespace DiversityPhone.Interface
{
    using DiversityPhone.Model;
    using System.Collections.Generic;

    public interface IFieldDataService : IRepository
    {
        void ClearDatabase();

        IEnumerable<IElementVM> CollectModifications();
    }

    public static class FieldDataExtensions
    {
        public static IEnumerable<MultimediaObject> GetMultimediaForObject(this IFieldDataService This, IMultimediaOwner owner)
        {
            return This.Get<MultimediaObject>(mmo => mmo.OwnerType == owner.EntityType &&
                                                     mmo.RelatedId == owner.EntityID);

        }

        public static IEnumerable<MultimediaObject> GetNewMultimedia(this IFieldDataService This)
        {
            return This.Get<MultimediaObject>(mmo => mmo.CollectionURI == null);
        }

        public static IEnumerable<Event> GetEventsForSeries(this IFieldDataService This, EventSeries es)
        {
            //Workaround for the fact, that ev.SeriesID == es.SeriesID doesn't work for null values
            if (es.IsNoEventSeries())
            {
                return This.Get<Event>(x => x.SeriesID == null);
            }
            else
            {
                return This.Get<Event>(x => x.SeriesID == es.SeriesID);
            }
        }

        public static IEnumerable<IdentificationUnit> GetTopLevelIUForSpecimen(this IFieldDataService This, int specimenID)
        {
            return This.Get<IdentificationUnit>(x => x.SpecimenID == specimenID && x.RelatedUnitID == null);
        }

    }
}
