using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Authorization;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers
{
    public class ApproveSpecificReleaseRequirement : IAuthorizationRequirement
    {}
    
    public class ApproveSpecificReleaseCanApproveAllReleasesAuthorizationHandler : HasClaimAuthorizationHandler<
        ApproveSpecificReleaseRequirement>
    {
        public ApproveSpecificReleaseCanApproveAllReleasesAuthorizationHandler() 
            : base(SecurityClaimTypes.ApproveAllReleases) {}
    }

    public class ApproveSpecificReleaseHasApproverRoleOnReleaseAuthorizationHandler
        : HasRoleOnReleaseAuthorizationHandler<ApproveSpecificReleaseRequirement>
    {
        public ApproveSpecificReleaseHasApproverRoleOnReleaseAuthorizationHandler(ContentDbContext context) 
            : base(context, ctx => ctx.Roles.Any(role => role.Role == ReleaseRole.Approver))
        {}
    }
}