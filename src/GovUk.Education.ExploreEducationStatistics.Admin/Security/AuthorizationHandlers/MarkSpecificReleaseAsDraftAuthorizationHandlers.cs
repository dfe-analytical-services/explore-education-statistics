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
    
    public class MarkSpecificReleaseAsDraftCanMarkAllReleasesAsDraftAuthorizationHandler : HasClaimAuthorizationHandler<
        MarkSpecificReleaseAsDraftRequirement>
    {
        public MarkSpecificReleaseAsDraftCanMarkAllReleasesAsDraftAuthorizationHandler() 
            : base(SecurityClaimTypes.MarkAllReleasesAsDraft) {}
    }
    
    public class MarkSpecificReleaseAsDraftHasRoleOnReleaseAuthorizationHandler
        : HasRoleOnReleaseAuthorizationHandler<MarkSpecificReleaseAsDraftRequirement>
    {
        public MarkSpecificReleaseAsDraftHasRoleOnReleaseAuthorizationHandler(ContentDbContext context) 
            : base(context, ctx => ContainsEditorRole(ctx.Roles))
        {}
    }
}