using GovUk.Education.ExploreEducationStatistics.Common.Security.AuthorizationHandlers;
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
            new CanApproveAllReleasesAuthorizationHandler(),
            new HasApproverRoleOnReleaseAuthorizationHandler(context))
        {
            
        }
        
        public class CanApproveAllReleasesAuthorizationHandler : 
            EntityAuthorizationHandler<ApproveSpecificReleaseRequirement, Release>
        {
            public CanApproveAllReleasesAuthorizationHandler() 
                : base(ctx => 
                    ctx.Entity.Status != ReleaseStatus.Approved 
                    && SecurityUtils.HasClaim(ctx.User, SecurityClaimTypes.ApproveAllReleases)) {}
        }

        public class HasApproverRoleOnReleaseAuthorizationHandler
            : HasRoleOnReleaseAuthorizationHandler<ApproveSpecificReleaseRequirement>
        {
            public HasApproverRoleOnReleaseAuthorizationHandler(ContentDbContext context) 
                : base(context, ctx => ctx.Release.Status != ReleaseStatus.Approved && ContainsApproverRole(ctx.Roles))
            {}
        }
    }
}