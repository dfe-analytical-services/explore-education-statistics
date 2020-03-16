using GovUk.Education.ExploreEducationStatistics.Common.Security.AuthorizationHandlers;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Authorization;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers.AuthorizationHandlerUtil;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers
{
    public class DeleteSpecificReleaseRequirement : IAuthorizationRequirement
    {}
    
    public class DeleteSpecificReleaseAuthorizationHandler : CompoundAuthorizationHandler<DeleteSpecificReleaseRequirement, Release>
    {
        public DeleteSpecificReleaseAuthorizationHandler(ContentDbContext context) : base(
            new CanDeleteAllReleaseAmendmentsAuthorizationHandler(),
            new HasEditorRoleOnReleaseAmendmentAuthorizationHandler(context))
        {
            
        }
        
        public class CanDeleteAllReleaseAmendmentsAuthorizationHandler : 
            EntityAuthorizationHandler<DeleteSpecificReleaseRequirement, Release>
        {
            public CanDeleteAllReleaseAmendmentsAuthorizationHandler() 
                : base(ctx => 
                    ctx.Entity.Amendment 
                    && SecurityUtils.HasClaim(ctx.User, SecurityClaimTypes.DeleteAllReleaseAmendments)) {}
        }

        public class HasEditorRoleOnReleaseAmendmentAuthorizationHandler
            : HasRoleOnReleaseAuthorizationHandler<DeleteSpecificReleaseRequirement>
        {
            public HasEditorRoleOnReleaseAmendmentAuthorizationHandler(ContentDbContext context) 
                : base(context, ctx => 
                    ctx.Release.Amendment 
                    && ContainsEditorRole(ctx.Roles))
            {}
        }
    }
}