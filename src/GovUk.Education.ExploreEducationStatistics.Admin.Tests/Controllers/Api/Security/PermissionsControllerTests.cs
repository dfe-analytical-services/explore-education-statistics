#nullable enable
using System.Security.Claims;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.Fixture;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.Fixture.Optimised;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.IntegrationTests;
using GovUk.Education.ExploreEducationStatistics.Common.IntegrationTests.WebApp;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Controllers.Api.Security;

// ReSharper disable once ClassNeverInstantiated.Global
public class PermissionsControllerTestsFixture()
    : OptimisedAdminCollectionFixture(capabilities: [AdminIntegrationTestCapability.UserAuth]);

[CollectionDefinition(nameof(PermissionsControllerTestsFixture))]
public class PermissionsControllerTestsCollection : ICollectionFixture<PermissionsControllerTestsFixture>;

[Collection(nameof(PermissionsControllerTestsFixture))]
public class PermissionsControllerTests(PermissionsControllerTestsFixture fixture)
    : OptimisedIntegrationTestBase<Startup>(fixture)
{
    private static readonly DataFixture _dataFixture = new();

    [Fact]
    public async Task GetGlobalPermissions_StandardUser()
    {
        ClaimsPrincipal identityUser = _dataFixture.StandardUser();

        User user = _dataFixture.DefaultUser().WithId(identityUser.GetUserId());

        await fixture.GetContentDbContext().AddTestData(context => context.Users.Add(user));

        var client = fixture.CreateClient(identityUser);

        var response = await client.GetAsync("/api/permissions/access");

        response.AssertOk(
            new GlobalPermissionsViewModel(
                CanAccessSystem: true,
                CanAccessAnalystPages: false,
                CanAccessAllImports: false,
                CanManageAllTaxonomy: false,
                CanManagePublicApiDataSets: false,
                IsBauUser: false,
                IsApprover: false
            )
        );
    }

    [Fact]
    public async Task GetGlobalPermissions_BauUser()
    {
        ClaimsPrincipal identityUser = _dataFixture.BauUser();

        User user = _dataFixture
            .DefaultUser()
            .WithId(identityUser.GetUserId())
            .WithRoleId(GlobalRoles.Role.BauUser.GetEnumValue());

        await fixture.GetContentDbContext().AddTestData(context => context.Users.Add(user));

        var client = fixture.CreateClient(identityUser);

        var response = await client.GetAsync("/api/permissions/access");

        response.AssertOk(
            new GlobalPermissionsViewModel(
                CanAccessSystem: true,
                CanAccessAnalystPages: true,
                CanAccessAllImports: true,
                CanManageAllTaxonomy: true,
                CanManagePublicApiDataSets: true,
                IsBauUser: true,
                // Expect "IsApprover" to be false even for BAU as we don't expect BAU users to be assigned
                // individual Approver roles on Releases or Publications.
                IsApprover: false
            )
        );
    }

    [Fact]
    public async Task GetGlobalPermissions_StandardUser_NotPublicationApprover()
    {
        ClaimsPrincipal identityUser = _dataFixture.StandardUser();
        User user = _dataFixture.DefaultUser().WithId(identityUser.GetUserId());
        UserPublicationRole userPublicationRole = _dataFixture
            .DefaultUserPublicationRole()
            .WithUser(user)
            .WithRole(PublicationRole.Drafter)
            .WithPublication(_dataFixture.DefaultPublication());

        // Add test data that gives the user access to a Publication without being an Approver.
        await fixture
            .GetContentDbContext()
            .AddTestData(context => context.UserPublicationRoles.Add(userPublicationRole));

        var client = fixture.CreateClient(identityUser);

        var response = await client.GetAsync("/api/permissions/access");

        response.AssertOk(
            new GlobalPermissionsViewModel(
                CanAccessSystem: true,
                CanAccessAnalystPages: true,
                CanAccessAllImports: false,
                CanManageAllTaxonomy: false,
                CanManagePublicApiDataSets: false,
                IsBauUser: false,
                // Expect this to be false if the user isn't an approver
                IsApprover: false
            )
        );
    }

    [Fact]
    public async Task GetGlobalPermissions_StandardUser_PublicationApprover()
    {
        ClaimsPrincipal identityUser = _dataFixture.StandardUser();
        User user = _dataFixture.DefaultUser().WithId(identityUser.GetUserId());
        UserPublicationRole userPublicationRole = _dataFixture
            .DefaultUserPublicationRole()
            .WithUser(user)
            .WithRole(PublicationRole.Approver)
            .WithPublication(_dataFixture.DefaultPublication());

        await fixture
            .GetContentDbContext()
            .AddTestData(context => context.UserPublicationRoles.Add(userPublicationRole));

        var client = fixture.CreateClient(identityUser);

        var response = await client.GetAsync("/api/permissions/access");

        response.AssertOk(
            new GlobalPermissionsViewModel(
                CanAccessSystem: true,
                CanAccessAnalystPages: true,
                CanAccessAllImports: false,
                CanManageAllTaxonomy: false,
                CanManagePublicApiDataSets: false,
                IsBauUser: false,
                // Expect this to be true if the user is a Publication approver
                IsApprover: true
            )
        );
    }

    [Fact]
    public async Task GetGlobalPermissions_UnauthenticatedUser()
    {
        var response = await fixture.CreateClient().GetAsync("/api/permissions/access");
        response.AssertUnauthorized();
    }
}
