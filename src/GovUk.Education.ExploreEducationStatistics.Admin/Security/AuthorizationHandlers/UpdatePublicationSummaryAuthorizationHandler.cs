#nullable enable
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Authorization;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityClaimTypes;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.PublicationRole;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers
{
    public class UpdatePublicationSummaryRequirement : IAuthorizationRequirement
    {
    }

    public class UpdatePublicationSummaryAuthorizationHandler : AuthorizationHandler<UpdatePublicationSummaryRequirement, Publication>
    {
        private readonly AuthorizationHandlerResourceRoleService _authorizationHandlerResourceRoleService;

        public UpdatePublicationSummaryAuthorizationHandler(
            AuthorizationHandlerResourceRoleService authorizationHandlerResourceRoleService)
        {
            _authorizationHandlerResourceRoleService = authorizationHandlerResourceRoleService;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
            UpdatePublicationSummaryRequirement summaryRequirement,
            Publication publication)
        {
            if (SecurityUtils.HasClaim(context.User, UpdateAllPublications))
            {
                context.Succeed(summaryRequirement);
                return;
            }

            if (await _authorizationHandlerResourceRoleService
                    .HasRolesOnPublication(
                        context.User.GetUserId(),
                        publication.Id,
                        Owner))
            {
                context.Succeed(summaryRequirement);
            }
        }
    }
}
