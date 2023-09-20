#nullable enable
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.Utils.ClaimsPrincipalUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Controllers.Api.Security;

public class PermissionsControllerTests : IClassFixture<TestApplicationFactory<TestStartup>>
{
    private readonly WebApplicationFactory<TestStartup> _testApp;
    
    public PermissionsControllerTests(TestApplicationFactory<TestStartup> testApp)
    {
        _testApp = testApp;
    }

    [Fact]
    public async Task GetGlobalPermissions_AuthenticatedUser()
    {
        var client = _testApp
            .SetUser(AuthenticatedUser())
            .CreateClient();
        
        var response = await client.GetAsync("/api/permissions/access");
        
        response.AssertOk(new GlobalPermissionsViewModel(
            CanAccessSystem: true,
            CanAccessAnalystPages: false,
            CanAccessAllImports: false,
            CanAccessPrereleasePages: false,
            CanManageAllTaxonomy: false,
            IsBauUser: false,
            IsApprover: false));
    }
    
    [Fact]
    public async Task GetGlobalPermissions_BauUser()
    {
        var client = _testApp
            .SetUser(BauUser())
            .CreateClient();
        
        var response = await client.GetAsync("/api/permissions/access");
        
        response.AssertOk(new GlobalPermissionsViewModel(
            CanAccessSystem: true,
            CanAccessAnalystPages: true,
            CanAccessAllImports: true,
            CanAccessPrereleasePages: true,
            CanManageAllTaxonomy: true,
            IsBauUser: true,
            // Expect "IsApprover" to be false even for BAU as we don't expect BAU users to be assigned
            // individual Approver roles on Releases or Publications.
            IsApprover: false));
    }
    
    [Fact]
    public async Task GetGlobalPermissions_AnalystUser_NotReleaseOrPublicationApprover()
    {
        var user = AnalystUser();

        var client = _testApp
            .SetUser(user)
            .AddContentDbTestData(context =>
            {
                // Add test data that gives the user access to a Release without being an Approver.
                context.UserReleaseRoles.Add(new UserReleaseRole
                {
                    UserId = user.GetUserId(),
                    Role = ReleaseRole.Contributor
                });
                
                // Add test data that gives the user access to a Publication without being an Approver.
                context.UserPublicationRoles.Add(new UserPublicationRole
                {
                    UserId = user.GetUserId(),
                    Role = PublicationRole.Owner
                });
            })
            .CreateClient();
        
        var response = await client.GetAsync("/api/permissions/access");
        
        response.AssertOk(new GlobalPermissionsViewModel(
            CanAccessSystem: true,
            CanAccessAnalystPages: true,
            CanAccessAllImports: false,
            CanAccessPrereleasePages: true,
            CanManageAllTaxonomy: false,
            IsBauUser: false,
            // Expect this to be false if the user isn't an approver of any kind
            IsApprover: false));
    }
    
    [Fact]
    public async Task GetGlobalPermissions_AnalystUser_ReleaseApprover()
    {
        var user = AnalystUser();

        var client = _testApp
            .SetUser(user)
            .AddContentDbTestData(context =>
            {
                context.UserReleaseRoles.Add(new UserReleaseRole
                {
                    UserId = user.GetUserId(),
                    Role = ReleaseRole.Approver
                });
            })
            .CreateClient();
        
        var response = await client.GetAsync("/api/permissions/access");
        
        response.AssertOk(new GlobalPermissionsViewModel(
            CanAccessSystem: true,
            CanAccessAnalystPages: true,
            CanAccessAllImports: false,
            CanAccessPrereleasePages: true,
            CanManageAllTaxonomy: false,
            IsBauUser: false,
            // Expect this to be true if the user is a Release approver
            IsApprover: true));
    }

    [Fact]
    public async Task GetGlobalPermissions_AnalystUser_PublicationApprover()
    {
        var user = AnalystUser();

        var client = _testApp
            .SetUser(user)
            .AddContentDbTestData(context =>
            {
                context.UserPublicationRoles.Add(new UserPublicationRole
                {
                    UserId = user.GetUserId(),
                    Role = PublicationRole.Approver
                });
            })
            .CreateClient();
        
        var response = await client.GetAsync("/api/permissions/access");
        
        response.AssertOk(new GlobalPermissionsViewModel(
            CanAccessSystem: true,
            CanAccessAnalystPages: true,
            CanAccessAllImports: false,
            CanAccessPrereleasePages: true,
            CanManageAllTaxonomy: false,
            IsBauUser: false,
            // Expect this to be true if the user is a Publication approver
            IsApprover: true));
    }

    [Fact]
    public async Task GetGlobalPermissions_PreReleaseUser()
    {
        var client = _testApp
            .SetUser(PreReleaseUser())
            .CreateClient();
        
        var response = await client.GetAsync("/api/permissions/access");
        
        response.AssertOk(new GlobalPermissionsViewModel(
            CanAccessSystem: true,
            CanAccessAnalystPages: false,
            CanAccessAllImports: false,
            CanAccessPrereleasePages: true,
            CanManageAllTaxonomy: false,
            IsBauUser: false,
            IsApprover: false));
    }
    
    [Fact]
    public async Task GetGlobalPermissions_UnauthenticatedUser()
    {
        var response = await _testApp.CreateClient().GetAsync("/api/permissions/access");
        response.AssertUnauthorized();
    }
}