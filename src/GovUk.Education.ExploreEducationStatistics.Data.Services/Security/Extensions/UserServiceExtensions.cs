#nullable enable
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Security.Extensions;

public static class UserServiceExtensions
{
    public static async Task<Either<ActionResult, ReleaseSubject>> CheckCanViewSubjectData(
        this IUserService userService,
        ReleaseSubject releaseSubject)
    {
        if (await userService.MatchesPolicy(releaseSubject, DataSecurityPolicies.CanViewSubjectData))
        {
            return releaseSubject;
        }

        return new ForbidResult();
    }
}
