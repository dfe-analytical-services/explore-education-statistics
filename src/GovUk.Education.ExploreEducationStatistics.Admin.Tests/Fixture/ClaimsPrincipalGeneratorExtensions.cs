using System.Security.Claims;
using GovUk.Education.ExploreEducationStatistics.Admin.Models;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Fixture;

public static class ClaimsPrincipalGeneratorExtensions
{
    public static Generator<ClaimsPrincipal> VerifiedByIdentityProviderUser(this DataFixture fixture)
    {
        return fixture
            .Generator<ClaimsPrincipal>()
            .WithId(Guid.NewGuid())
            .WithScope(SecurityScopes.AccessAdminApiScope);
    }

    public static Generator<ClaimsPrincipal> AuthenticatedUser(this DataFixture fixture, Guid userId = default)
    {
        return fixture
            .Generator<ClaimsPrincipal>()
            .WithId(userId != Guid.Empty ? userId : Guid.NewGuid())
            .WithScope(SecurityScopes.AccessAdminApiScope)
            .WithClaim(nameof(SecurityClaimTypes.ApplicationAccessGranted));
    }

    public static Generator<ClaimsPrincipal> BauUser(this DataFixture fixture)
    {
        return fixture
            .Generator<ClaimsPrincipal>()
            .WithId(Guid.NewGuid())
            .WithRole(GlobalRoles.RoleNames.BauUser)
            .WithScope(SecurityScopes.AccessAdminApiScope)
            .ForInstance(s => EnumUtil.GetEnums<SecurityClaimTypes>().ForEach(c => s.AddClaim(c.ToString())));
    }

    public static Generator<ClaimsPrincipal> StandardUser(this DataFixture fixture)
    {
        return fixture
            .Generator<ClaimsPrincipal>()
            .WithId(Guid.NewGuid())
            .WithRole(GlobalRoles.RoleNames.StandardUser)
            .WithScope(SecurityScopes.AccessAdminApiScope)
            .WithClaim(nameof(SecurityClaimTypes.ApplicationAccessGranted));
    }

    public static Generator<ClaimsPrincipal> WithId(this Generator<ClaimsPrincipal> generator, Guid userId) =>
        generator.ForInstance(p => p.AddClaim(EesClaimTypes.LocalUserId, userId.ToString()));
}
