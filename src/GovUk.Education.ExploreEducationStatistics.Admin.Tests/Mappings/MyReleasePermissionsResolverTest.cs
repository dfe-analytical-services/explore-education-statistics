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
    public class MyReleasePermissionsResolverTest
    {
        [Fact]
        public void ResolvePermissions()
        {
            var release = new Release();

            var userService = new Mock<IUserService>(Strict);
            var resolver = new MyReleasePermissionsResolver(userService.Object);

            userService.Setup(s => s.MatchesPolicy(release, SecurityPolicies.CanUpdateSpecificRelease)).ReturnsAsync(true);
            userService.Setup(s => s.MatchesPolicy(release, SecurityPolicies.CanDeleteSpecificRelease)).ReturnsAsync(true);
            userService.Setup(s => s.MatchesPolicy(release, SecurityPolicies.CanAssignPrereleaseContactsToSpecificRelease)).ReturnsAsync(false);
            userService.Setup(s => s.MatchesPolicy(release, SecurityPolicies.CanMakeAmendmentOfSpecificRelease)).ReturnsAsync(false);

            var permissionsSet = resolver.Resolve(release, null, null, null);
            VerifyAllMocks(userService);

            Assert.True(permissionsSet.CanUpdateRelease);
            Assert.True(permissionsSet.CanDeleteRelease);
            Assert.False(permissionsSet.CanAddPrereleaseUsers);
            Assert.False(permissionsSet.CanMakeAmendmentOfRelease);
        }
    }
}
