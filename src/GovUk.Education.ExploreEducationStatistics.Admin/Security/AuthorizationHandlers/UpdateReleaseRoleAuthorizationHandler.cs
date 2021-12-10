#nullable enable
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Authorization;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityClaimTypes;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers
{
    public class UpdateReleaseRoleRequirement : IAuthorizationRequirement
    {
    }

    public class UpdateReleaseRoleAuthorizationHandler :
        AuthorizationHandler<UpdateReleaseRoleRequirement, Tuple<Publication, ReleaseRole>>
    {
        private readonly IUserPublicationRoleRepository _userPublicationRoleRepository;

        public UpdateReleaseRoleAuthorizationHandler(IUserPublicationRoleRepository userPublicationRoleRepository)
        {
            _userPublicationRoleRepository = userPublicationRoleRepository;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
            UpdateReleaseRoleRequirement requirement,
            Tuple<Publication, ReleaseRole> tuple)
        {
            var (publication, releaseRole) = tuple;

            if (SecurityUtils.HasClaim(context.User, ManageAnyUser))
            {
                context.Succeed(requirement);
                return;
            }

            if (releaseRole == ReleaseRole.Contributor
                && await _userPublicationRoleRepository
                    .IsUserPublicationOwner(context.User.GetUserId(), publication.Id))
            {
                context.Succeed(requirement);
            }
        }
    }
}
