using GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityClaimTypes;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.ReleaseAuthorizationHandlersTestUtil;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers
{
    public class ApproveSpecificReleaseAuthorizationHandlersTests
    {
        [Fact]
        public void ApproveSpecificReleaseCanApproveAllReleasesAuthorizationHandler()
        {
            // Assert that any users with the "ApproveAllReleases" claim can approve an arbitrary Release
            // (and no other claim allows this)
            AssertReleaseHandlerSucceedsWithCorrectClaims<ApproveSpecificReleaseRequirement>(
                new ApproveSpecificReleaseCanApproveAllReleasesAuthorizationHandler(), ApproveAllReleases);
        }
        
        [Fact]
        public void ApproveSpecificReleaseHasApproverRoleOnReleaseAuthorizationHandler()
        {
            // Assert that a User who has the "Approver" role on a Release can approve the Release
            // (and no other role)
            AssertReleaseHandlerSucceedsWithCorrectReleaseRoles<ApproveSpecificReleaseRequirement>(
                contentDbContext => new ApproveSpecificReleaseHasApproverRoleOnReleaseAuthorizationHandler(contentDbContext), 
                ReleaseRole.Approver);
        }
    }
}