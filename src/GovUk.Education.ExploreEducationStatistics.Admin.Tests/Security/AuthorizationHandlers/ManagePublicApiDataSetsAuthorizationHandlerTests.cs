#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers.Utils.AuthorizationHandlersTestUtil;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.AuthorizationHandlers;

public abstract class ManagePublicApiDataSetsAuthorizationHandlerTests
{
    public class GlobalRolesTests : ManagePublicApiDataSetsAuthorizationHandlerTests
    {
        [Fact]
        public async Task SucceedsOnlyForValidGlobalRoles()
        {
            await AssertHandlerSucceedsWithCorrectGlobalRoles<ManagePublicApiDataSetsRequirement, object?>(
                handler: new ManagePublicApiDataSetsAuthorizationHandler(),
                entity: null,
                rolesExpectedToSucceed: [GlobalRoles.Role.BauUser]
            );
        }
    }
}
