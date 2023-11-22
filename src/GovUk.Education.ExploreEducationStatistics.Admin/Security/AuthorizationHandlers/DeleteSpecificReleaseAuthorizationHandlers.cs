using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Authorization;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityClaimTypes;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.PublicationRole;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.ReleaseApprovalStatus;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers
{
    public class DeleteSpecificReleaseRequirement : IAuthorizationRequirement
    {}
    
    public class DeleteSpecificReleaseAuthorizationHandler
        : AuthorizationHandler<DeleteSpecificReleaseRequirement, Release>
    {
        private readonly AuthorizationHandlerService _authorizationHandlerService;

        public DeleteSpecificReleaseAuthorizationHandler(
            AuthorizationHandlerService authorizationHandlerService)
        {
            _authorizationHandlerService = authorizationHandlerService;
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
            
            if (await _authorizationHandlerService
                    .HasRolesOnPublication(
                        context.User.GetUserId(),
                        release.PublicationId,
                        Owner))
            {
                context.Succeed(requirement);
            }
        }
    }
}
