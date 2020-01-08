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
            AssertReleaseHandlerSucceedsWithCorrectClaims(
                new ViewSpecificReleaseCanSeeAllReleasesAuthorizationHandler(), AccessAllReleases);
        }
        
        [Fact]
        public void ViewSpecificReleaseHasRoleOnReleaseAuthorizationHandler()
        {
            AssertReleaseHandlerSucceedsWithCorrectReleaseRoles(
                contentDbContext => new ViewSpecificReleaseHasRoleOnReleaseAuthorizationHandler(contentDbContext),
                GetEnumValuesAsArray<ReleaseRole>());
        }
    }
}