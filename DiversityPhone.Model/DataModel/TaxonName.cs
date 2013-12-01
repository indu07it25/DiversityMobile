

namespace DiversityPhone.Model
{
    using DiversityPhone.Interface;
    using System.Data.Linq.Mapping;

    [Table]
    public class TaxonName : IReadOnlyEntity
    {
        [Column(IsPrimaryKey = true)]
        public string URI { get; set; }

        [Column]
        public string TaxonNameCache { get; set; }

        [Column]
        public string TaxonNameSinAuth { get; set; }

        [Column]
        public string GenusOrSupragenic { get; set; }

        [Column]
        public string SpeciesEpithet { get; set; }

        [Column]
        public string InfraspecificEpithet { get; set; }

        [Column]
        public Synonymy Synonymy { get; set; }

        [Column]
        public string AcceptedNameURI { get; set; }

        [Column]
        public string AcceptedNameCache { get; set; }
    }
}
