using GovUk.Education.ExploreEducationStatistics.Common.Security.AuthorizationHandlers;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Authorization;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers
{
    public class UpdateSpecificMethodologyRequirement : IAuthorizationRequirement
    {}

    public class UpdateSpecificMethodologyAuthorizationHandler : CompoundAuthorizationHandler<UpdateSpecificMethodologyRequirement, Methodology>
    {
        public UpdateSpecificMethodologyAuthorizationHandler() : base(
            new CanUpdateAllMethodologiesAuthorizationHandler())
        {}
    
        public class CanUpdateAllMethodologiesAuthorizationHandler 
            : EntityAuthorizationHandler<UpdateSpecificMethodologyRequirement, Methodology>
        {
            public CanUpdateAllMethodologiesAuthorizationHandler()
                : base(ctx =>
                    ctx.Entity.Status != MethodologyStatus.Approved && 
                    SecurityUtils.HasClaim(ctx.User, SecurityClaimTypes.UpdateAllMethodologies)
                )
            {}
        }
    }
}