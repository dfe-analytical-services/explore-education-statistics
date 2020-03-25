using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using static System.Activator;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers
{
    public class DelegatingAuthorizationHandler<TRequirement, TEntity, TDelegateRequirement, TDelegateEntity> : AuthorizationHandler<TRequirement, TEntity>
        where TRequirement : IAuthorizationRequirement
        where TDelegateRequirement : IAuthorizationRequirement
        where TEntity : class
        where TDelegateEntity : class
    {
        private readonly AuthorizationHandler<TDelegateRequirement, TDelegateEntity> _delegateHandler;
        private readonly Func<TEntity, TDelegateEntity> _delegateResourceFn;

        public DelegatingAuthorizationHandler(
            AuthorizationHandler<TDelegateRequirement, TDelegateEntity> delegateHandler,
            Func<TEntity, TDelegateEntity> delegateResourceFn)
        {
            _delegateHandler = delegateHandler;
            _delegateResourceFn = delegateResourceFn;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, TRequirement requirement, TEntity resource)
        {
            IAuthorizationRequirement delegateRequirement = CreateInstance<TDelegateRequirement>();
            var delegateContext = new AuthorizationHandlerContext(new[] {delegateRequirement}, context.User, _delegateResourceFn.Invoke(resource));
            await _delegateHandler.HandleAsync(delegateContext);
            if (delegateContext.HasSucceeded)
            {
                context.Succeed(requirement);
            }
        }
    }
}