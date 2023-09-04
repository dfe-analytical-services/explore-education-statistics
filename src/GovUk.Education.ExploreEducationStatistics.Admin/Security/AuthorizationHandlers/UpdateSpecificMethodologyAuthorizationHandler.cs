#nullable enable
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using Microsoft.AspNetCore.Authorization;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers.AuthorizationHandlerResourceRoleService;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityClaimTypes;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.PublicationRole;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers
{
    public class UpdateSpecificMethodologyRequirement : IAuthorizationRequirement
    {
    }

    public class UpdateSpecificMethodologyAuthorizationHandler :
        AuthorizationHandler<UpdateSpecificMethodologyRequirement, MethodologyVersion>
    {
        private readonly IMethodologyRepository _methodologyRepository;
        private readonly AuthorizationHandlerResourceRoleService _authorizationHandlerResourceRoleService;

        public UpdateSpecificMethodologyAuthorizationHandler(
            IMethodologyRepository methodologyRepository,
            AuthorizationHandlerResourceRoleService authorizationHandlerResourceRoleService)
        {
            _methodologyRepository = methodologyRepository;
            _authorizationHandlerResourceRoleService = authorizationHandlerResourceRoleService;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
            UpdateSpecificMethodologyRequirement requirement,
            MethodologyVersion methodologyVersion)
        {
            if (methodologyVersion.Approved)
            {
                return;
            }

            if (SecurityUtils.HasClaim(context.User, UpdateAllMethodologies))
            {
                context.Succeed(requirement);
                return;
            }

            var owningPublication =
                await _methodologyRepository.GetOwningPublication(methodologyVersion.MethodologyId);
            
            if (await _authorizationHandlerResourceRoleService
                    .HasRolesOnPublicationOrLatestRelease(
                        context.User.GetUserId(),
                        owningPublication.Id,
                        ListOf(Owner, Approver),
                        ReleaseEditorAndApproverRoles))
            {
                context.Succeed(requirement);
            }
        }
    }
}
