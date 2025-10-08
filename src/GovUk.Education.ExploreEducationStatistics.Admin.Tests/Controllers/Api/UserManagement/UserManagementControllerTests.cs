#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.Fixture;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Controllers.Api.UserManagement;

public class UserManagementControllerTests(TestApplicationFactory testApp) : IntegrationTestFixture(testApp)
{
    private readonly DataFixture _fixture = new();

    public class DeleteUserTests(TestApplicationFactory testApp) : UserManagementControllerTests(testApp)
    {
        [Theory]
        [InlineData("BAU User", false)]
        [InlineData("Analyst", false)]
        [InlineData("Prerelease User", false)]
        public async Task PermissionCheck(string globalRoleName, bool successExpected)
        {
            var claimsPrincipal = _fixture
                .AuthenticatedUser()
                .WithRole(globalRoleName)
                .WithEmail("user@education.gov.uk");

            var client = TestApp.SetUser(claimsPrincipal).CreateClient();

            var response = await client.DeleteAsync("/api/user-management/user/ees-test.delete@education.gov.uk");

            if (successExpected)
            {
                response.AssertNoContent();
            }
            else
            {
                response.AssertForbidden();
            }
        }
    }
}
