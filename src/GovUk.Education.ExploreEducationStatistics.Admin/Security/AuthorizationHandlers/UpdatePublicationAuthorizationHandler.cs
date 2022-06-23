using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Authorization;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityClaimTypes;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.PublicationRole;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers
{
    public class UpdatePublicationRequirement : IAuthorizationRequirement
    {
    }

    public class UpdatePublicationAuthorizationHandler : AuthorizationHandler<UpdatePublicationRequirement, Publication>
    {
        private readonly AuthorizationHandlerResourceRoleService _authorizationHandlerResourceRoleService;

        public UpdatePublicationAuthorizationHandler(
            AuthorizationHandlerResourceRoleService authorizationHandlerResourceRoleService)
        {
            _authorizationHandlerResourceRoleService = authorizationHandlerResourceRoleService;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
            UpdatePublicationRequirement requirement,
            Publication publication)
        {
            if (SecurityUtils.HasClaim(context.User, UpdateAllPublications))
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
