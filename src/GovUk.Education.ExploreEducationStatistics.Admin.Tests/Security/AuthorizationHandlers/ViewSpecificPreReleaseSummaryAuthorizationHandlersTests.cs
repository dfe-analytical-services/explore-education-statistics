using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers.ViewSpecificPreReleaseSummaryAuthorizationHandler;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityClaimTypes;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.Utils.ReleaseAuthorizationHandlersTestUtil;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.ReleaseRole;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers
{
    public class ViewSpecificPreReleaseSummaryAuthorizationHandlersTests
    {
        [Fact]
        public async Task CanSeeAllReleasesAuthorizationHandler()
        {
            // Assert that any users with the "AccessAllReleases" claim can view an arbitrary PreRelease Summary
            // (and no other claim allows this)
            await AssertReleaseHandlerSucceedsWithCorrectClaims<ViewSpecificPreReleaseSummaryRequirement>(
                new CanSeeAllReleasesAuthorizationHandler(), AccessAllReleases);
        }

        [Fact]
        public async Task HasUnrestrictedViewerRoleOnReleaseAuthorizationHandler()
        {
            // Assert that a User who has any unrestricted viewer role on a Release can view the PreRelease Summary
            await AssertReleaseHandlerSucceedsWithCorrectReleaseRoles<ViewSpecificPreReleaseSummaryRequirement>(
                contentDbContext =>
                    new HasUnrestrictedViewerRoleOnReleaseAuthorizationHandler(
                        new AuthorizationHandlerResourceRoleService(
                            new UserReleaseRoleRepository(contentDbContext),
                            new UserPublicationRoleRepository(contentDbContext),
                            Mock.Of<IPublicationRepository>(Strict))),
                Viewer, Lead, Contributor, Approver);
        }

        [Fact]
        public async Task HasPreReleaseRoleWithinAccessWindowAuthorizationHandler()
        {
            // Assert that a User who specifically has the Pre Release role on a Release can view the PreRelease Summary
            await AssertReleaseHandlerSucceedsWithCorrectReleaseRoles<ViewSpecificPreReleaseSummaryRequirement>(
                contentDbContext =>
                    new HasPreReleaseRoleOnReleaseAuthorizationHandler(
                        new AuthorizationHandlerResourceRoleService(
                            new UserReleaseRoleRepository(contentDbContext),
                            new UserPublicationRoleRepository(contentDbContext),
                            Mock.Of<IPublicationRepository>(Strict))),
                PrereleaseViewer);
        }
    }
}
