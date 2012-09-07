﻿using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Data.Linq.Mapping;
using System.Collections.Generic;
using System.Linq;
using ReactiveUI;

namespace DiversityPhone.Model
{
    [Table]    
    public class TaxonList : ReactiveObject, IEquatable<TaxonList>
    {
        public TaxonList()
        {
            TableID = InvalidTableID;
        }

        [Column(IsPrimaryKey = true)]
        public int TableID { get; set; }

        [Column]
        public string TableName { get; set; }

        [Column]
        public string TableDisplayName { get; set; }

        [Column]
        public string TaxonomicGroup { get; set; }

        [Column]
        public bool IsPublicList { get; set; }

        private bool _IsSelected;
        [Column]
        public bool IsSelected
        {
            get { return _IsSelected; }
            set { this.RaiseAndSetIfChanged(x => x.IsSelected, ref _IsSelected, value); }
        }


        public static IEnumerable<int> ValidTableIDs
        {
            get
            {
                return Enumerable.Range(0, 100);
            }
        }

        public const int InvalidTableID = -1;

        public bool Equals(TaxonList other)
        {
            return this.TableName == other.TableName && this.TaxonomicGroup == other.TaxonomicGroup;
        }
    }
}
