using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Authorization;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers.AuthorizationHandlerUtil;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers
{
    public class MarkSpecificReleaseAsDraftRequirement : IAuthorizationRequirement
    {}
    
    public class MarkSpecificReleaseAsDraftAuthorizationHandler : CompoundAuthorizationHandler<MarkSpecificReleaseAsDraftRequirement, Release>
    {
        public MarkSpecificReleaseAsDraftAuthorizationHandler(ContentDbContext context) : base(
            new MarkSpecificReleaseAsDraftCanMarkAllReleasesAsDraftAuthorizationHandler(),
            new MarkSpecificReleaseAsDraftHasRoleOnReleaseAuthorizationHandler(context))
        {
            
        }
    }
    
    public class MarkSpecificReleaseAsDraftCanMarkAllReleasesAsDraftAuthorizationHandler : ReleaseAuthorizationHandler<
        MarkSpecificReleaseAsDraftRequirement>
    {
        public MarkSpecificReleaseAsDraftCanMarkAllReleasesAsDraftAuthorizationHandler() 
            : base(ctx => 
                ctx.Release.Status != ReleaseStatus.Approved 
                && SecurityUtils.HasClaim(ctx.User, SecurityClaimTypes.MarkAllReleasesAsDraft)) {}
    }
    
    public class MarkSpecificReleaseAsDraftHasRoleOnReleaseAuthorizationHandler
        : HasRoleOnReleaseAuthorizationHandler<MarkSpecificReleaseAsDraftRequirement>
    {
        public MarkSpecificReleaseAsDraftHasRoleOnReleaseAuthorizationHandler(ContentDbContext context) 
            : base(context, ctx => ctx.Release.Status != ReleaseStatus.Approved && ContainsEditorRole(ctx.Roles))
        {}
    }
}