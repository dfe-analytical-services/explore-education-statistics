using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Authorization;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers.AuthorizationHandlerUtil;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityClaimTypes;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers
{
    public class CreateReleaseForSpecificPublicationRequirement : IAuthorizationRequirement
    {
    }

    public class CreateReleaseForSpecificPublicationAuthorizationHandler 
        : AuthorizationHandler<CreateReleaseForSpecificPublicationRequirement, Publication>
    {
        private readonly IUserPublicationRoleRepository _userPublicationRoleRepository;

        public CreateReleaseForSpecificPublicationAuthorizationHandler(
            IUserPublicationRoleRepository userPublicationRoleRepository)
        {
            _userPublicationRoleRepository = userPublicationRoleRepository;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
            CreateReleaseForSpecificPublicationRequirement requirement,
            Publication publication)
        {
            if (SecurityUtils.HasClaim(context.User, CreateAnyRelease))
            {
                context.Succeed(requirement);
                return;
            }

            var publicationRoles =
                await _userPublicationRoleRepository.GetAllRolesByUserAndPublication(context.User.GetUserId(), publication.Id);

            if (ContainPublicationOwnerRole(publicationRoles))
            {
                context.Succeed(requirement);
            }
        }
    }
}
