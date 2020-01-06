using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Authorization;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers
{
    public class UpdateSpecificReleaseRequirement : IAuthorizationRequirement
    {}
    
    public class UpdateSpecificReleaseCanUpdateAllReleasesAuthorizationHandler : HasClaimAuthorizationHandler<
        UpdateSpecificReleaseRequirement>
    {
        public UpdateSpecificReleaseCanUpdateAllReleasesAuthorizationHandler() 
            : base(SecurityClaimTypes.UpdateAllReleases) {}
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
            : base(context, roles => EditorRoles.Intersect(roles.Select(r => r.Role)).Any())
        {}
    }
}