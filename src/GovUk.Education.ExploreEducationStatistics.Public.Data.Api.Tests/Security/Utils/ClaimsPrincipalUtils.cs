using System.Security.Claims;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Security;
using static GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils.ClaimsPrincipalUtils;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Tests.Security.Utils;

public static class ClaimsPrincipalUtils
{
    public static ClaimsPrincipal AdminAccessUser()
    {
        return CreateClaimsPrincipal(RoleClaim(SecurityConstants.AdminAccessAppRole));
    }
    
    public static ClaimsPrincipal UnknownRoleUser()
    {
        return CreateClaimsPrincipal(RoleClaim("Unknown Role"));
    }
}
