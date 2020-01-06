using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Authorization;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers
{
    public class UpdateSpecificReleaseRequirement : IAuthorizationRequirement
    {}

    public class UpdateSpecificReleaseCanUpdateAllReleasesAuthorizationHandler : ReleaseAuthorizationHandler<
        UpdateSpecificReleaseRequirement>
    {
        public UpdateSpecificReleaseCanUpdateAllReleasesAuthorizationHandler()
            : base(ctx =>
                SecurityUtils.HasClaim(ctx.User, SecurityClaimTypes.UpdateAllReleases)
                && ctx.Release.Status != ReleaseStatus.Approved
            )
        {
            
        }
}

    public class UpdateSpecificReleaseHasUpdaterRoleOnReleaseAuthorizationHandler
        : HasRoleOnReleaseAuthorizationHandler<UpdateSpecificReleaseRequirement>
    {
        private static readonly List<ReleaseRole> EditorRoles = new List<ReleaseRole>
        {
            ReleaseRole.Contributor,
            ReleaseRole.Approver,
            ReleaseRole.Lead
        };
        
        public UpdateSpecificReleaseHasUpdaterRoleOnReleaseAuthorizationHandler(ContentDbContext context) 
            : base(context, ctx =>
            {
                return ctx.Release.Status != ReleaseStatus.Approved && 
                       EditorRoles.Intersect(ctx.Roles.Select(r => r.Role)).Any();
            })
        {}
    }
}