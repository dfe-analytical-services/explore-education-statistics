using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security
{
    public interface IUserService
    {
        Guid GetUserId();

        Task<bool> MatchesPolicy(Enum policy);

        Task<bool> MatchesPolicy(object resource, Enum policy);
    }

    public static class UserServiceExtensions
    {
        public static async Task<Either<ActionResult, TResource>> CheckPolicy<TResource, TPolicy>(
            this IUserService userService,
            TResource resource,
            TPolicy policy)
            where TPolicy : Enum
        {
            var result = await userService.MatchesPolicy(resource, policy);

            if (result)
            {
                return resource;
            }

            return new ForbidResult();
        }

        public static async Task<Either<ActionResult, Unit>> CheckPolicy<TPolicy>(
            this IUserService userService,
            TPolicy policy)
            where TPolicy : Enum
        {
            var result = await userService.MatchesPolicy(policy);

            if (result)
            {
                return Unit.Instance;
            }

            return new ForbidResult();
        }
    }
}