#nullable enable
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Authorization;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers
{
    public class AdoptMethodologyForSpecificPublicationRequirement : IAuthorizationRequirement
    {
    }

    public class AdoptMethodologyForSpecificPublicationAuthorizationHandler
        : AuthorizationHandler<AdoptMethodologyForSpecificPublicationRequirement, Publication>
    {
        private readonly IUserPublicationRoleRepository _userPublicationRoleRepository;

        public AdoptMethodologyForSpecificPublicationAuthorizationHandler(
            IUserPublicationRoleRepository userPublicationRoleRepository)
        {
            _userPublicationRoleRepository = userPublicationRoleRepository;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            AdoptMethodologyForSpecificPublicationRequirement requirement,
            Publication publication)
        {
            if (SecurityUtils.HasClaim(context.User, SecurityClaimTypes.AdoptAnyMethodology))
            {
                context.Succeed(requirement);
                return;
            }

            if (await _userPublicationRoleRepository.IsUserPublicationOwner(context.User.GetUserId(), publication.Id))
            {
                context.Succeed(requirement);
            }
        }
    }
}
