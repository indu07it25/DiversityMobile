﻿using System;
using System.Linq;
using Xunit;

namespace DiversityService.Test
{
    [Trait("Service", "Download")]
    public class DownloadTest
    {
        readonly ServiceReference.DiversityServiceClient Target;

        public DownloadTest()
        {
            Target = new ServiceReference.DiversityServiceClient();
        }

        [Fact]
        public void SeriesShouldRetrieveCorrectInfo()
        {
            var series = Target.EventSeriesByID(TestResources.SeriesID, TestResources.Credentials);

            Assert.Equal(TestResources.SeriesID, series.CollectionEventSeriesID);
            Assert.Equal("TestDescription", series.Description);
            Assert.Equal("TestCode", series.SeriesCode);
            Assert.Equal(DateTime.Parse("2013-03-22 15:19:09.410"), series.SeriesStart);
            Assert.Equal(DateTime.Parse("2013-03-23 15:19:09.413"), series.SeriesEnd);
        }

        [Fact]        
        public void EventShouldRetrieveCorrectInfo()
        {
            var ev = Target.EventsByLocality("TestLocality", TestResources.Credentials).Single();

            Assert.Equal(1, ev.CollectionEventID);
            Assert.Equal(TestResources.SeriesID, ev.CollectionSeriesID);
            Assert.Equal(DateTime.Parse("2013-03-23 00:00:00.000"), ev.CollectionDate);
            Assert.Equal("TestLocality", ev.LocalityDescription);
            Assert.Equal("TestHabitat", ev.HabitatDescription);
        }

        [Fact]
        public void SpecimenRetrievesCorrectInfo()
        {
            var spec = Target.SpecimenForEvent(TestResources.EventID, TestResources.Credentials).Where(s => s.CollectionSpecimenID == TestResources.SpecimenID).Single();

            Assert.Equal(TestResources.EventID, spec.CollectionEventID);
            Assert.Equal("TestAccession", spec.AccessionNumber);
        }

        [Fact]
        public void IdentificationUnitRetrievesCorrectInfo()
        {
            var iu = Target.UnitsForSpecimen(TestResources.SpecimenID, TestResources.Credentials).Where(u => u.CollectionUnitID == TestResources.UnitID).Single();

            Assert.Equal(TestResources.SpecimenID, iu.CollectionSpecimenID);
            Assert.Equal("TestIdentification", iu.LastIdentificationCache);
            Assert.Equal("TestURI", iu.IdentificationUri);
            Assert.Equal(DateTime.Parse("2013-03-23 00:00:00.000"), iu.AnalysisDate);
            Assert.Null(iu.CollectionRelatedUnitID);
            Assert.Null(iu.RelationType);
            Assert.Equal(true, iu.OnlyObserved);
            Assert.Null(iu.Altitude);
            Assert.Null(iu.Latitude);
            Assert.Null(iu.Longitude);            
            Assert.Equal("?", iu.Qualification);
            Assert.Equal("plant", iu.TaxonomicGroup);
        }

    }
}
