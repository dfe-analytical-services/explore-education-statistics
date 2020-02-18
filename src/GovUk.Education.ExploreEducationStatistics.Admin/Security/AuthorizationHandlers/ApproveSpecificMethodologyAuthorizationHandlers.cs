using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Authorization;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers
{
    public class ApproveSpecificMethodologyRequirement : IAuthorizationRequirement
    {}
    
    public class ApproveSpecificMethodologyAuthorizationHandler : CompoundAuthorizationHandler<ApproveSpecificMethodologyRequirement, Methodology>
    {
        public ApproveSpecificMethodologyAuthorizationHandler() : base(
            new CanApproveAllMethodologiesAuthorizationHandler())
        {
            
        }
    }
    
    public class CanApproveAllMethodologiesAuthorizationHandler : 
        EntityAuthorizationHandler<ApproveSpecificMethodologyRequirement, Methodology>
    {
        public CanApproveAllMethodologiesAuthorizationHandler() 
            : base(ctx => 
                ctx.Entity.Status != MethodologyStatus.Approved 
                && SecurityUtils.HasClaim(ctx.User, SecurityClaimTypes.ApproveAllMethodologies)) {}
    }
}