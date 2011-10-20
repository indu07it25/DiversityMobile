﻿namespace DiversityPhone.Model
{
    using System;
    using System.Collections.Generic;
    using System.Data.Linq.Mapping;
    using System.Linq;
    using System.Text;
    using Microsoft.Phone.Data.Linq.Mapping;

    [Table]
    [Index(Columns = "CollectionDate", IsUnique = false, Name = "event_collectiondate")]
    public class Event 
    {
        public Event()
        {
            this.CollectionDate = DateTime.Now;
            this.IsModified = null;
        }

        [Column(IsPrimaryKey = true)]
        public int EventID { get; set; }

        [Column]        
        public int SeriesID { get; set; }

        [Column]
        public DateTime CollectionDate { get; set; }

        [Column]
        public string LocalityDescription { get; set; }

        [Column]
        public string HabitatDescription { get; set; }

        [Column]
        public bool? IsModified { get; set; }        
    }
}