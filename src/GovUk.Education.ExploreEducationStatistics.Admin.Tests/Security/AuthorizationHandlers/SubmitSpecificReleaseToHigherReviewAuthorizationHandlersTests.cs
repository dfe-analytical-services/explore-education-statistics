using GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityClaimTypes;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.ReleaseAuthorizationHandlersTestUtil;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers
{
    public class SubmitSpecificReleaseToHigherReviewAuthorizationHandlersTests
    {
        [Fact]
        public void SubmitSpecificReleaseToHigherReviewCanSubmitAllReleasesAuthorizationHandler()
        {
            // Assert that any users with the "SubmitAllReleasesToHigherReview" claim can submit an arbitrary Release to higher review
            // (and no other claim allows this)
            AssertReleaseHandlerSucceedsWithCorrectClaims<SubmitSpecificReleaseToHigherReviewRequirement>(
                new SubmitSpecificReleaseToHigherReviewCanSubmitAllReleasesAuthorizationHandler(), SubmitAllReleasesToHigherReview);
        }
        
        [Fact]
        public void SubmitSpecificReleaseToHigherReviewHasRoleOnReleaseAuthorizationHandler()
        {
            // Assert that a User who has the "Contributor", "Lead" or "Approver" role on a Release can submit the Release
            // to higher review
            // (and no other role)
            AssertReleaseHandlerSucceedsWithCorrectReleaseRoles<SubmitSpecificReleaseToHigherReviewRequirement>(
                contentDbContext => new SubmitSpecificReleaseToHigherReviewHasRoleOnReleaseAuthorizationHandler(contentDbContext),
                ReleaseRole.Contributor, ReleaseRole.Lead, ReleaseRole.Approver);
        }
    }
}