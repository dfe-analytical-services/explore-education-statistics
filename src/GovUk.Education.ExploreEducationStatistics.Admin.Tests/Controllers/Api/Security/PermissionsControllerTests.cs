#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.Fixture;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Controllers.Api.Security;

public class PermissionsControllerTests(TestApplicationFactory testApp) : IntegrationTestFixture(testApp)
{
    [Fact]
    public async Task GetGlobalPermissions_AuthenticatedUser()
    {
        var client = TestApp.SetUser(DataFixture.AuthenticatedUser()).CreateClient();

        var response = await client.GetAsync("/api/permissions/access");

        response.AssertOk(
            new GlobalPermissionsViewModel(
                CanAccessSystem: true,
                CanAccessAnalystPages: false,
                CanAccessAllImports: false,
                CanAccessPrereleasePages: false,
                CanManageAllTaxonomy: false,
                IsBauUser: false,
                IsApprover: false
            )
        );
    }

    [Fact]
    public async Task GetGlobalPermissions_BauUser()
    {
        var client = TestApp.SetUser(DataFixture.BauUser()).CreateClient();

        var response = await client.GetAsync("/api/permissions/access");

        response.AssertOk(
            new GlobalPermissionsViewModel(
                CanAccessSystem: true,
                CanAccessAnalystPages: true,
                CanAccessAllImports: true,
                CanAccessPrereleasePages: true,
                CanManageAllTaxonomy: true,
                IsBauUser: true,
                // Expect "IsApprover" to be false even for BAU as we don't expect BAU users to be assigned
                // individual Approver roles on Releases or Publications.
                IsApprover: false
            )
        );
    }

    [Fact]
    public async Task GetGlobalPermissions_AnalystUser_NotReleaseOrPublicationApprover()
    {
        var user = DataFixture.AnalystUser().Generate();

        await TestApp.AddTestData<ContentDbContext>(context =>
        {
            // Add test data that gives the user access to a Release without being an Approver.
            context.UserReleaseRoles.Add(
                new UserReleaseRole { UserId = user.GetUserId(), Role = ReleaseRole.Contributor }
            );

            // Add test data that gives the user access to a Publication without being an Approver.
            context.UserPublicationRoles.Add(
                new UserPublicationRole { UserId = user.GetUserId(), Role = PublicationRole.Owner }
            );
        });

        var client = TestApp.SetUser(user).CreateClient();

        var response = await client.GetAsync("/api/permissions/access");

        response.AssertOk(
            new GlobalPermissionsViewModel(
                CanAccessSystem: true,
                CanAccessAnalystPages: true,
                CanAccessAllImports: false,
                CanAccessPrereleasePages: true,
                CanManageAllTaxonomy: false,
                IsBauUser: false,
                // Expect this to be false if the user isn't an approver of any kind
                IsApprover: false
            )
        );
    }

    [Fact]
    public async Task GetGlobalPermissions_AnalystUser_ReleaseApprover()
    {
        var user = DataFixture.AnalystUser().Generate();

        await TestApp.AddTestData<ContentDbContext>(context =>
        {
            context.UserReleaseRoles.Add(
                new UserReleaseRole { UserId = user.GetUserId(), Role = ReleaseRole.Approver }
            );
        });

        var client = TestApp.SetUser(user).CreateClient();

        var response = await client.GetAsync("/api/permissions/access");

        response.AssertOk(
            new GlobalPermissionsViewModel(
                CanAccessSystem: true,
                CanAccessAnalystPages: true,
                CanAccessAllImports: false,
                CanAccessPrereleasePages: true,
                CanManageAllTaxonomy: false,
                IsBauUser: false,
                // Expect this to be true if the user is a Release approver
                IsApprover: true
            )
        );
    }

    [Fact]
    public async Task GetGlobalPermissions_AnalystUser_PublicationApprover()
    {
        var user = DataFixture.AnalystUser().Generate();

        await TestApp.AddTestData<ContentDbContext>(context =>
        {
            context.UserReleaseRoles.Add(
                new UserReleaseRole { UserId = user.GetUserId(), Role = ReleaseRole.Approver }
            );
        });

        var client = TestApp.SetUser(user).CreateClient();

        var response = await client.GetAsync("/api/permissions/access");

        response.AssertOk(
            new GlobalPermissionsViewModel(
                CanAccessSystem: true,
                CanAccessAnalystPages: true,
                CanAccessAllImports: false,
                CanAccessPrereleasePages: true,
                CanManageAllTaxonomy: false,
                IsBauUser: false,
                // Expect this to be true if the user is a Publication approver
                IsApprover: true
            )
        );
    }

    [Fact]
    public async Task GetGlobalPermissions_PreReleaseUser()
    {
        var client = TestApp.SetUser(DataFixture.PreReleaseUser()).CreateClient();

        var response = await client.GetAsync("/api/permissions/access");

        response.AssertOk(
            new GlobalPermissionsViewModel(
                CanAccessSystem: true,
                CanAccessAnalystPages: false,
                CanAccessAllImports: false,
                CanAccessPrereleasePages: true,
                CanManageAllTaxonomy: false,
                IsBauUser: false,
                IsApprover: false
            )
        );
    }

    [Fact]
    public async Task GetGlobalPermissions_UnauthenticatedUser()
    {
        var response = await TestApp.CreateClient().GetAsync("/api/permissions/access");
        response.AssertUnauthorized();
    }
}
