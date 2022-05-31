using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Authorization;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers.AuthorizationHandlerUtil;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityClaimTypes;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.ReleaseApprovalStatus;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers
{
    public class DeleteSpecificReleaseRequirement : IAuthorizationRequirement
    {}
    
    public class DeleteSpecificReleaseAuthorizationHandler
        : AuthorizationHandler<DeleteSpecificReleaseRequirement, Release>
    {
        private readonly IUserPublicationRoleRepository _userPublicationRoleRepository;

        public DeleteSpecificReleaseAuthorizationHandler(IUserPublicationRoleRepository userPublicationRoleRepository)
        {
            _userPublicationRoleRepository = userPublicationRoleRepository;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            DeleteSpecificReleaseRequirement requirement,
            Release release)
        {
            if (!release.Amendment || release.ApprovalStatus == Approved)
            {
                return;
            }

            if (SecurityUtils.HasClaim(context.User, DeleteAllReleaseAmendments))
            {
                context.Succeed(requirement);
                return;
            }

            var publicationRoles = await _userPublicationRoleRepository.GetAllRolesByUserAndPublication(context.User.GetUserId(), release.PublicationId);

            if (ContainPublicationOwnerRole(publicationRoles))
            {
                context.Succeed(requirement);
            }
        }
    }
}
