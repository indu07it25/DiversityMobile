using System.Data.Linq.Mapping;


namespace DiversityPhone.Model
{
    [Table]
    public class AnalysisResult
    {
        //Read-Only
        [Column(IsPrimaryKey = true)]
        public int AnalysisID { get; set; }

        [Column(IsPrimaryKey = true)]
        public string Result { get; set; }

        [Column]
        public string Description { get; set; }

        [Column]
        public string Notes { get; set; }

        [Column]
        public string DisplayText { get; set; }
    }
}
