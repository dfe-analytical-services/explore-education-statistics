using GovUk.Education.ExploreEducationStatistics.Common.Security.AuthorizationHandlers;
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
            new CanUpdateAllReleasesAuthorizationHandler(),
            new HasEditorRoleOnReleaseAuthorizationHandler(context))
        {
            
        }
    
        public class CanUpdateAllReleasesAuthorizationHandler : EntityAuthorizationHandler<UpdateSpecificReleaseRequirement, Release>
        {
            public CanUpdateAllReleasesAuthorizationHandler()
                : base(ctx =>
                    ctx.Entity.Status != ReleaseStatus.Approved && 
                    SecurityUtils.HasClaim(ctx.User, SecurityClaimTypes.UpdateAllReleases)
                )
            {
            
            }
        }

        public class HasEditorRoleOnReleaseAuthorizationHandler
            : HasRoleOnReleaseAuthorizationHandler<UpdateSpecificReleaseRequirement>
        {
            public HasEditorRoleOnReleaseAuthorizationHandler(ContentDbContext context) 
                : base(context, ctx => ctx.Release.Status != ReleaseStatus.Approved && ContainsEditorRole(ctx.Roles))
            {}
        }
    }
}