#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.Utils;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;
using Xunit;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

public class ClaimsPrincipalTransformationServiceTests : IntegrationTest<TestStartup>
{
    public ClaimsPrincipalTransformationServiceTests(TestApplicationFactory<TestStartup> testApp)
        : base(testApp)
    {}

    [Theory]
    [InlineData("unknown-scope-claim", "access-admin-api", false)]
    [InlineData("scp", "access-admin-api", true)]
    [InlineData("scp", "scope1 access-admin-api scope3", true)]
    [InlineData("scp", "unknown-scope", false)]
    [InlineData("scope", "access-admin-api", true)]
    [InlineData("scope", "scope1 access-admin-api scope3", true)]
    [InlineData("scope", "unknown-scope", false)]
    [InlineData("http://schemas.microsoft.com/identity/claims/scope", "access-admin-api", true)]
    [InlineData("http://schemas.microsoft.com/identity/claims/scope", "scope1 access-admin-api scope3", true)]
    [InlineData("http://schemas.microsoft.com/identity/claims/scope", "unknown-scope", false)]
    public async Task ScopeClaims(string scopeClaimName, string scopeClaimValue, bool successExpected)
    {
        var claimsPrincipal = ClaimsPrincipalUtils.CreateClaimsPrincipal(
            Guid.NewGuid(),
            new Claim(ClaimTypes.Email, "user@example.com"),
            new Claim(scopeClaimName, scopeClaimValue));

        // Set up scenario and test data.
        var client = TestApp
            .WithWebHostBuilder(builder => builder
                .WithAdditionalControllers(typeof(TestController)))
            .SetUser(claimsPrincipal)
            .CreateClient();

        var response = await client.GetAsync("/test/scope-access");

        // By getting an OK result, we've shown that the user has successfully presented the correct scope in
        // their JWT. Otherwise they will be forbidden.
        if (successExpected)
        {
            response.AssertOk();
        }
        else
        {
            response.AssertForbidden();
        }
    }

    [Fact]
    public async Task RoleClaims()
    {
        const string email = "EXISTING-USER@education.gov.uk";
        const string unrelatedUserEmail = "unrelated-user@education.gov.uk";
        var userId = Guid.NewGuid();
        var unrelatedUserId = Guid.NewGuid();

        var claimsPrincipal = ClaimsPrincipalUtils.VerifiedByIdentityProviderUser();
        var claimsIdentity = (claimsPrincipal.Identity as ClaimsIdentity)!;
        claimsIdentity.AddClaim(new Claim(ClaimTypes.Email, email));

        // Set up scenario and test data.
        var client = TestApp
            .WithWebHostBuilder(builder => builder
                .WithAdditionalControllers(typeof(TestController)))
            .SetUser(claimsPrincipal)
            .AddUsersAndRolesDbTestData(context =>
            {
                var globalRoles = GetGlobalRoles();
                context.Roles.AddRange(globalRoles);

                var globalRoleClaims = GetGlobalRoleClaims();
                context.RoleClaims.AddRange(globalRoleClaims);

                // Add an Identity user for an unrelated user.
                context.Users.Add(new ApplicationUser
                {
                    Id = unrelatedUserId.ToString(),
                    Email = unrelatedUserEmail.ToLower(),
                    NormalizedEmail = unrelatedUserEmail.ToUpper(),
                    FirstName = "AnotherFirstName",
                    LastName = "AnotherLastName"
                });

                // Add a global role assignment for the unrelated user.
                context.UserRoles.Add(new IdentityUserRole<string>
                {
                    UserId = unrelatedUserId.ToString(),
                    RoleId = globalRoles[0].Id
                });

                // Add an Identity user for the user.
                context.Users.Add(new ApplicationUser
                {
                    Id = userId.ToString(),
                    Email = email,
                    NormalizedEmail = email.ToUpper(),
                    FirstName = "FirstName",
                    LastName = "LastName"
                });

                // Add a global role assignment for the user.
                context.UserRoles.Add(new IdentityUserRole<string>
                {
                    UserId = userId.ToString(),
                    RoleId = globalRoles[1].Id
                });

            })
            .CreateClient();

        var response = await client.GetAsync("/test/role-claims");

        // Get the list of Claims that were on the ClaimsPrincipal when it hit the controller endpoint.
        var claimList = response.AssertOk<List<ClaimViewModel>>();

        // Check that the user's Id was correctly mapped to a Claim.
        Assert.Contains(new ClaimViewModel(EesClaimTypes.LocalUserId, userId.ToString()), claimList);

        // Check that the scope Claim is present.
        Assert.Contains(new ClaimViewModel(EesClaimTypes.SupportedMsalScope, "access-admin-api"), claimList);

        // Check that the user's Global role Claim is present.
        Assert.Contains(new ClaimViewModel(EesClaimTypes.Role, "Role 2"), claimList);

        // Check that the Claims that hang off the user's Global role are present.
        Assert.Contains(new ClaimViewModel("Role 2 Claim 1", "Role 2 Claim 1 value"), claimList);
        Assert.Contains(new ClaimViewModel("Role 2 Claim 2", "Role 2 Claim 2 value"), claimList);

        // Check that Claims unrelated to the user are not present.
        Assert.DoesNotContain(new ClaimViewModel(EesClaimTypes.LocalUserId, unrelatedUserId.ToString()), claimList);
        Assert.DoesNotContain(new ClaimViewModel(EesClaimTypes.Role, "Role 1"), claimList);
        Assert.DoesNotContain(new ClaimViewModel("Role 1 Claim 1", "Role 1 Claim 1 value"), claimList);
        Assert.DoesNotContain(new ClaimViewModel("Role 1 Claim 2", "Role 1 Claim 2 value"), claimList);
    }

    private static List<IdentityRole> GetGlobalRoles()
    {
        return new List<IdentityRole>
        {
            new()
            {
                Id = "role-1",
                Name = "Role 1",
                NormalizedName = "ROLE 1"
            },
            new()
            {
                Id = "role-2",
                Name = "Role 2",
                NormalizedName = "ROLE 2"
            }
        };
    }

    private static List<IdentityRoleClaim<string>> GetGlobalRoleClaims()
    {
        return new List<IdentityRoleClaim<string>>
        {
            new()
            {
                RoleId = "role-1",
                ClaimType = "Role 1 Claim 1",
                ClaimValue = "Role 1 Claim 1 value"
            },
            new()
            {
                RoleId = "role-1",
                ClaimType = "Role 1 Claim 2",
                ClaimValue = "Role 1 Claim 2 value"
            },
            new()
            {
                RoleId = "role-2",
                ClaimType = "Role 2 Claim 1",
                ClaimValue = "Role 2 Claim 1 value"
            },
            new()
            {
                RoleId = "role-2",
                ClaimType = "Role 2 Claim 2",
                ClaimValue = "Role 2 Claim 2 value"
            }
        };
    }

    private class TestController : ControllerBase
    {
        private readonly IUserService _userService;

        public TestController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet("/test/scope-access")]
        [AllowAnonymous]
        public async Task<ActionResult> ScopeAccess()
        {
            var authorizedByIdP = await _userService.MatchesPolicy(SecurityPolicies.AuthenticatedByIdentityProvider);
            if (!authorizedByIdP)
            {
                return new ForbidResult();
            }

            return new OkResult();
        }

        [HttpGet("/test/role-claims")]
        [AllowAnonymous]
        public async Task<ActionResult<List<string>>> RoleClaims()
        {
            return new OkObjectResult(HttpContext
                .User
                .Claims
                .Select(c => new ClaimViewModel(c.Type, c.Value))
                .ToList());
        }
    }

    public record ClaimViewModel(string Type, string Value);
}
