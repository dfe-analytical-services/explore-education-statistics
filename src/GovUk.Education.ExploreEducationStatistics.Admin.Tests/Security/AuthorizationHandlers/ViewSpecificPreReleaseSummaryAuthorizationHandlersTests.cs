using GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers.
    ViewSpecificPreReleaseSummaryAuthorizationHandler;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityClaimTypes;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.Utils.
    ReleaseAuthorizationHandlersTestUtil;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers
{
    public class ViewSpecificPreReleaseSummaryAuthorizationHandlersTests
    {
        [Fact]
        public void CanSeeAllReleasesAuthorizationHandler()
        {
            // Assert that any users with the "AccessAllReleases" claim can view an arbitrary PreRelease Summary
            // (and no other claim allows this)
            AssertReleaseHandlerSucceedsWithCorrectClaims<ViewSpecificPreReleaseSummaryRequirement>(
                new CanSeeAllReleasesAuthorizationHandler(), AccessAllReleases);
        }

        [Fact]
        public void HasUnrestrictedViewerRoleOnReleaseAuthorizationHandler()
        {
            // Assert that a User who has any unrestricted viewer role on a Release can view the PreRelease Summary
            AssertReleaseHandlerSucceedsWithCorrectReleaseRoles<ViewSpecificPreReleaseSummaryRequirement>(
                contentDbContext => new HasUnrestrictedViewerRoleOnReleaseAuthorizationHandler(contentDbContext),
                ReleaseRole.Viewer, ReleaseRole.Lead, ReleaseRole.Contributor, ReleaseRole.Approver);
        }

        [Fact]
        public void HasPreReleaseRoleWithinAccessWindowAuthorizationHandler()
        {
            var release = new Release();

            // Assert that a User who specifically has the Pre Release role on a Release can view the PreRelease Summary
            AssertReleaseHandlerSucceedsWithCorrectReleaseRoles<ViewSpecificPreReleaseSummaryRequirement>(
                contentDbContext => new HasPreReleaseRoleOnReleaseAuthorizationHandler(contentDbContext),
                release,
                ReleaseRole.PrereleaseViewer);
        }
    }
}