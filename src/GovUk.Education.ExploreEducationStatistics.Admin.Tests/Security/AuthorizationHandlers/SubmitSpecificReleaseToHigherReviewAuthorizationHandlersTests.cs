using GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityClaimTypes;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.PermissionTestUtil;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.EnumUtil;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers
{
    public class SubmitSpecificReleaseToHigherReviewAuthorizationHandlersTests
    {
        [Fact]
        public void SubmitSpecificReleaseToHigherReviewCanSubmitAllReleasesAuthorizationHandler()
        {
            AssertHandlerSucceedsWithCorrectClaims(
                new SubmitSpecificReleaseToHigherReviewCanSubmitAllReleasesAuthorizationHandler(), SubmitAllReleasesToHigherReview);
        }
        
        [Fact]
        public void SubmitSpecificReleaseToHigherReviewHasRoleOnReleaseAuthorizationHandler()
        {
            AssertHandlerSucceedsWithCorrectReleaseRoles(
                contentDbContext => new SubmitSpecificReleaseToHigherReviewHasRoleOnReleaseAuthorizationHandler(contentDbContext),
                GetEnumValuesAsArray<ReleaseRole>());
        }
    }
}