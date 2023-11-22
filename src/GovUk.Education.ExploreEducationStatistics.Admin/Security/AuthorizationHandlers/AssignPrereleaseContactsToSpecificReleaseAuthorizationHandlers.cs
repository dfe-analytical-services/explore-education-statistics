using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Authorization;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers.AuthorizationHandlerService;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityClaimTypes;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers
{
    public class AssignPrereleaseContactsToSpecificReleaseRequirement : IAuthorizationRequirement
    {
    }

    public class AssignPrereleaseContactsToSpecificReleaseAuthorizationHandler
        : AuthorizationHandler<AssignPrereleaseContactsToSpecificReleaseRequirement, Release>
    {
        private readonly AuthorizationHandlerService _authorizationHandlerService;

        public AssignPrereleaseContactsToSpecificReleaseAuthorizationHandler(
            AuthorizationHandlerService authorizationHandlerService)
        {
            _authorizationHandlerService = authorizationHandlerService;
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
            
            if (await _authorizationHandlerService
                    .HasRolesOnPublicationOrRelease(
                        context.User.GetUserId(),
                        release.PublicationId,
                        release.Id,
                        ListOf(PublicationRole.Owner, PublicationRole.Approver),
                        ReleaseEditorAndApproverRoles))
            {
                context.Succeed(requirement);
            }
        }
    }
}
