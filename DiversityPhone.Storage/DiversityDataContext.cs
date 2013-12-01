using DiversityPhone.Model;
using System.Data.Linq;

namespace DiversityPhone.Services
{
    public class DiversityDataContext : DataContext
    {
        private static string GetCurrentProfileDBPath(ICurrentProfile Profile)
        {
            var profilePath = Profile.CurrentProfilePath();
            return string.Format("{0}/{1}", profilePath.Trim('/'), DiversityConstants.DB_FILENAME);
        }

        public DiversityDataContext(ICurrentProfile ProfileSvc)
            : this(GetCurrentProfileDBPath(ProfileSvc))
        {

        }

        public DiversityDataContext(
            string DatabaseFilePath
            )
            : base(string.Format("isostore:/{0}", DatabaseFilePath.TrimStart('/')))
        {
        }

        public Table<EventSeries> EventSeries;
        public Table<Localization> Localizations;

        public Table<Event> Events;
        public Table<EventProperty> EventProperties;

        public Table<Specimen> Specimen;

        public Table<IdentificationUnit> IdentificationUnits;
        public Table<IdentificationUnitAnalysis> IdentificationUnitAnalyses;

        public Table<MultimediaObject> MultimediaObjects;
    }
}
