using System.Security.Claims;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Security;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Fixture;

public static class ClaimsPrincipalGeneratorExtensions
{
    public static Generator<ClaimsPrincipal> AdminAccessUser(this DataFixture fixture)
    {
        return fixture.Generator<ClaimsPrincipal>().WithRole(SecurityConstants.AdminAccessAppRole);
    }

    public static Generator<ClaimsPrincipal> UnsupportedRoleUser(this DataFixture fixture)
    {
        return fixture.Generator<ClaimsPrincipal>().WithRole("Unsupported Role");
    }
}
