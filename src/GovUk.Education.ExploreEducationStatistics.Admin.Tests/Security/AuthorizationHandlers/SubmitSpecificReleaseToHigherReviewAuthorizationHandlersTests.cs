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
            AssertReleaseHandlerSucceedsWithCorrectClaims(
                new SubmitSpecificReleaseToHigherReviewCanSubmitAllReleasesAuthorizationHandler(), SubmitAllReleasesToHigherReview);
        }
        
        [Fact]
        public void SubmitSpecificReleaseToHigherReviewHasRoleOnReleaseAuthorizationHandler()
        {
            AssertReleaseHandlerSucceedsWithCorrectReleaseRoles(
                contentDbContext => new SubmitSpecificReleaseToHigherReviewHasRoleOnReleaseAuthorizationHandler(contentDbContext),
                ReleaseRole.Contributor, ReleaseRole.Lead, ReleaseRole.Approver);
        }
    }
}