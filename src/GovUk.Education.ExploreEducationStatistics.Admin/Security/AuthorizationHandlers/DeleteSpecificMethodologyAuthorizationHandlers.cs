#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Services.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using Microsoft.AspNetCore.Authorization;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityClaimTypes;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.MethodologyApprovalStatus;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.PublicationRole;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;

public class DeleteSpecificMethodologyRequirement : IAuthorizationRequirement { }

public class DeleteSpecificMethodologyAuthorizationHandler
    : AuthorizationHandler<DeleteSpecificMethodologyRequirement, MethodologyVersion>
{
    private readonly IMethodologyRepository _methodologyRepository;
    private readonly AuthorizationHandlerService _authorizationHandlerService;

    public DeleteSpecificMethodologyAuthorizationHandler(
        IMethodologyRepository methodologyRepository,
        AuthorizationHandlerService authorizationHandlerService
    )
    {
        _methodologyRepository = methodologyRepository;
        _authorizationHandlerService = authorizationHandlerService;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        DeleteSpecificMethodologyRequirement requirement,
        MethodologyVersion methodologyVersion
    )
    {
        if (methodologyVersion.Status == Approved)
        {
            return;
        }

        if (SecurityUtils.HasClaim(context.User, DeleteAllMethodologies))
        {
            context.Succeed(requirement);
            return;
        }

        var owningPublication = await _methodologyRepository.GetOwningPublication(methodologyVersion.MethodologyId);

        if (
            await _authorizationHandlerService.HasRolesOnPublication(
                context.User.GetUserId(),
                owningPublication.Id,
                Owner
            )
        )
        {
            context.Succeed(requirement);
        }
    }
}
