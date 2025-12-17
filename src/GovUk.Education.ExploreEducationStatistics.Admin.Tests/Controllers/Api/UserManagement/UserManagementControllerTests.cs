#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.Fixture;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.Fixture.Optimised;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Controllers.Api.UserManagement;

// ReSharper disable once ClassNeverInstantiated.Global
public class UserManagementControllerTestsFixture()
    : OptimisedAdminCollectionFixture(capabilities: [AdminIntegrationTestCapability.UserAuth]);

[CollectionDefinition(nameof(UserManagementControllerTestsFixture))]
public class UserManagementControllerTestsCollection : ICollectionFixture<UserManagementControllerTestsFixture>;

[Collection(nameof(UserManagementControllerTestsFixture))]
public class UserManagementControllerTests
{
    private static readonly DataFixture DataFixture = new();

    public class DeleteUserTests(UserManagementControllerTestsFixture fixture) : UserManagementControllerTests
    {
        [Theory]
        [InlineData("BAU User", false)]
        [InlineData("Analyst", false)]
        [InlineData("Prerelease User", false)]
        public async Task PermissionCheck(string globalRoleName, bool successExpected)
        {
            var claimsPrincipal = DataFixture
                .AuthenticatedUser()
                .WithRole(globalRoleName)
                .WithEmail("user@education.gov.uk");

            var client = fixture.CreateClient(user: claimsPrincipal);

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
