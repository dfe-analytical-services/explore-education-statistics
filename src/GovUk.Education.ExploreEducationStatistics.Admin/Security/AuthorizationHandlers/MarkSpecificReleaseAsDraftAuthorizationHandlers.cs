using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Authorization;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers.AuthorizationHandlerUtil;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers
{
    public class MarkSpecificReleaseAsDraftRequirement : IAuthorizationRequirement
    {}
    
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