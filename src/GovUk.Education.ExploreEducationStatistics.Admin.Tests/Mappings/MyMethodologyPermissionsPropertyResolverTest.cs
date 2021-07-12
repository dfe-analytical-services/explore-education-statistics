using GovUk.Education.ExploreEducationStatistics.Admin.Mappings;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Mappings
{
    public class MyMethodologyPermissionsPropertyResolverTest
    {
        [Fact]
        public void ResolvePermissions()
        {
            var methodology = new Methodology();
            
            var userService = new Mock<IUserService>(Strict);
            var resolver = new MyMethodologyPermissionSetPropertyResolver(userService.Object);
            
            userService.Setup(s => s.MatchesPolicy(methodology, SecurityPolicies.CanUpdateSpecificMethodology)).ReturnsAsync(true);
            userService.Setup(s => s.MatchesPolicy(methodology, SecurityPolicies.CanMakeAmendmentOfSpecificMethodology)).ReturnsAsync(false);
            userService.Setup(s => s.MatchesPolicy(methodology, SecurityPolicies.CanDeleteSpecificMethodology)).ReturnsAsync(true);

            var permissionsSet = resolver.Resolve(methodology, null, null, null);
            VerifyAllMocks(userService);
            
            Assert.True(permissionsSet.CanUpdateMethodology);
            Assert.False(permissionsSet.CanMakeAmendmentOfMethodology);
            Assert.True(permissionsSet.CanDeleteMethodology);
        }
    }
}