using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Authorization;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers.AuthorizationHandlerUtil;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers
{
    public class UpdateSpecificReleaseRequirement : IAuthorizationRequirement
    {}

    public class UpdateSpecificReleaseAuthorizationHandler : CompoundAuthorizationHandler<UpdateSpecificReleaseRequirement, Release>
    {
        public UpdateSpecificReleaseAuthorizationHandler(ContentDbContext context) : base(
            new UpdateSpecificEntityCanUpdateAllReleasesAuthorizationHandler(),
            new UpdateSpecificReleaseHasUpdaterRoleOnReleaseAuthorizationHandler(context))
        {
            
        }
    }
    
    public class UpdateSpecificEntityCanUpdateAllReleasesAuthorizationHandler : EntityAuthorizationHandler<UpdateSpecificReleaseRequirement, Release>
    {
        public UpdateSpecificEntityCanUpdateAllReleasesAuthorizationHandler()
            : base(ctx =>
                ctx.Entity.Status != ReleaseStatus.Approved && 
                SecurityUtils.HasClaim(ctx.User, SecurityClaimTypes.UpdateAllReleases)
            )
        {
            
        }
    }

    public class UpdateSpecificReleaseHasUpdaterRoleOnReleaseAuthorizationHandler
        : HasRoleOnReleaseAuthorizationHandler<UpdateSpecificReleaseRequirement>
    {
        public UpdateSpecificReleaseHasUpdaterRoleOnReleaseAuthorizationHandler(ContentDbContext context) 
            : base(context, ctx => ctx.Release.Status != ReleaseStatus.Approved && ContainsEditorRole(ctx.Roles))
        {}
    }
}