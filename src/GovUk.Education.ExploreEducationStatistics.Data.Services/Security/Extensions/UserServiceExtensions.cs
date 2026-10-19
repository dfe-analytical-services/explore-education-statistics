#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Security.Extensions;

public static class UserServiceExtensions
{
    public static async Task<Either<ActionResult, ReleaseFile>> CheckCanViewSubjectData(
        this IUserService userService,
        ReleaseFile releaseFile
    )
    {
        if (await userService.MatchesPolicy(releaseFile, DataSecurityPolicies.CanViewSubjectData))
        {
            return releaseFile;
        }

        return new ForbidResult();
    }
}
