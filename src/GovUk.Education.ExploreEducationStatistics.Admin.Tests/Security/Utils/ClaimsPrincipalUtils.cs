using System;
using System.Linq;
using System.Security.Claims;
using GovUk.Education.ExploreEducationStatistics.Admin.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Models.GlobalRoles;
using static GovUk.Education.ExploreEducationStatistics.Admin.Models.GlobalRoles.RoleNames;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityClaimTypes;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.ClaimsPrincipalUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Security.Utils;

public static class ClaimsPrincipalUtils
{
    public static ClaimsPrincipal VerifiedButNotAuthorizedByIdentityProviderUser()
    {
        return CreateClaimsPrincipal(
            IdClaim(Guid.NewGuid()),
            ScopeClaim("some-other-scope"));
    }

    public static ClaimsPrincipal VerifiedByIdentityProviderUser()
    {
        return CreateClaimsPrincipal(
            IdClaim(Guid.NewGuid()),
            ScopeClaim(SecurityScopes.AccessAdminApiScope));
    }

    public static ClaimsPrincipal AuthenticatedUser(params Claim[] additionalClaims)
    {
        return CreateClaimsPrincipal(
            [
                IdClaim(Guid.NewGuid()),
                GenericClaim(ApplicationAccessGranted.ToString()),
                ScopeClaim(SecurityScopes.AccessAdminApiScope),
                ..additionalClaims
            ]);
    }

    public static ClaimsPrincipal BauUser()
    {
        // Give the BAU User the "BAU User" role, plus every Claim from the SecurityClaimTypes enum.
        var claims =
            ListOf(RoleClaim(RoleNames.BauUser))
            .Concat(EnumUtil.GetEnums<SecurityClaimTypes>().Select(c => GenericClaim(c.ToString())))
            .Append(ScopeClaim(SecurityScopes.AccessAdminApiScope))
            .Append(IdClaim(Guid.NewGuid()));

        return CreateClaimsPrincipal(claims.ToArray());
    }

    public static ClaimsPrincipal AnalystUser()
    {
        return CreateClaimsPrincipal(
            IdClaim(Guid.NewGuid()),
            RoleClaim(Analyst),
            GenericClaim(ApplicationAccessGranted.ToString()),
            GenericClaim(AnalystPagesAccessGranted.ToString()),
            GenericClaim(PrereleasePagesAccessGranted.ToString()),
            GenericClaim(CanViewPrereleaseContacts.ToString()),
            ScopeClaim(SecurityScopes.AccessAdminApiScope));
    }

    public static ClaimsPrincipal PreReleaseUser()
    {
        return CreateClaimsPrincipal(
            IdClaim(Guid.NewGuid()),
            RoleClaim(PrereleaseUser),
            GenericClaim(ApplicationAccessGranted.ToString()),
            GenericClaim(PrereleasePagesAccessGranted.ToString()),
            ScopeClaim(SecurityScopes.AccessAdminApiScope));
    }

    private static Claim IdClaim(Guid userId)
    {
        return new Claim(EesClaimTypes.LocalUserId, userId.ToString());
    }
}
