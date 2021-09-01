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
    public class MyPublicationMethodologyPermissionsPropertyResolverTest
    {
        [Fact]
        public void ResolvePermissions()
        {
            var publicationMethodology = new PublicationMethodology();

            var userService = new Mock<IUserService>(Strict);
            var resolver = new MyPublicationMethodologyPermissionsPropertyResolver(userService.Object);

            userService.Setup(s =>
                    s.MatchesPolicy(publicationMethodology, CanDropMethodologyLink))
                .ReturnsAsync(true);

            var permissions = resolver.Resolve(publicationMethodology, null, null, null);
            VerifyAllMocks(userService);

            Assert.True(permissions.CanDropMethodology);
        }
    }
}
