#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.Fixture;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.Fixture.Optimised;
using GovUk.Education.ExploreEducationStatistics.Common.IntegrationTests.WebApp;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;

#pragma warning disable CS9107 // Parameter is captured into the state of the enclosing type and its value is also passed to the base constructor. The value might be captured by the base class as well.

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Controllers.Api.UserManagement;

// ReSharper disable once ClassNeverInstantiated.Global
public class UsersControllerTestsFixture()
    : OptimisedAdminCollectionFixture(capabilities: [AdminIntegrationTestCapability.UserAuth]);

[CollectionDefinition(nameof(UsersControllerTestsFixture))]
public class UsersControllerTestsCollection : ICollectionFixture<UsersControllerTestsFixture>;

[Collection(nameof(UsersControllerTestsFixture))]
public abstract class UsersControllerTests(UsersControllerTestsFixture fixture)
    : OptimisedIntegrationTestBase<Startup>(fixture)
{
    private const string BaseAddress = "/api/users";

    private static readonly DataFixture DataFixture = new();

    public class DeleteUserTests(UsersControllerTestsFixture fixture) : UsersControllerTests(fixture)
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

            var response = await client.DeleteAsync($"{BaseAddress}/ees-test.delete@education.gov.uk");

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
