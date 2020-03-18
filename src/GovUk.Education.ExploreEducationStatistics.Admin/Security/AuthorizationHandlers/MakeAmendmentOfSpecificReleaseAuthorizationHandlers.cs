using GovUk.Education.ExploreEducationStatistics.Common.Security.AuthorizationHandlers;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Authorization;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers.AuthorizationHandlerUtil;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers
{
    public class MakeAmendmentOfSpecificReleaseRequirement : IAuthorizationRequirement
    {}
    
    public class MakeAmendmentOfSpecificReleaseAuthorizationHandler : CompoundAuthorizationHandler<MakeAmendmentOfSpecificReleaseRequirement, Release>
    {
        public MakeAmendmentOfSpecificReleaseAuthorizationHandler(ContentDbContext context) : base(
            new CanMakeAmendmentOfAllReleasesAuthorizationHandler(),
            new HasEditorRoleOnReleaseAuthorizationHandler(context))
        {
            
        }
        
        public class CanMakeAmendmentOfAllReleasesAuthorizationHandler : 
            EntityAuthorizationHandler<MakeAmendmentOfSpecificReleaseRequirement, Release>
        {
            public CanMakeAmendmentOfAllReleasesAuthorizationHandler() 
                : base(ctx => 
                    ctx.Entity.Live 
                    && SecurityUtils.HasClaim(ctx.User, SecurityClaimTypes.MakeAmendmentsOfAllReleases)) {}
        }

        public class HasEditorRoleOnReleaseAuthorizationHandler
            : HasRoleOnReleaseAuthorizationHandler<MakeAmendmentOfSpecificReleaseRequirement>
        {
            public HasEditorRoleOnReleaseAuthorizationHandler(ContentDbContext context) 
                : base(context, ctx => 
                    ctx.Release.Live 
                    && ContainsEditorRole(ctx.Roles))
            {}
        }
    }
}