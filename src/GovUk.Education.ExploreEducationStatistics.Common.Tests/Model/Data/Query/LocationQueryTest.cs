#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data.Query;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Model.Data.Query
{
    public class LocationQueryTest
    {
        [Fact]
        public void CountItems_HandlesNullCollections()
        {
            var locationQuery = new LocationQuery();
            Assert.Equal(0, locationQuery.CountItems());
        }

        [Fact]
        public void CountItems_CountsAllCollections()
        {
            var locationQuery = new LocationQuery
            {
                Country = AsList("code1", "code2"),
                EnglishDevolvedArea = AsList("code1", "code2"),
                Institution = AsList("code1", "code2"),
                LocalAuthority = AsList("code1", "code2"),
                LocalAuthorityDistrict = AsList("code1", "code2"),
                LocalEnterprisePartnership = AsList("code1", "code2"),
                MultiAcademyTrust = AsList("code1", "code2"),
                MayoralCombinedAuthority = AsList("code1", "code2"),
                OpportunityArea = AsList("code1", "code2"),
                ParliamentaryConstituency = AsList("code1", "code2"),
                Provider = AsList("code1", "code2"),
                PlanningArea = AsList("code1", "code2"),
                Region = AsList("code1", "code2"),
                RscRegion = AsList("code1", "code2"),
                School = AsList("code1", "code2"),
                Sponsor = AsList("code1", "code2"),
                Ward = AsList("code1", "code2")
            };
            Assert.Equal(34, locationQuery.CountItems());
        }
    }
}
