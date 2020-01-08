using GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityClaimTypes;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.ReleaseAuthorizationHandlersTestUtil;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.PermissionTestUtil;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers
{
    public class MarkSpecificReleaseAsDraftAuthorizationHandlersTests
    {
        [Fact]
        public void MarkSpecificReleaseAsDraftCanMarkAllReleasesAsDraftAuthorizationHandler()
        {
            // Assert that any users with the "MarkAllReleasesAsDraft" claim can mark an arbitrary Release as draft
            // (and no other claim allows this)
            AssertReleaseHandlerSucceedsWithCorrectClaims(
                new MarkSpecificReleaseAsDraftCanMarkAllReleasesAsDraftAuthorizationHandler(), MarkAllReleasesAsDraft);
        }
        
        [Fact]
        public void MarkSpecificReleaseAsDraftHasRoleOnReleaseAuthorizationHandler()
        {
            // Assert that a User who has the "Contributor", "Lead" or "Approver" role on a Release can mark the Release
            // as Draft
            // (and no other role)
            AssertReleaseHandlerSucceedsWithCorrectReleaseRoles(
                contentDbContext => new MarkSpecificReleaseAsDraftHasRoleOnReleaseAuthorizationHandler(contentDbContext),
                ReleaseRole.Contributor, ReleaseRole.Lead, ReleaseRole.Approver);
        }
    }
}