using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Common.Security.AuthorizationHandlers;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Authorization;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers.AuthorizationHandlerUtil;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers
{
    public class AssignPrereleaseContactsToSpecificReleaseRequirement : IAuthorizationRequirement
    {}

    public class AssignPrereleaseContactsToSpecificReleaseAuthorizationHandler : 
        CompoundAuthorizationHandler<AssignPrereleaseContactsToSpecificReleaseRequirement, Release>
    {
        public AssignPrereleaseContactsToSpecificReleaseAuthorizationHandler(ContentDbContext context) : base(
            new AssignPrereleaseContactsToSpecificReleaseCanUpdateAllReleasesAuthorizationHandler(),
            new AssignPrereleaseContactsToSpecificReleaseHasUpdaterRoleOnReleaseAuthorizationHandler(context))
        {
            
        }
    }
    
    public class AssignPrereleaseContactsToSpecificReleaseCanUpdateAllReleasesAuthorizationHandler : 
        EntityAuthorizationHandler<AssignPrereleaseContactsToSpecificReleaseRequirement, Release>
    {
        public AssignPrereleaseContactsToSpecificReleaseCanUpdateAllReleasesAuthorizationHandler()
            : base(ctx =>
                ctx.Entity.Status == ReleaseStatus.Approved && 
                SecurityUtils.HasClaim(ctx.User, SecurityClaimTypes.UpdateAllReleases)
            )
        {
            
        }
    }

    public class AssignPrereleaseContactsToSpecificReleaseHasUpdaterRoleOnReleaseAuthorizationHandler
        : HasRoleOnReleaseAuthorizationHandler<AssignPrereleaseContactsToSpecificReleaseRequirement>
    {
        public AssignPrereleaseContactsToSpecificReleaseHasUpdaterRoleOnReleaseAuthorizationHandler(ContentDbContext context) 
            : base(context, ctx => ctx.Release.Status == ReleaseStatus.Approved && ContainsEditorRole(ctx.Roles))
        {}
    }
}