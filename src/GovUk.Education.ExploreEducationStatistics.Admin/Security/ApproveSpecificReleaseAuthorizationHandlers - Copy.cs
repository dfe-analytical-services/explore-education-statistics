using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore.Internal;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security
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
            : base(context, roles => roles.Any(role => role.Role == ReleaseRole.Approver))
        {}
    }
}