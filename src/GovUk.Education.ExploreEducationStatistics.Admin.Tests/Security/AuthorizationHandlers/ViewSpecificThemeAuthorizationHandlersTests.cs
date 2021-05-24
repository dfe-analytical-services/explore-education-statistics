using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityClaimTypes;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.Utils.AuthorizationHandlersTestUtil;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers
{
    public class ViewSpecificThemeAuthorizationHandlersTests
    {
        [Fact]
        public async Task ViewSpecificThemeAuthorizationHandler()
        {
            // Assert that any users with the "AccessAllTopics" claim can view an arbitrary Theme
            // (and no other claim allows this)
            //
            // Note that we're deliberately using the "All TOPICS" claim rather than having to have a separate 
            // "All THEMES" claim, as they're effectively the same
            await AssertHandlerSucceedsWithCorrectClaims<ViewSpecificThemeRequirement>(
                new ViewSpecificThemeAuthorizationHandler(),
                AccessAllTopics);
        }
    }
}
