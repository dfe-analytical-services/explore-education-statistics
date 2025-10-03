using System.Security.Claims;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Security;

namespace GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;

public static class ClaimsPrincipalGeneratorExtensions
{
    public static Generator<ClaimsPrincipal> DefaultClaimsPrincipal(this DataFixture fixture) =>
        fixture.Generator<ClaimsPrincipal>();

    public static Generator<ClaimsPrincipal> WithName(this Generator<ClaimsPrincipal> generator, string name) =>
        generator.ForInstance(p => p.AddClaim(EesClaimTypes.Name, name));

    public static Generator<ClaimsPrincipal> WithRole(this Generator<ClaimsPrincipal> generator, string role) =>
        generator.ForInstance(p => p.AddClaim(EesClaimTypes.Role, role));

    public static Generator<ClaimsPrincipal> WithScope(this Generator<ClaimsPrincipal> generator, string scopeName) =>
        generator.ForInstance(p => p.AddClaim(EesClaimTypes.SupportedMsalScope, scopeName));

    public static Generator<ClaimsPrincipal> WithEmail(this Generator<ClaimsPrincipal> generator, string email) =>
        generator.ForInstance(p => p.AddClaim(ClaimTypes.Email, email));

    public static Generator<ClaimsPrincipal> WithClaim(
        this Generator<ClaimsPrincipal> generator,
        string claimName,
        string claimValue = ""
    ) => generator.ForInstance(p => p.AddClaim(claimName, claimValue));

    public static Generator<ClaimsPrincipal> WithClaims(this Generator<ClaimsPrincipal> generator, Claim[] claims) =>
        generator.ForInstance(p => p.AddClaims(claims));

    public static InstanceSetters<ClaimsPrincipal> AddClaim(
        this InstanceSetters<ClaimsPrincipal> setters,
        string claimName,
        string claimValue = ""
    ) => setters.AddClaims([new Claim(claimName, claimValue)]);

    public static InstanceSetters<ClaimsPrincipal> AddClaims(
        this InstanceSetters<ClaimsPrincipal> setters,
        Claim[] claims
    ) =>
        setters.Set(
            (_, p) =>
            {
                var claimsIdentity = new ClaimsIdentity(
                    authenticationType: "Bearer",
                    claims: claims,
                    nameType: EesClaimTypes.Name,
                    roleType: EesClaimTypes.Role
                );
                p.AddIdentity(claimsIdentity);
            }
        );
}
