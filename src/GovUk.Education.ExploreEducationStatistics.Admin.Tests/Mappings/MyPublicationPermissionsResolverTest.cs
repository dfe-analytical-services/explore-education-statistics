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
    public class MyPublicationPermissionsResolverTest
    {
        [Fact]
        public void ResolvePermissions()
        {
            var publication = new Publication();

            var userService = new Mock<IUserService>(Strict);
            var resolver = new MyPublicationPermissionsResolver(userService.Object);

            userService.Setup(s => s.MatchesPolicy(publication, CanUpdateSpecificPublication))
                .ReturnsAsync(true);
            userService.Setup(s => s.MatchesPolicy(CanUpdatePublicationTitles))
                .ReturnsAsync(true);
            userService.Setup(s => s.MatchesPolicy(publication, CanCreateReleaseForSpecificPublication))
                .ReturnsAsync(true);
            userService.Setup(s => s.MatchesPolicy(publication, CanAdoptMethodologyForSpecificPublication))
                .ReturnsAsync(true);
            userService.Setup(s => s.MatchesPolicy(publication, CanCreateMethodologyForSpecificPublication))
                .ReturnsAsync(false);
            userService.Setup(s => s.MatchesPolicy(publication, CanManageExternalMethodologyForSpecificPublication))
                .ReturnsAsync(false);

            var permissionsSet = resolver.Resolve(publication, null, null, null);
            VerifyAllMocks(userService);

            Assert.True(permissionsSet.CanUpdatePublication);
            Assert.True(permissionsSet.CanUpdatePublicationTitle);
            Assert.True(permissionsSet.CanCreateReleases);
            Assert.True(permissionsSet.CanAdoptMethodologies);
            Assert.False(permissionsSet.CanCreateMethodologies);
            Assert.False(permissionsSet.CanManageExternalMethodology);
        }
    }
}
