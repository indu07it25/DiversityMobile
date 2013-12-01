


using System;
using System.Linq;
using ReactiveUI;
using Microsoft.Phone.Data.Linq.Mapping;
using System.Data.Linq; 
using System.Data.Linq.Mapping;
using DiversityPhone;
using DiversityPhone.Interface;

namespace DiversityPhone.Model
{	
	[Table]
	public class EventSeries : ReactiveObject, IMappedEntity, ILocationOwner, IModifyable
	{
#pragma warning disable 0169
		[Column(IsVersion = true)]
		private Binary version;
#pragma warning restore 0169

		
		private int? _SeriesID;
		[Column(IsPrimaryKey=true,IsDbGenerated=true)]
		public int? SeriesID
		{
			get { return _SeriesID; }
			set 
			{
				

				if (_SeriesID != value)
				{
					this.raisePropertyChanging("SeriesID");
					_SeriesID = value;
					this.raisePropertyChanged("SeriesID");
				}  
			}
		}
				
		private int? _CollectionSeriesID;
		[Column(CanBeNull=true)]
		public int? CollectionSeriesID
		{
			get { return _CollectionSeriesID; }
			set 
			{
				

				if (_CollectionSeriesID != value)
				{
					this.raisePropertyChanging("CollectionSeriesID");
					_CollectionSeriesID = value;
					this.raisePropertyChanged("CollectionSeriesID");
				}  
			}
		}
		   

		
		private DateTime _SeriesStart;
		[Column]
		public DateTime SeriesStart
		{
			get { return _SeriesStart; }
			set 
			{
				
				var minSQLCEDate = new DateTime(1753, 01, 01);
				var maxSQLCEDate = new DateTime(9999, 12, 31);
				if (value < minSQLCEDate)
					value = minSQLCEDate;
				if (value > maxSQLCEDate)
					value = maxSQLCEDate;
				

				if (_SeriesStart != value)
				{
					this.raisePropertyChanging("SeriesStart");
					_SeriesStart = value;
					this.raisePropertyChanged("SeriesStart");
				}  
			}
		}
		  
		
		private DateTime? _SeriesEnd;
		[Column(CanBeNull=true,UpdateCheck=UpdateCheck.Never)]
		public DateTime? SeriesEnd
		{
			get { return _SeriesEnd; }
			set 
			{
				

				if (_SeriesEnd != value)
				{
					this.raisePropertyChanging("SeriesEnd");
					_SeriesEnd = value;
					this.raisePropertyChanged("SeriesEnd");
				}  
			}
		}
		     
		
		private string _Description;
		[Column]
		public string Description
		{
			get { return _Description; }
			set 
			{
				

				if (_Description != value)
				{
					this.raisePropertyChanging("Description");
					_Description = value;
					this.raisePropertyChanged("Description");
				}  
			}
		}
				
		private string _SeriesCode;
		[Column]
		public string SeriesCode
		{
			get { return _SeriesCode; }
			set 
			{
				

				if (_SeriesCode != value)
				{
					this.raisePropertyChanging("SeriesCode");
					_SeriesCode = value;
					this.raisePropertyChanged("SeriesCode");
				}  
			}
		}
		   
 
		
		private ModificationState _ModificationState;
		[Column]
		public ModificationState ModificationState
		{
			get { return _ModificationState; }
			set 
			{
				

				if (_ModificationState != value)
				{
					this.raisePropertyChanging("ModificationState");
					_ModificationState = value;
					this.raisePropertyChanged("ModificationState");
				}  
			}
		}
		 

        public EventSeries()
        {
            this.Description = string.Empty;
            this.SeriesCode = string.Empty;
            this.SeriesStart = DateTime.Now;
            this.SeriesEnd = null;
            this.SeriesID = 0;
            this.ModificationState = ModificationState.New;            
        }

		public DBObjectType EntityType
        {
            get { return DBObjectType.EventSeries; }
        }

        public int? EntityID
        {
            get { return SeriesID; }
			set { SeriesID = value; }
        }

		public int? MappedID
        {
            get { return CollectionSeriesID; }
			set { CollectionSeriesID = value; }
        }

    }	
} 