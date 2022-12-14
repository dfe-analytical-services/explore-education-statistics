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
        private readonly IMethodologyVersionRepository _methodologyVersionRepository;
        private readonly IMethodologyRepository _methodologyRepository;
        private readonly AuthorizationHandlerResourceRoleService _authorizationHandlerResourceRoleService;

        public UpdateSpecificMethodologyAuthorizationHandler(
            IMethodologyVersionRepository methodologyVersionRepository,
            IMethodologyRepository methodologyRepository,
            AuthorizationHandlerResourceRoleService authorizationHandlerResourceRoleService)
        {
            _methodologyVersionRepository = methodologyVersionRepository;
            _methodologyRepository = methodologyRepository;
            _authorizationHandlerResourceRoleService = authorizationHandlerResourceRoleService;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
            UpdateSpecificMethodologyRequirement requirement,
            MethodologyVersion methodologyVersion)
        {
            // An Approved Methodology cannot be updated.  Instead, it should firstly be unapproved if permissions
            // allow and then updated.
            if (methodologyVersion.Approved)
            {
                return;
            }

            // If the Methodology is already public, it cannot be updated.
            if (await _methodologyVersionRepository.IsPubliclyAccessible(methodologyVersion.Id))
            {
                return;
            }

            // If the user has a global Claim that allows them to update any Methodology, allow it.
            if (SecurityUtils.HasClaim(context.User, UpdateAllMethodologies))
            {
                context.Succeed(requirement);
                return;
            }

            var owningPublication =
                await _methodologyRepository.GetOwningPublication(methodologyVersion.MethodologyId);
            
            // If the user is a Publication Owner or Approver of the Publication that owns this Methodology, they can
            // update it.
            // Additionally, if they're an Editor (Contributor, Lead) or an Approver of the latest (Live or non-Live)
            // Release of the owning Publication of this Methodology, they can update it.
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
