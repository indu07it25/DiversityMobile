using DiversityPhone.Interface;
using System;
using System.Linq.Expressions;

namespace DiversityPhone.Model
{
    public static class Queries
    {
        public static Expression<Func<MultimediaObject, bool>> Multimedia(this IMultimediaOwner owner)
        {
            return mmo => mmo.OwnerType == owner.EntityType && mmo.RelatedId == owner.EntityID;
        }

        public static Expression<Func<Event, bool>> Events(this EventSeries es)
        {
            if (es.IsNoEventSeries())
            {
                return ev => ev.SeriesID == null;
            }
            else
            {
                return ev => ev.SeriesID == es.SeriesID;
            }
        }

        public static Expression<Func<Localization, bool>> Localizations(this EventSeries es)
        {
            return gp => gp.RelatedID == es.SeriesID.Value;
        }

        public static Expression<Func<Specimen, bool>> Specimen(this Event ev)
        {
            return s => s.EventID == ev.EventID;
        }

        public static Expression<Func<EventProperty, bool>> Properties(this Event ev)
        {
            return p => p.EventID == ev.EventID;
        }

        public static Expression<Func<IdentificationUnit, bool>> ToplevelUnits(this Specimen s)
        {
            return iu => iu.SpecimenID == s.SpecimenID && iu.RelatedUnitID == null;
        }

        public static Expression<Func<IdentificationUnit, bool>> Units(this Specimen s)
        {
            return iu => iu.SpecimenID == s.SpecimenID;
        }

        public static Expression<Func<IdentificationUnit, bool>> SubUnits(this IdentificationUnit u)
        {
            return iu => iu.RelatedUnitID == u.UnitID;
        }

        public static Expression<Func<IdentificationUnitAnalysis, bool>> Analyses(this IdentificationUnit iu)
        {
            return a => a.UnitID == iu.UnitID;
        }
    }

}