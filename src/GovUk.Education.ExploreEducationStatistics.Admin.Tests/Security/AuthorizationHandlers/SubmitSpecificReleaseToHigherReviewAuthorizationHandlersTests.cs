using GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers.SubmitSpecificReleaseToHigherReviewAuthorizationHandler;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityClaimTypes;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.ReleaseAuthorizationHandlersTestUtil;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers
{
    public class SubmitSpecificReleaseToHigherReviewAuthorizationHandlersTests
    {
        [Fact]
        public void CanSubmitAllReleasesToHigherReviewAuthorizationHandler()
        {
            // Assert that any users with the "SubmitAllReleasesToHigherReview" claim can submit an arbitrary Release to higher review
            // (and no other claim allows this)
            AssertReleaseHandlerSucceedsWithCorrectClaims<SubmitSpecificReleaseToHigherReviewRequirement>(
                new CanSubmitAllReleasesToHigherReviewAuthorizationHandler(), SubmitAllReleasesToHigherReview);
        }
        
        [Fact]
        public void CanSubmitAllReleasesToHigherReviewAuthorizationHandler_ReleaseApproved()
        {
            var release = new Release
            {
                Status = ReleaseStatus.Approved
            };
            
            // Assert that no users can submit an approved Release to higher review
            AssertReleaseHandlerSucceedsWithCorrectClaims<SubmitSpecificReleaseToHigherReviewRequirement>(
                new CanSubmitAllReleasesToHigherReviewAuthorizationHandler(),
                release);
        }
        
        [Fact]
        public void HasEditorRoleOnReleaseAuthorizationHandler()
        {
            // Assert that a User who has the "Contributor", "Lead" or "Approver" role on a Release can submit the Release
            // to higher review
            // (and no other role)
            AssertReleaseHandlerSucceedsWithCorrectReleaseRoles<SubmitSpecificReleaseToHigherReviewRequirement>(
                contentDbContext => new HasEditorRoleOnReleaseAuthorizationHandler(contentDbContext),
                ReleaseRole.Contributor, ReleaseRole.Lead, ReleaseRole.Approver);
        }
        
        [Fact]
        public void HasEditorRoleOnReleaseAuthorizationHandler_ReleaseApproved()
        {
            var release = new Release
            {
                Status = ReleaseStatus.Approved
            };
            
            // Assert that no User can submit the Release to higher review if it is already Approved
            AssertReleaseHandlerSucceedsWithCorrectReleaseRoles<SubmitSpecificReleaseToHigherReviewRequirement>(
                contentDbContext => new HasEditorRoleOnReleaseAuthorizationHandler(contentDbContext),
                release);
        }
    }
}