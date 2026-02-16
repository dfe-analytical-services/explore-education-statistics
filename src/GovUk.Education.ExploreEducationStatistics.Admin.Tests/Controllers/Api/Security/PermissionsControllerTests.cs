#nullable enable
using System.Security.Claims;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.Fixture;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.Fixture.Optimised;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
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
    public async Task GetGlobalPermissions_AuthenticatedUser()
    {
        var client = fixture.CreateClient(user: OptimisedTestUsers.Authenticated);

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
        var client = fixture.CreateClient(user: OptimisedTestUsers.Bau);

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
        var user = _dataFixture.AnalystUser().Generate();

        await fixture
            .GetContentDbContext()
            .AddTestData(context =>
            {
                // Add test data that gives the user access to a Release without being an Approver.
                context.UserReleaseRoles.Add(
                    _dataFixture
                        .DefaultUserReleaseRole()
                        .WithUserId(user.GetUserId())
                        .WithRole(ReleaseRole.Contributor)
                        .WithReleaseVersion(
                            _dataFixture
                                .DefaultReleaseVersion()
                                .WithRelease(
                                    _dataFixture.DefaultRelease().WithPublication(_dataFixture.DefaultPublication())
                                )
                        )
                        .Generate()
                );

                // Add test data that gives the user access to a Publication without being an Approver.
                context.UserPublicationRoles.Add(
                    _dataFixture
                        .DefaultUserPublicationRole()
                        .WithUserId(user.GetUserId())
                        .WithRole(PublicationRole.Owner)
                        .WithPublication(_dataFixture.DefaultPublication())
                        .Generate()
                );
            });

        var client = fixture.CreateClient(user: user);

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
        ClaimsPrincipal identityUser = _dataFixture.AnalystUser();
        User user = _dataFixture.DefaultUser().WithId(identityUser.GetUserId());
        UserReleaseRole userReleaseRole = _dataFixture
            .DefaultUserReleaseRole()
            .WithUser(user)
            .WithRole(ReleaseRole.Approver);

        await fixture
            .GetContentDbContext()
            .AddTestData(context =>
            {
                context.UserReleaseRoles.Add(userReleaseRole);
            });

        var client = fixture.CreateClient(user: identityUser);

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
        ClaimsPrincipal identityUser = _dataFixture.AnalystUser();
        User user = _dataFixture.DefaultUser().WithId(identityUser.GetUserId());
        UserPublicationRole userPublicationRole = _dataFixture
            .DefaultUserPublicationRole()
            .WithUser(user)
            .WithRole(PublicationRole.Allower);

        await fixture
            .GetContentDbContext()
            .AddTestData(context =>
            {
                context.UserPublicationRoles.Add(userPublicationRole);
            });

        var client = fixture.CreateClient(user: identityUser);

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
        var client = fixture.CreateClient(user: OptimisedTestUsers.PreReleaseUser);

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
        var response = await fixture.CreateClient().GetAsync("/api/permissions/access");
        response.AssertUnauthorized();
    }
}
