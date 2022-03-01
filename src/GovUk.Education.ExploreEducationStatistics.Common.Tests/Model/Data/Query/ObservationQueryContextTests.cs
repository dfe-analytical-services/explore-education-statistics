#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.TimeIdentifier;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Model.Data.Query
{
    public class ObservationQueryContextTests
    {
        [Fact]
        public void Clone_CreatesDeepCopy()
        {
            var subjectId = Guid.NewGuid();
            var indicatorId = Guid.NewGuid();
            var filterItemId = Guid.NewGuid();
            var locationId = Guid.NewGuid();

            var original = new ObservationQueryContext
            {
                SubjectId = subjectId,
                BoundaryLevel = 1,
                IncludeGeoJson = false,
                Filters = ListOf(filterItemId),
                Indicators = ListOf(indicatorId),
                LocationIds = ListOf(locationId),
                Locations = new LocationQuery
                {
                    Country = ListOf("original"),
                    EnglishDevolvedArea = ListOf("original"),
                    Institution = ListOf("original"),
                    LocalAuthority = ListOf("original"),
                    LocalAuthorityDistrict = ListOf("original"),
                    LocalEnterprisePartnership = ListOf("original"),
                    MultiAcademyTrust = ListOf("original"),
                    MayoralCombinedAuthority = ListOf("original"),
                    OpportunityArea = ListOf("original"),
                    ParliamentaryConstituency = ListOf("original"),
                    Provider = ListOf("original"),
                    PlanningArea = ListOf("original"),
                    Region = ListOf("original"),
                    RscRegion = ListOf("original"),
                    School = ListOf("original"),
                    Sponsor = ListOf("original"),
                    Ward = ListOf("original")
                },
                TimePeriod = new TimePeriodQuery
                {
                    StartCode = CalendarYear,
                    StartYear = 2022,
                    EndCode = CalendarYear,
                    EndYear = 2022
                }
            };

            var clone = original.Clone();

            clone.AssertDeepEqualTo(original);

            clone.SubjectId = Guid.NewGuid();
            clone.BoundaryLevel = 2;
            clone.IncludeGeoJson = true;
            clone.Filters = new List<Guid>();
            clone.Indicators = new List<Guid>();
            clone.LocationIds = new List<Guid>();
            clone.Locations.Country![0] = "updated";
            clone.Locations.EnglishDevolvedArea![0] = "updated";
            clone.Locations.Institution![0] = "updated";
            clone.Locations.LocalAuthority![0] = "updated";
            clone.Locations.LocalAuthorityDistrict![0] = "updated";
            clone.Locations.LocalEnterprisePartnership![0] = "updated";
            clone.Locations.MultiAcademyTrust![0] = "updated";
            clone.Locations.MayoralCombinedAuthority![0] = "updated";
            clone.Locations.ParliamentaryConstituency![0] = "updated";
            clone.Locations.Provider![0] = "updated";
            clone.Locations.PlanningArea![0] = "updated";
            clone.Locations.Region![0] = "updated";
            clone.Locations.RscRegion![0] = "updated";
            clone.Locations.School![0] = "updated";
            clone.Locations.Sponsor![0] = "updated";
            clone.Locations.Ward![0] = "updated";
            clone.TimePeriod.StartCode = AcademicYear;
            clone.TimePeriod.StartYear = 2023;
            clone.TimePeriod.EndCode = AcademicYear;
            clone.TimePeriod.EndYear = 2023;

            Assert.Equal(subjectId, original.SubjectId);
            Assert.Equal(1, original.BoundaryLevel);
            Assert.False(original.IncludeGeoJson);
            Assert.Equal(filterItemId, original.Filters.ToList()[0]);
            Assert.Equal(indicatorId, original.Indicators.ToList()[0]);
            Assert.Equal(locationId, original.LocationIds.ToList()[0]);
            Assert.Equal("original", original.Locations.Country[0]);
            Assert.Equal("original", original.Locations.EnglishDevolvedArea[0]);
            Assert.Equal("original", original.Locations.Institution[0]);
            Assert.Equal("original", original.Locations.LocalAuthority[0]);
            Assert.Equal("original", original.Locations.LocalAuthorityDistrict[0]);
            Assert.Equal("original", original.Locations.LocalEnterprisePartnership[0]);
            Assert.Equal("original", original.Locations.MultiAcademyTrust[0]);
            Assert.Equal("original", original.Locations.MayoralCombinedAuthority[0]);
            Assert.Equal("original", original.Locations.ParliamentaryConstituency[0]);
            Assert.Equal("original", original.Locations.Provider[0]);
            Assert.Equal("original", original.Locations.PlanningArea[0]);
            Assert.Equal("original", original.Locations.Region[0]);
            Assert.Equal("original", original.Locations.RscRegion[0]);
            Assert.Equal("original", original.Locations.School[0]);
            Assert.Equal("original", original.Locations.Sponsor[0]);
            Assert.Equal("original", original.Locations.Ward[0]);
            Assert.Equal(CalendarYear, original.TimePeriod.StartCode);
            Assert.Equal(2022, original.TimePeriod.StartYear);
            Assert.Equal(CalendarYear, original.TimePeriod.EndCode);
            Assert.Equal(2022, original.TimePeriod.EndYear);
        }
    }
}
