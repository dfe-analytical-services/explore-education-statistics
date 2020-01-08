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
            AssertReleaseHandlerSucceedsWithCorrectClaims(
                new MarkSpecificReleaseAsDraftCanMarkAllReleasesAsDraftAuthorizationHandler(), MarkAllReleasesAsDraft);
        }
        
        [Fact]
        public void MarkSpecificReleaseAsDraftHasRoleOnReleaseAuthorizationHandler()
        {
            AssertReleaseHandlerSucceedsWithCorrectReleaseRoles(
                contentDbContext => new MarkSpecificReleaseAsDraftHasRoleOnReleaseAuthorizationHandler(contentDbContext),
                ReleaseRole.Contributor, ReleaseRole.Lead, ReleaseRole.Approver);
        }
    }
}