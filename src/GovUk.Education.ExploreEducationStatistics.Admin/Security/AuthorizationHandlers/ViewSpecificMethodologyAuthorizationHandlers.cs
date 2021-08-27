#nullable enable
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers
{
    public class ViewSpecificMethodologyRequirement : IAuthorizationRequirement
    {
    }

    public class ViewSpecificMethodologyAuthorizationHandler :
        AuthorizationHandler<ViewSpecificMethodologyRequirement, Methodology>
    {
        private readonly IMethodologyRepository _methodologyRepository;
        private readonly IUserPublicationRoleRepository _userPublicationRoleRepository;
        private readonly IUserReleaseRoleRepository _userReleaseRoleRepository;

        public ViewSpecificMethodologyAuthorizationHandler(
            IMethodologyRepository methodologyRepository,
            IUserPublicationRoleRepository userPublicationRoleRepository,
            IUserReleaseRoleRepository userReleaseRoleRepository)
        {
            _methodologyRepository = methodologyRepository;
            _userPublicationRoleRepository = userPublicationRoleRepository;
            _userReleaseRoleRepository = userReleaseRoleRepository;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
            ViewSpecificMethodologyRequirement requirement,
            Methodology methodology)
        {
            // If the user has a global Claim that allows them to access any Methodology, allow it.
            if (SecurityUtils.HasClaim(context.User, SecurityClaimTypes.AccessAllMethodologies))
            {
                context.Succeed(requirement);
                return;
            }

            var owningPublication =
                await _methodologyRepository.GetOwningPublicationByMethodologyParent(methodology.MethodologyParentId);

            // If the user is a Publication Owner of the Publication that owns this Methodology, they can view it.
            if (await _userPublicationRoleRepository.IsUserPublicationOwner(context.User.GetUserId(),
                owningPublication.Id))
            {
                context.Succeed(requirement);
                return;
            }

            // If the user is an Editor (Contributor, Lead) or an Approver of the latest (Live or non-Live) Release
            // of the owning Publication of this Methodology, they can view it.
            if (await _userReleaseRoleRepository.IsUserEditorOrApproverOnLatestRelease(
                context.User.GetUserId(),
                owningPublication.Id))
            {
                context.Succeed(requirement);
            }
        }
    }
}
