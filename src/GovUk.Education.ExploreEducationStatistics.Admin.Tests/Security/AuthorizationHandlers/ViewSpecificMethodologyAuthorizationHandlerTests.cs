using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityClaimTypes;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.Utils.AuthorizationHandlersTestUtil;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers
{
    public class ViewSpecificMethodologyAuthorizationHandlerTests
    {
        [Fact]
        public async Task CanViewAllMethodologies()
        {
            await AssertHandlerSucceedsWithCorrectClaims<ViewSpecificMethodologyRequirement>(
                new ViewSpecificMethodologyAuthorizationHandler(),
                AccessAllMethodologies);
        }
    }
}
