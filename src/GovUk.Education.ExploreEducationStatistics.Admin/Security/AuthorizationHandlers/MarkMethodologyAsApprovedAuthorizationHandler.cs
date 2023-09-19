#nullable enable
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using Microsoft.AspNetCore.Authorization;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityClaimTypes;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.PublicationRole;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;

// ReSharper disable once ClassNeverInstantiated.Global
public class MarkMethodologyAsApprovedRequirement : IAuthorizationRequirement
{
}

public class MarkMethodologyAsApprovedAuthorizationHandler :
    AuthorizationHandler<MarkMethodologyAsApprovedRequirement, MethodologyVersion>
{
    private readonly IMethodologyVersionRepository _methodologyVersionRepository;
    private readonly IMethodologyRepository _methodologyRepository;
    private readonly AuthorizationHandlerResourceRoleService _authorizationHandlerResourceRoleService;

    public MarkMethodologyAsApprovedAuthorizationHandler(
        IMethodologyVersionRepository methodologyVersionRepository,
        IMethodologyRepository methodologyRepository,
        AuthorizationHandlerResourceRoleService authorizationHandlerResourceRoleService)
    {
        _methodologyVersionRepository = methodologyVersionRepository;
        _methodologyRepository = methodologyRepository;
        _authorizationHandlerResourceRoleService = authorizationHandlerResourceRoleService;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
        MarkMethodologyAsApprovedRequirement requirement,
        MethodologyVersion methodologyVersion)
    {
        // If the Methodology is already public, it cannot be approved
        // An approved Methodology that isn't public can be approved to change attributes associated with approval
        if (await _methodologyVersionRepository.IsLatestPublishedVersion(methodologyVersion))
        {
            return;
        }

        if (SecurityUtils.HasClaim(context.User, ApproveAllMethodologies))
        {
            context.Succeed(requirement);
            return;
        }

        var owningPublication =
            await _methodologyRepository.GetOwningPublication(methodologyVersion.MethodologyId);

        // If the user is a Publication Approver that owns this Methodology, they can approve it.
        // Additionally, if they're an Approver for Releases on the owning Publication, they can approve it.
        if (await _authorizationHandlerResourceRoleService
                .HasRolesOnPublicationOrLatestRelease(
                    context.User.GetUserId(),
                    owningPublication.Id,
                    ListOf(Approver),
                    ListOf(ReleaseRole.Approver)))
        {
            context.Succeed(requirement);
        }
    }
}
