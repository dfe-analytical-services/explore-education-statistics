using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Authorization;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers.AuthorizationHandlerResourceRoleService;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityClaimTypes;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.PublicationRole;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers
{
    public class AssignPrereleaseContactsToSpecificReleaseRequirement : IAuthorizationRequirement
    {
    }

    public class AssignPrereleaseContactsToSpecificReleaseAuthorizationHandler
        : AuthorizationHandler<AssignPrereleaseContactsToSpecificReleaseRequirement, Release>
    {
        private readonly AuthorizationHandlerResourceRoleService _authorizationHandlerResourceRoleService;

        public AssignPrereleaseContactsToSpecificReleaseAuthorizationHandler(
            AuthorizationHandlerResourceRoleService authorizationHandlerResourceRoleService)
        {
            _authorizationHandlerResourceRoleService = authorizationHandlerResourceRoleService;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            AssignPrereleaseContactsToSpecificReleaseRequirement requirement,
            Release release)
        {
            if (SecurityUtils.HasClaim(context.User, UpdateAllReleases))
            {
                context.Succeed(requirement);
                return;
            }
            
            if (await _authorizationHandlerResourceRoleService
                    .HasRolesOnPublicationOrRelease(
                        context.User.GetUserId(),
                        release.PublicationId,
                        release.Id,
                        ListOf(Owner, Approver),
                        ReleaseEditorAndApproverRoles))
            {
                context.Succeed(requirement);
            }
        }
    }
}
