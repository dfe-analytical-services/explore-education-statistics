using GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityClaimTypes;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.ReleaseAuthorizationHandlersTestUtil;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.EnumUtil;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers
{
    public class ViewSpecificReleaseAuthorizationHandlersTests
    {
        [Fact]
        public void ViewSpecificReleaseCanSeeAllReleasesAuthorizationHandler()
        {
            // Assert that any users with the "AccessAllReleases" claim can view an arbitrary Release
            // (and no other claim allows this)
            AssertReleaseHandlerSucceedsWithCorrectClaims<ViewSpecificReleaseRequirement>(
                new ViewSpecificReleaseCanSeeAllReleasesAuthorizationHandler(), AccessAllReleases);
        }
        
        [Fact]
        public void ViewSpecificReleaseHasRoleOnReleaseAuthorizationHandler()
        {
            // Assert that a User who has any role on a Release can view the Release
            AssertReleaseHandlerSucceedsWithCorrectReleaseRoles<ViewSpecificReleaseRequirement>(
                contentDbContext => new ViewSpecificReleaseHasRoleOnReleaseAuthorizationHandler(contentDbContext),
                GetEnumValuesAsArray<ReleaseRole>());
        }
    }
}