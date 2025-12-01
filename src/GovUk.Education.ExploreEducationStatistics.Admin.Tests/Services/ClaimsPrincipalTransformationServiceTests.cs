#nullable enable
using System.Security.Claims;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.Fixture;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.Fixture.Optimised;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

[CollectionDefinition(nameof(ClaimsPrincipalTransformationServiceTestsFixture))]
public class ClaimsPrincipalTransformationServiceTestsCollection
    : ICollectionFixture<ClaimsPrincipalTransformationServiceTestsFixture>;

[Collection(nameof(ClaimsPrincipalTransformationServiceTestsFixture))]
public class ClaimsPrincipalTransformationServiceTests(ClaimsPrincipalTransformationServiceTestsFixture fixture)
{
    private static readonly DataFixture DataFixture = new(new Random().Next());

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
        var claimsPrincipal = DataFixture
            .DefaultClaimsPrincipal()
            .WithId(Guid.NewGuid())
            .WithEmail("user@example.com")
            .WithClaim(scopeClaimName, scopeClaimValue);

        // Set up scenario and test data.
        fixture.RegisterTestUser(claimsPrincipal);
        var client = fixture.CreateClient().WithUser(claimsPrincipal);

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

        var claimsPrincipal = DataFixture.VerifiedByIdentityProviderUser().Generate();

        var claimsIdentity = (claimsPrincipal.Identity as ClaimsIdentity)!;
        claimsIdentity.AddClaim(new Claim(ClaimTypes.Email, email));

        await fixture
            .GetUsersAndRolesDbContext()
            .AddTestData(context =>
            {
                var globalRoles = GetGlobalRoles();
                context.Roles.AddRange(globalRoles);

                var globalRoleClaims = GetGlobalRoleClaims();
                context.RoleClaims.AddRange(globalRoleClaims);

                // Add an Identity user for an unrelated user.
                context.Users.Add(
                    new ApplicationUser
                    {
                        Id = unrelatedUserId.ToString(),
                        Email = unrelatedUserEmail.ToLower(),
                        NormalizedEmail = unrelatedUserEmail.ToUpper(),
                        FirstName = "AnotherFirstName",
                        LastName = "AnotherLastName",
                    }
                );

                // Add a global role assignment for the unrelated user.
                context.UserRoles.Add(
                    new IdentityUserRole<string> { UserId = unrelatedUserId.ToString(), RoleId = globalRoles[0].Id }
                );

                // Add an Identity user for the user.
                context.Users.Add(
                    new ApplicationUser
                    {
                        Id = userId.ToString(),
                        Email = email,
                        NormalizedEmail = email.ToUpper(),
                        FirstName = "FirstName",
                        LastName = "LastName",
                    }
                );

                // Add a global role assignment for the user.
                context.UserRoles.Add(
                    new IdentityUserRole<string> { UserId = userId.ToString(), RoleId = globalRoles[1].Id }
                );
            });

        // Set up scenario and test data.
        fixture.RegisterTestUser(claimsPrincipal);
        var client = fixture.CreateClient().WithUser(claimsPrincipal);

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
                NormalizedName = "ROLE 1",
            },
            new()
            {
                Id = "role-2",
                Name = "Role 2",
                NormalizedName = "ROLE 2",
            },
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
                ClaimValue = "Role 1 Claim 1 value",
            },
            new()
            {
                RoleId = "role-1",
                ClaimType = "Role 1 Claim 2",
                ClaimValue = "Role 1 Claim 2 value",
            },
            new()
            {
                RoleId = "role-2",
                ClaimType = "Role 2 Claim 1",
                ClaimValue = "Role 2 Claim 1 value",
            },
            new()
            {
                RoleId = "role-2",
                ClaimType = "Role 2 Claim 2",
                ClaimValue = "Role 2 Claim 2 value",
            },
        };
    }

    public record ClaimViewModel(string Type, string Value);
}

public class TestController(IUserService userService) : ControllerBase
{
    [HttpGet("/test/scope-access")]
    [AllowAnonymous]
    public async Task<ActionResult> ScopeAccess()
    {
        var authorizedByIdP = await userService.MatchesPolicy(SecurityPolicies.AuthenticatedByIdentityProvider);
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
        return await Task.FromResult(
            new OkObjectResult(
                HttpContext
                    .User.Claims.Select(c => new ClaimsPrincipalTransformationServiceTests.ClaimViewModel(
                        c.Type,
                        c.Value
                    ))
                    .ToList()
            )
        );
    }
}

// ReSharper disable once ClassNeverInstantiated.Global
public class ClaimsPrincipalTransformationServiceTestsFixture()
    : OptimisedAdminCollectionFixture(capabilities: [AdminIntegrationTestCapability.UserAuth])
{
    protected override void ModifyServices(OptimisedServiceCollectionModifications modifications)
    {
        modifications.AddController(typeof(TestController));
    }
}
