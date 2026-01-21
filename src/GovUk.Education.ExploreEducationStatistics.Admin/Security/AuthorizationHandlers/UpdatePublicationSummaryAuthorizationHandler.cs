#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Services.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Authorization;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityClaimTypes;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.PublicationRole;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;

public class UpdatePublicationSummaryRequirement : IAuthorizationRequirement { }

public class UpdatePublicationSummaryAuthorizationHandler
    : AuthorizationHandler<UpdatePublicationSummaryRequirement, Publication>
{
    private readonly AuthorizationHandlerService _authorizationHandlerService;

    public UpdatePublicationSummaryAuthorizationHandler(AuthorizationHandlerService authorizationHandlerService)
    {
        _authorizationHandlerService = authorizationHandlerService;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        UpdatePublicationSummaryRequirement summaryRequirement,
        Publication publication
    )
    {
        if (SecurityUtils.HasClaim(context.User, UpdateAllPublications))
        {
            context.Succeed(summaryRequirement);
            return;
        }

        if (
            await _authorizationHandlerService.UserHasAnyPublicationRoleOnPublication(
                context.User.GetUserId(),
                publication.Id,
                Owner
            )
        )
        {
            context.Succeed(summaryRequirement);
        }
    }
}
