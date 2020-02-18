using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Authorization;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers
{
    public class MarkSpecificMethodologyAsDraftRequirement : IAuthorizationRequirement
    {}
    
    public class MarkSpecificMethodologyAsDraftAuthorizationHandler : CompoundAuthorizationHandler<MarkSpecificMethodologyAsDraftRequirement, Methodology>
    {
        public MarkSpecificMethodologyAsDraftAuthorizationHandler(ContentDbContext context) : base(
            new CanMarkAllMethodologiesAsDraftAuthorizationHandler())
        {}
    }
    
    public class CanMarkAllMethodologiesAsDraftAuthorizationHandler : 
        EntityAuthorizationHandler<MarkSpecificMethodologyAsDraftRequirement, Methodology>
    {
        public CanMarkAllMethodologiesAsDraftAuthorizationHandler() 
            : base(ctx => 
                ctx.Entity.Status != MethodologyStatus.Approved 
                && SecurityUtils.HasClaim(ctx.User, SecurityClaimTypes.ApproveAllMethodologies)) {}
    }
}