using GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityClaimTypes;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers.UpdateSpecificMethodologyAuthorizationHandler;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.Utils.
    ReleaseAuthorizationHandlersTestUtil;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers
{
    public class UpdateSpecificMethodologyAuthorizationHandlersTests
    {
        [Fact]
        public void UpdateAllSpecificMethodologies()
        {
            AssertReleaseHandlerSucceedsWithCorrectClaims<UpdateSpecificMethodologyRequirement>(
                new CanUpdateAllSpecificMethodologies(),
                UpdateAllMethodologies
            );
        }
    }
}
