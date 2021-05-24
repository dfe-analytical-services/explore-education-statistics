using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityClaimTypes;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.Utils.AuthorizationHandlersTestUtil;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers
{
    public class ViewSpecificTopicAuthorizationHandlersTests
    {
        [Fact]
        public async Task CanViewAllTopics()
        {
            // Assert that any users with the "AccessAllTopics" claim can view an arbitrary Theme
            // (and no other claim allows this)
            await AssertHandlerSucceedsWithCorrectClaims<ViewSpecificTopicRequirement>(
                new ViewSpecificTopicAuthorizationHandler(),
                AccessAllTopics);
        }
    }
}
