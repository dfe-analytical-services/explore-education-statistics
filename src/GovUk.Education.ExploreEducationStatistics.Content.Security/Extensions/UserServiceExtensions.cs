#nullable enable
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Mvc;
using static GovUk.Education.ExploreEducationStatistics.Content.Security.ContentSecurityPolicies;

namespace GovUk.Education.ExploreEducationStatistics.Content.Security.Extensions
{
    public static class UserServiceExtensions
    {
        public static Task<Either<ActionResult, MethodologyVersion>> CheckCanViewMethodologyVersion(
            this IUserService userService,
            MethodologyVersion methodologyVersion)
        {
            return userService.CheckPolicy(methodologyVersion, CanViewSpecificMethodologyVersion);
        }

        public static Task<Either<ActionResult, Publication>> CheckCanViewPublication(
            this IUserService userService,
            Publication publication)
        {
            return userService.CheckPolicy(publication, CanViewSpecificPublication);
        }

        public static Task<Either<ActionResult, Release>> CheckCanViewRelease(
            this IUserService userService,
            Release release)
        {
            return userService.CheckPolicy(release, CanViewSpecificRelease);
        }
    }
}
