#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Authorization;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityClaimTypes;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.PublicationRole;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers
{
    public class UpdateReleaseRoleRequirement : IAuthorizationRequirement
    {
    }

    public class UpdateReleaseRoleAuthorizationHandler :
        AuthorizationHandler<UpdateReleaseRoleRequirement, Tuple<Publication, ReleaseRole>>
    {
        private readonly AuthorizationHandlerResourceRoleService _authorizationHandlerResourceRoleService;

        public UpdateReleaseRoleAuthorizationHandler(
            AuthorizationHandlerResourceRoleService authorizationHandlerResourceRoleService)
        {
            _authorizationHandlerResourceRoleService = authorizationHandlerResourceRoleService;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            UpdateReleaseRoleRequirement requirement,
            Tuple<Publication, ReleaseRole> tuple)
        {
            var (publication, releaseRole) = tuple;

            if (SecurityUtils.HasClaim(context.User, ManageAnyUser))
            {
                context.Succeed(requirement);
                return;
            }

            if (releaseRole == ReleaseRole.Contributor)
            {
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
}
