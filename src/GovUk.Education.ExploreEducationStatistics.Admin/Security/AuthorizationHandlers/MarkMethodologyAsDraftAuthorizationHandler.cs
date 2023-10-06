#nullable enable
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using Microsoft.AspNetCore.Authorization;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityClaimTypes;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers.AuthorizationHandlerResourceRoleService;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.MethodologyApprovalStatus;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;

// ReSharper disable once ClassNeverInstantiated.Global
public class MarkMethodologyAsDraftRequirement : IAuthorizationRequirement
{
}

public class MarkMethodologyAsDraftAuthorizationHandler : AuthorizationHandler<
    MarkMethodologyAsDraftRequirement, MethodologyVersion>
{
    private readonly IMethodologyVersionRepository _methodologyVersionRepository;
    private readonly IMethodologyRepository _methodologyRepository;
    private readonly AuthorizationHandlerResourceRoleService _authorizationHandlerResourceRoleService;

    public MarkMethodologyAsDraftAuthorizationHandler(
        IMethodologyVersionRepository methodologyVersionRepository,
        IMethodologyRepository methodologyRepository,
        AuthorizationHandlerResourceRoleService authorizationHandlerResourceRoleService)
    {
        _methodologyVersionRepository = methodologyVersionRepository;
        _methodologyRepository = methodologyRepository;
        _authorizationHandlerResourceRoleService = authorizationHandlerResourceRoleService;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
        MarkMethodologyAsDraftRequirement requirement,
        MethodologyVersion methodologyVersion)
    {
        // If the Methodology is already public, it cannot be marked as draft
        if (await _methodologyVersionRepository.IsLatestPublishedVersion(methodologyVersion))
        {
            return;
        }

        if (SecurityUtils.HasClaim(context.User, MarkAllMethodologiesDraft))
        {
            context.Succeed(requirement);
            return;
        }

        var allowedPublicationRoles = methodologyVersion.Status == Approved
            ? ListOf(PublicationRole.Approver)
            : ListOf(PublicationRole.Owner, PublicationRole.Approver);

        var allowedReleaseRoles = methodologyVersion.Status == Approved
            ? ListOf(ReleaseRole.Approver)
            : ReleaseEditorAndApproverRoles;

        var owningPublication =
            await _methodologyRepository.GetOwningPublication(methodologyVersion.MethodologyId);

        if (await _authorizationHandlerResourceRoleService
                .HasRolesOnPublicationOrAnyRelease(
                    context.User.GetUserId(),
                    owningPublication.Id,
                    allowedPublicationRoles,
                    allowedReleaseRoles))
        {
            context.Succeed(requirement);
        }
    }
}
