#nullable enable
using System.Security.Claims;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.Fixture;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Controllers.Api.UserManagement;

public class UserManagementControllerTests(TestApplicationFactory testApp) : IntegrationTestFixture(testApp)
{
    public class DeleteUserTests(TestApplicationFactory testApp) : UserManagementControllerTests(testApp)
    {
        [Theory]
        [InlineData("BAU User", true)]
        [InlineData("Analyst", false)]
        [InlineData("Prerelease User", false)]
        public async Task PermissionCheck(
            string globalRoleName,
            bool successExpected)
        {
            var claimsPrincipal = ClaimsPrincipalUtils.AuthenticatedUser();
            var claimsIdentity = (claimsPrincipal.Identity as ClaimsIdentity)!;
            claimsIdentity.AddClaim(ClaimsPrincipalUtils.RoleClaim(globalRoleName));
            claimsIdentity.AddClaim(new Claim(ClaimTypes.Email, "user@education.gov.uk"));

            var client = TestApp
                .SetUser(claimsPrincipal)
                .CreateClient();

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
