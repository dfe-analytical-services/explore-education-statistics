#nullable enable
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using Microsoft.AspNetCore.Authorization;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityClaimTypes;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.MethodologyStatus;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.PublicationRole;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers
{
    public class DeleteSpecificMethodologyRequirement : IAuthorizationRequirement
    {
    }

    public class DeleteSpecificMethodologyAuthorizationHandler
        : AuthorizationHandler<DeleteSpecificMethodologyRequirement, MethodologyVersion>
    {
        private readonly IMethodologyVersionRepository _methodologyVersionRepository;
        private readonly IMethodologyRepository _methodologyRepository;
        private readonly AuthorizationHandlerResourceRoleService _authorizationHandlerResourceRoleService;

        public DeleteSpecificMethodologyAuthorizationHandler(
            IMethodologyVersionRepository methodologyVersionRepository,
            IMethodologyRepository methodologyRepository,
            AuthorizationHandlerResourceRoleService authorizationHandlerResourceRoleService)
        {
            _methodologyVersionRepository = methodologyVersionRepository;
            _methodologyRepository = methodologyRepository;
            _authorizationHandlerResourceRoleService = authorizationHandlerResourceRoleService;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            DeleteSpecificMethodologyRequirement requirement,
            MethodologyVersion methodologyVersion)
        {
            // If the Methodology is already public, it cannot be deleted.
            if (await _methodologyVersionRepository.IsPubliclyAccessible(methodologyVersion.Id))
            {
                return;
            }

            if (methodologyVersion.Status == Approved)
            {
                return;
            }

            if (SecurityUtils.HasClaim(context.User, DeleteAllMethodologies))
            {
                context.Succeed(requirement);
                return;
            }

            var owningPublication =
                await _methodologyRepository.GetOwningPublication(methodologyVersion.MethodologyId);
            
            if (await _authorizationHandlerResourceRoleService
                    .HasRolesOnPublication(
                        context.User.GetUserId(),
                        owningPublication.Id,
                        Owner))
            {
                context.Succeed(requirement);
            }
        }
    }
}
