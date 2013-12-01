using DiversityPhone.Interface;
using Microsoft.Phone.Data.Linq.Mapping;
using System;
using System.Data.Linq.Mapping;
using System.Text;


namespace DiversityPhone.Model
{
    [Table]
    [Index(Columns = "LastUsed", IsUnique = false, Name = "term_lastusage")]
    public class Analysis : IReadOnlyEntity
    {
        //Read-Only

        public static readonly DateTime DefaultLastUsed = new DateTime(2009, 01, 01); // DateTime.MinValue creates an overflow on insert.

        public Analysis()
        {
            this.LastUsed = DefaultLastUsed;
        }

        [Column(IsPrimaryKey = true)]
        public int AnalysisID { get; set; }

        [Column]
        public string DisplayText { get; set; }

        [Column]
        public string Description { get; set; }

        [Column]
        public string MeasurementUnit { get; set; }


        [Column]
        public DateTime LastUsed { get; set; }

        public string TextAndUnit
        {
            get
            {
                StringBuilder sb = new StringBuilder(DisplayText);
                if (!string.IsNullOrWhiteSpace(MeasurementUnit))
                {
                    sb.Append(" in ").Append(MeasurementUnit);
                }
                return sb.ToString();
            }
        }
    }
}
