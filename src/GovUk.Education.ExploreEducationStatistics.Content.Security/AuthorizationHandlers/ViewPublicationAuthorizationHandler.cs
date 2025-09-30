#nullable enable
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Authorization;

namespace GovUk.Education.ExploreEducationStatistics.Content.Security.AuthorizationHandlers;

public class ViewPublicationRequirement : IAuthorizationRequirement { }

public class ViewPublicationAuthorizationHandler
    : AuthorizationHandler<ViewPublicationRequirement, Publication>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext authContext,
        ViewPublicationRequirement requirement,
        Publication publication
    )
    {
        if (publication.LatestPublishedReleaseVersionId != null)
        {
            authContext.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
