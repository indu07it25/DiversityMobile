namespace DiversityPhone.Model
{
    using System;

    public static class NoEventSeriesMixin
    {
        public static EventSeries NoEventSeries
        {
            get;
            private set;
        }

        public static bool IsNoEventSeries(this EventSeries This)
        {
            return This == NoEventSeries;
        }

        static NoEventSeriesMixin()
        {
            NoEventSeries = new EventSeries()
            {
                CollectionSeriesID = null,
                Description = DiversityResources.EventSeries_NoES_Header,
                ModificationState = ModificationState.Unmodified,
                SeriesCode = string.Empty,
                SeriesStart = DateTime.MinValue,
                SeriesEnd = DateTime.MinValue
            };
        }
    }
}
