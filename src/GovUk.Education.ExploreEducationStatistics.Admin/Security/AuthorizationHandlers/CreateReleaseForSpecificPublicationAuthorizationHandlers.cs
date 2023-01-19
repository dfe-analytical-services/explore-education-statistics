#nullable enable
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Authorization;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityClaimTypes;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.PublicationRole;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers
{
    public class CreateReleaseForSpecificPublicationRequirement : IAuthorizationRequirement
    {
    }

    public class CreateReleaseForSpecificPublicationAuthorizationHandler
        : AuthorizationHandler<CreateReleaseForSpecificPublicationRequirement, Publication>
    {
        private readonly AuthorizationHandlerResourceRoleService _authorizationHandlerResourceRoleService;

        public CreateReleaseForSpecificPublicationAuthorizationHandler(
            AuthorizationHandlerResourceRoleService authorizationHandlerResourceRoleService)
        {
            _authorizationHandlerResourceRoleService = authorizationHandlerResourceRoleService;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
            CreateReleaseForSpecificPublicationRequirement requirement,
            Publication publication)
        {
            // No user is allowed to create a new release of an archived publication
            if (publication.SupersededById.HasValue)
            {
                return;
            }

            if (SecurityUtils.HasClaim(context.User, CreateAnyRelease))
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
