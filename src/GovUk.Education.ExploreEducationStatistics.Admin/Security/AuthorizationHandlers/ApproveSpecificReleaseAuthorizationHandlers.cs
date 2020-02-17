using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Authorization;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers.AuthorizationHandlerUtil;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers
{
    public class ApproveSpecificReleaseRequirement : IAuthorizationRequirement
    {}
    
    public class ApproveSpecificReleaseAuthorizationHandler : CompoundAuthorizationHandler<ApproveSpecificReleaseRequirement, Release>
    {
        public ApproveSpecificReleaseAuthorizationHandler(ContentDbContext context) : base(
            new ApproveSpecificReleaseCanApproveAllEntitiesAuthorizationHandler(),
            new ApproveSpecificReleaseHasApproverRoleOnReleaseAuthorizationHandler(context))
        {
            
        }
    }
    
    public class ApproveSpecificReleaseCanApproveAllEntitiesAuthorizationHandler : 
        EntityAuthorizationHandler<ApproveSpecificReleaseRequirement, Release>
    {
        public ApproveSpecificReleaseCanApproveAllEntitiesAuthorizationHandler() 
            : base(ctx => 
                ctx.Entity.Status != ReleaseStatus.Approved 
                && SecurityUtils.HasClaim(ctx.User, SecurityClaimTypes.ApproveAllReleases)) {}
    }

    public class ApproveSpecificReleaseHasApproverRoleOnReleaseAuthorizationHandler
        : HasRoleOnReleaseAuthorizationHandler<ApproveSpecificReleaseRequirement>
    {
        public ApproveSpecificReleaseHasApproverRoleOnReleaseAuthorizationHandler(ContentDbContext context) 
            : base(context, ctx => ctx.Release.Status != ReleaseStatus.Approved && ContainsApproverRole(ctx.Roles))
        {}
    }
    
}