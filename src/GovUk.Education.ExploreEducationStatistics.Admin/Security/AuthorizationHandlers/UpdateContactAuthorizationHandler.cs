using GovUk.Education.ExploreEducationStatistics.Common.Services.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Authorization;
using static GovUk.Education.ExploreEducationStatistics.Admin.Security.SecurityClaimTypes;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.PublicationRole;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;

public class UpdateContactRequirement : IAuthorizationRequirement
{
}

public class UpdateContactAuthorizationHandler : AuthorizationHandler<UpdateContactRequirement, Publication>
{
    private readonly AuthorizationHandlerService _authorizationHandlerService;

    public UpdateContactAuthorizationHandler(
        AuthorizationHandlerService authorizationHandlerService)
    {
        _authorizationHandlerService = authorizationHandlerService;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
        UpdateContactRequirement contactRequirement,
        Publication publication)
    {
        if (SecurityUtils.HasClaim(context.User, UpdateAllPublications))
        {
            context.Succeed(contactRequirement);
            return;
        }

        if (await _authorizationHandlerService
                .HasRolesOnPublication(
                    context.User.GetUserId(),
                    publication.Id,
                    Owner))
        {
            context.Succeed(contactRequirement);
        }
    }
}
