#nullable enable
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Authorization;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.PublicationRole;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers
{
    public class AdoptMethodologyForSpecificPublicationRequirement : IAuthorizationRequirement
    {
    }

    public class AdoptMethodologyForSpecificPublicationAuthorizationHandler
        : AuthorizationHandler<AdoptMethodologyForSpecificPublicationRequirement, Publication>
    {
        private readonly AuthorizationHandlerResourceRoleService _authorizationHandlerResourceRoleService;

        public AdoptMethodologyForSpecificPublicationAuthorizationHandler(
            AuthorizationHandlerResourceRoleService authorizationHandlerResourceRoleService)
        {
            _authorizationHandlerResourceRoleService = authorizationHandlerResourceRoleService;
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
            
            if (await _authorizationHandlerResourceRoleService
                    .HasRolesOnPublication(
                        context.User.GetUserId(),
                        publication.Id,
                        Owner))
            {
                context.Succeed(requirement);
            }
        }
    }
}
