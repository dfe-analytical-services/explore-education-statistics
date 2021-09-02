#nullable enable
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Authorization;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityClaimTypes;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers
{
    public class DropMethodologyLinkRequirement : IAuthorizationRequirement
    {
    }

    public class DropMethodologyLinkAuthorizationHandler
        : AuthorizationHandler<DropMethodologyLinkRequirement, PublicationMethodology>
    {
        private readonly IUserPublicationRoleRepository _userPublicationRoleRepository;

        public DropMethodologyLinkAuthorizationHandler(
            IUserPublicationRoleRepository userPublicationRoleRepository)
        {
            _userPublicationRoleRepository = userPublicationRoleRepository;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            DropMethodologyLinkRequirement requirement,
            PublicationMethodology link)
        {
            if (link.Owner)
            {
                // No user is allowed to drop the link between a methodology and its owning publication 
                return;
            }

            // Allow users who can adopt methodologies to also drop them

            if (SecurityUtils.HasClaim(context.User, AdoptAnyMethodology))
            {
                context.Succeed(requirement);
                return;
            }

            if (await _userPublicationRoleRepository.IsUserPublicationOwner(
                context.User.GetUserId(),
                link.PublicationId))
            {
                context.Succeed(requirement);
            }
        }
    }
}
