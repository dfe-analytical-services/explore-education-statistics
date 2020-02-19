using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Authorization;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers
{
    public class UpdateSpecificMethodologyRequirement : IAuthorizationRequirement
    {}

    public class UpdateSpecificMethodologyAuthorizationHandler : CompoundAuthorizationHandler<UpdateSpecificMethodologyRequirement, Methodology>
    {
        public UpdateSpecificMethodologyAuthorizationHandler(ContentDbContext context) : base(
            new UpdateSpecificMethodologyCanUpdateAllMethodologiesAuthorizationHandler())
        {
            
        }
    }
    
    public class UpdateSpecificMethodologyCanUpdateAllMethodologiesAuthorizationHandler 
        : EntityAuthorizationHandler<UpdateSpecificMethodologyRequirement, Methodology>
    {
        public UpdateSpecificMethodologyCanUpdateAllMethodologiesAuthorizationHandler()
            : base(ctx =>
                // TODO EES-1315 - represent Methodology status with enum
                ctx.Entity.Status != "Live" && 
                SecurityUtils.HasClaim(ctx.User, SecurityClaimTypes.UpdateAllMethodologies)
            )
        {
            
        }
    }
}