#nullable enable
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Authorization;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityClaimTypes;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers
{
    public class ManageExternalMethodologyForSpecificPublicationRequirement : IAuthorizationRequirement
    {
    }

    public class ManageExternalMethodologyForSpecificPublicationAuthorizationHandler 
        : AuthorizationHandler<ManageExternalMethodologyForSpecificPublicationRequirement, Publication>
    {
        private readonly IUserPublicationRoleRepository _userPublicationRoleRepository;

        public ManageExternalMethodologyForSpecificPublicationAuthorizationHandler(
            IUserPublicationRoleRepository userPublicationRoleRepository)
        {
            _userPublicationRoleRepository = userPublicationRoleRepository;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
            ManageExternalMethodologyForSpecificPublicationRequirement requirement,
            Publication publication)
        {
            if (SecurityUtils.HasClaim(context.User, CreateAnyMethodology))
            {
                context.Succeed(requirement);
                return;
            }

            if (await _userPublicationRoleRepository.IsUserPublicationOwner(context, publication.Id))
            {
                context.Succeed(requirement);
            }
        }
    }
}
