#nullable enable
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using Microsoft.AspNetCore.Authorization;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers.AuthorizationHandlerService;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityClaimTypes;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;

public class UpdateSpecificMethodologyRequirement : IAuthorizationRequirement
{
}

public class UpdateSpecificMethodologyAuthorizationHandler :
    AuthorizationHandler<UpdateSpecificMethodologyRequirement, MethodologyVersion>
{
    private readonly IMethodologyRepository _methodologyRepository;
    private readonly AuthorizationHandlerService _authorizationHandlerService;

    public UpdateSpecificMethodologyAuthorizationHandler(
        IMethodologyRepository methodologyRepository,
        AuthorizationHandlerService authorizationHandlerService)
    {
        _methodologyRepository = methodologyRepository;
        _authorizationHandlerService = authorizationHandlerService;
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
            
        if (await _authorizationHandlerService
                .HasRolesOnPublicationOrAnyReleaseVersion(
                    context.User.GetUserId(),
                    owningPublication.Id,
                    ListOf(PublicationRole.Owner, PublicationRole.Approver),
                    ReleaseEditorAndApproverRoles))
        {
            context.Succeed(requirement);
        }
    }
}
