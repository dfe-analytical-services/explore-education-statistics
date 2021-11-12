#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;

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
            var locationQuery = new LocationQuery
            {
                Country = AsList("code"),
                EnglishDevolvedArea = AsList("code"),
                Institution = AsList("code"),
                LocalAuthority = AsList("code"),
                LocalAuthorityDistrict = AsList("code"),
                LocalEnterprisePartnership = AsList("code"),
                MultiAcademyTrust = AsList("code"),
                MayoralCombinedAuthority = AsList("code"),
                OpportunityArea = AsList("code"),
                ParliamentaryConstituency = AsList("code"),
                Provider = AsList("code"),
                PlanningArea = AsList("code"),
                Region = AsList("code"),
                RscRegion = AsList("code"),
                School = AsList("code"),
                Sponsor = AsList("code"),
                Ward = AsList("code")
            };
            Assert.Equal(17, locationQuery.CountItems());
        }
    }
}
