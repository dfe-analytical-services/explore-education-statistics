using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Authorization;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers.AuthorizationHandlerUtil;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityClaimTypes;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.ReleaseApprovalStatus;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers
{
    public class AssignPrereleaseContactsToSpecificReleaseRequirement : IAuthorizationRequirement
    {
    }

    public class AssignPrereleaseContactsToSpecificReleaseAuthorizationHandler
        : AuthorizationHandler<AssignPrereleaseContactsToSpecificReleaseRequirement, Release>
    {
        private readonly IUserPublicationRoleRepository _userPublicationRoleRepository;
        private readonly IUserReleaseRoleRepository _userReleaseRoleRepository;

        public AssignPrereleaseContactsToSpecificReleaseAuthorizationHandler(
            IUserPublicationRoleRepository userPublicationRoleRepository,
            IUserReleaseRoleRepository userReleaseRoleRepository)
        {
            _userPublicationRoleRepository = userPublicationRoleRepository;
            _userReleaseRoleRepository = userReleaseRoleRepository;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            AssignPrereleaseContactsToSpecificReleaseRequirement requirement,
            Release release)
        {
            if (release.ApprovalStatus != Approved)
            {
                return;
            }

            if (SecurityUtils.HasClaim(context.User, UpdateAllReleases))
            {
                context.Succeed(requirement);
                return;
            }

            var publicationRoles =
                await _userPublicationRoleRepository.GetAllRolesByUser(context.User.GetUserId(), release.PublicationId);

            if (ContainPublicationOwnerRole(publicationRoles))
            {
                context.Succeed(requirement);
                return;
            }
            
            var releaseRoles = await _userReleaseRoleRepository.GetAllRolesByUser(context.User.GetUserId(), release.Id);

            if (ContainsEditorOrApproverRole(releaseRoles))
            {
                context.Succeed(requirement);
            }
        }
    }
}
