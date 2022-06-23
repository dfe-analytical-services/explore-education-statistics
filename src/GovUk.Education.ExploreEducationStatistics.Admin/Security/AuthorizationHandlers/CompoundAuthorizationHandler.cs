using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers
{
    public class CompoundAuthorizationHandler<TRequirement, TEntity> : AuthorizationHandler<TRequirement, TEntity> 
        where TRequirement : IAuthorizationRequirement
        where TEntity : class
    {
        private readonly IAuthorizationHandler[] _handlers;

        protected CompoundAuthorizationHandler(params IAuthorizationHandler[] handlers)
        {
            _handlers = handlers;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, TRequirement requirement, TEntity resource)
        {
            foreach (var handler in _handlers)
            {
                await handler.HandleAsync(context);

                if (context.HasSucceeded)
                {
                    return;
                }
            }
        }
    }
}