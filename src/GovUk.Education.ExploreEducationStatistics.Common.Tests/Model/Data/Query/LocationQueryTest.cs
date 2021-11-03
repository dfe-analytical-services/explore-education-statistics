#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Model.Data.Query
{
    public class LocationQueryTest
    {
        [Fact]
        public void CountItems_QueryIsEmpty()
        {
            var locationQuery = new LocationQuery();
            Assert.Equal(0, locationQuery.CountItems());
        }

        [Fact]
        public void CountItems()
        {
            var locationQuery = new LocationQuery();
            locationQuery.Country.Add("1");
            locationQuery.EnglishDevolvedArea.Add("2");
            locationQuery.Institution.Add("3");
            locationQuery.LocalAuthority.Add("4");
            locationQuery.LocalAuthorityDistrict.Add("5");
            locationQuery.LocalEnterprisePartnership.Add("6");
            locationQuery.MultiAcademyTrust.Add("7");
            locationQuery.MayoralCombinedAuthority.Add("8");
            locationQuery.OpportunityArea.Add("9");
            locationQuery.ParliamentaryConstituency.Add("10");
            locationQuery.Provider.Add("11");
            locationQuery.PlanningArea.Add("12");
            locationQuery.Region.Add("13");
            locationQuery.RscRegion.Add("14");
            locationQuery.School.Add("15");
            locationQuery.Sponsor.Add("16");
            locationQuery.Ward.Add("17");
            Assert.Equal(17, locationQuery.CountItems());
        }
    }
}
