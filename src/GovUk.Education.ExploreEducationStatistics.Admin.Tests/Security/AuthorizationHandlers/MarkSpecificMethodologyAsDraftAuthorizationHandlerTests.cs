using GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityClaimTypes;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.Utils.
    ReleaseAuthorizationHandlersTestUtil;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers
{
    public class MarkSpecificMethodologyAsDraftAuthorizationHandlerTests
    {
        [Fact]
        public void MarkSpecificMethodologyAsDraftAuthorizationHandler()
        {
            AssertReleaseHandlerSucceedsWithCorrectClaims<MarkSpecificMethodologyAsDraftRequirement>(
                new MarkSpecificMethodologyAsDraftAuthorizationHandler(),
                MarkAllMethodologiesDraft
            );
        }
    }
}