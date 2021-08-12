#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Mappings;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityPolicies;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.MockUtils;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Mappings
{
    public class MyMethodologyPermissionSetPropertyResolverTest
    {
        [Fact]
        public void ResolvePermissions()
        {
            var methodology = new Methodology();

            var userService = new Mock<IUserService>(Strict);
            var resolver = new MyMethodologyPermissionSetPropertyResolver(userService.Object);

            userService.Setup(s => s.MatchesPolicy(methodology, CanApproveSpecificMethodology)).ReturnsAsync(true);
            userService.Setup(s => s.MatchesPolicy(methodology, CanUpdateSpecificMethodology)).ReturnsAsync(false);
            userService.Setup(s => s.MatchesPolicy(methodology, CanDeleteSpecificMethodology)).ReturnsAsync(true);
            userService.Setup(s => s.MatchesPolicy(methodology, CanMakeAmendmentOfSpecificMethodology))
                .ReturnsAsync(false);
            userService.Setup(s => s.MatchesPolicy(methodology, CanMarkSpecificMethodologyAsDraft)).ReturnsAsync(true);

            var permissionsSet = resolver.Resolve(methodology, null, null, null);
            VerifyAllMocks(userService);

            Assert.True(permissionsSet.CanApproveMethodology);
            Assert.False(permissionsSet.CanUpdateMethodology);
            Assert.True(permissionsSet.CanDeleteMethodology);
            Assert.False(permissionsSet.CanMakeAmendmentOfMethodology);
            Assert.True(permissionsSet.CanMarkMethodologyAsDraft);
        }
    }
}
