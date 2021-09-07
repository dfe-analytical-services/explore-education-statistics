#nullable enable
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using Microsoft.AspNetCore.Authorization;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityClaimTypes;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers
{
    public class DeleteSpecificMethodologyRequirement : IAuthorizationRequirement
    {
    }

    public class DeleteSpecificMethodologyAuthorizationHandler
        : AuthorizationHandler<DeleteSpecificMethodologyRequirement, MethodologyVersion>
    {
        private readonly IMethodologyVersionRepository _methodologyVersionRepository;
        private readonly IMethodologyRepository _methodologyRepository;
        private readonly IUserPublicationRoleRepository _userPublicationRoleRepository;

        public DeleteSpecificMethodologyAuthorizationHandler(
            IMethodologyVersionRepository methodologyVersionRepository,
            IMethodologyRepository methodologyRepository,
            IUserPublicationRoleRepository userPublicationRoleRepository)
        {
            _methodologyVersionRepository = methodologyVersionRepository;
            _methodologyRepository = methodologyRepository;
            _userPublicationRoleRepository = userPublicationRoleRepository;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            DeleteSpecificMethodologyRequirement requirement,
            MethodologyVersion methodologyVersion)
        {
            // If the Methodology is already public, it cannot be deleted.
            if (await _methodologyVersionRepository.IsPubliclyAccessible(methodologyVersion.Id))
            {
                return;
            }

            // If the Methodology is the first version added to a Publication and is still in Draft, or if it is a 
            // subsequent version but is still an amendment, it can potentially be deleted.  Otherwise it cannot.
            if (!methodologyVersion.Amendment && !methodologyVersion.DraftFirstVersion)
            {
                return;
            }

            if (SecurityUtils.HasClaim(context.User, DeleteAllMethodologies))
            {
                context.Succeed(requirement);
                return;
            }

            var owningPublication =
                await _methodologyRepository.GetOwningPublication(methodologyVersion.MethodologyId);

            // If the user is a Publication Owner of the Publication that owns this Methodology, they can delete it.
            if (await _userPublicationRoleRepository.IsUserPublicationOwner(
                context.User.GetUserId(),
                owningPublication.Id))
            {
                context.Succeed(requirement);
            }
        }
    }
}
