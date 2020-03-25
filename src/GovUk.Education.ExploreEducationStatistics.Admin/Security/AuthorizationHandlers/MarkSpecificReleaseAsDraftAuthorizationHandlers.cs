using GovUk.Education.ExploreEducationStatistics.Common.Security.AuthorizationHandlers;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Authorization;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers.AuthorizationHandlerUtil;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers
{
    public class MarkSpecificReleaseAsDraftRequirement : IAuthorizationRequirement
    {}
    
    public class MarkSpecificReleaseAsDraftAuthorizationHandler 
        : CompoundAuthorizationHandler<MarkSpecificReleaseAsDraftRequirement, Release>
    {
        public MarkSpecificReleaseAsDraftAuthorizationHandler(ContentDbContext context) : base(
            new CanMarkAllReleasesAsDraftAuthorizationHandler(),
            new HasEditorRoleOnReleaseAuthorizationHandler(context))
        {
            
        }
        
        public class CanMarkAllReleasesAsDraftAuthorizationHandler 
            : EntityAuthorizationHandler<MarkSpecificReleaseAsDraftRequirement, Release>
        {
            public CanMarkAllReleasesAsDraftAuthorizationHandler() 
                : base(ctx => 
                    ctx.Entity.Status != ReleaseStatus.Approved 
                    && SecurityUtils.HasClaim(ctx.User, SecurityClaimTypes.MarkAllReleasesAsDraft)) {}
        }
    
        public class HasEditorRoleOnReleaseAuthorizationHandler
            : HasRoleOnReleaseAuthorizationHandler<MarkSpecificReleaseAsDraftRequirement>
        {
            public HasEditorRoleOnReleaseAuthorizationHandler(ContentDbContext context) 
                : base(context, ctx => ctx.Release.Status != ReleaseStatus.Approved && ContainsEditorRole(ctx.Roles))
            {}
        }
    }
}