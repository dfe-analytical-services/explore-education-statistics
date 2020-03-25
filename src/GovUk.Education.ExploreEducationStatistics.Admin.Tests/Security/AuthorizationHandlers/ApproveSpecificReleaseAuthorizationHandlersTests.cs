using GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers.ApproveSpecificReleaseAuthorizationHandler;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityClaimTypes;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.ReleaseAuthorizationHandlersTestUtil;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers
{
    public class ApproveSpecificReleaseAuthorizationHandlersTests
    {
        [Fact]
        public void CanApproveAllReleasesAuthorizationHandler()
        {
            // Assert that any users with the "ApproveAllReleases" claim can approve an arbitrary Release
            // (and no other claim allows this)
            AssertReleaseHandlerSucceedsWithCorrectClaims<ApproveSpecificReleaseRequirement>(
                new CanApproveAllReleasesAuthorizationHandler(), ApproveAllReleases);
        }
        
        [Fact]
        public void CanApproveAllReleasesAuthorizationHandler_ReleaseApproved()
        {
            var release = new Release
            {
                Status = ReleaseStatus.Approved
            };
            
            // Assert that no users can approve an approved Release
            AssertReleaseHandlerSucceedsWithCorrectClaims<ApproveSpecificReleaseRequirement>(
                new CanApproveAllReleasesAuthorizationHandler(),
                release);
        }
        
        [Fact]
        public void HasApproverRoleOnReleaseAuthorizationHandler()
        {
            // Assert that a User who has the "Approver" role on a Release can approve the Release
            // (and no other role)
            AssertReleaseHandlerSucceedsWithCorrectReleaseRoles<ApproveSpecificReleaseRequirement>(
                contentDbContext => new HasApproverRoleOnReleaseAuthorizationHandler(contentDbContext), 
                ReleaseRole.Approver);
        }
        
        [Fact]
        public void HasApproverRoleOnReleaseAuthorizationHandler_ReleaseApproved()
        {
            var release = new Release
            {
                Status = ReleaseStatus.Approved
            };
            
            // Assert that no User can approve the Release if it is already Approved
            AssertReleaseHandlerSucceedsWithCorrectReleaseRoles<ApproveSpecificReleaseRequirement>(
                contentDbContext => new HasApproverRoleOnReleaseAuthorizationHandler(contentDbContext),
                release);
        }
    }
}