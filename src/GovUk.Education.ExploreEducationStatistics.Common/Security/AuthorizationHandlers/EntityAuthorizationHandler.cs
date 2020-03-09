using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace GovUk.Education.ExploreEducationStatistics.Common.Security.AuthorizationHandlers
{
    public class EntityAuthorizationContext<TEntity>
    {
        public TEntity Entity { get; set; }
        
        public ClaimsPrincipal User { get; set; }
        
        public EntityAuthorizationContext(TEntity entity, ClaimsPrincipal user)
        {
            Entity = entity;
            User = user;
        }
    }
    
    public abstract class EntityAuthorizationHandler<TRequirement, TEntity> : AuthorizationHandler<TRequirement, TEntity> 
        where TRequirement : IAuthorizationRequirement
    {
        private readonly Predicate<EntityAuthorizationContext<TEntity>> _entityTest;

        protected EntityAuthorizationHandler(Predicate<EntityAuthorizationContext<TEntity>> entityTest)
        {
            _entityTest = entityTest;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext authContext,
            TRequirement requirement,
            TEntity entity)
        {
            if (_entityTest.Invoke(new EntityAuthorizationContext<TEntity>(entity, authContext.User))) 
            {
                authContext.Succeed(requirement);    
            }

            return Task.CompletedTask;
        }
    }
}