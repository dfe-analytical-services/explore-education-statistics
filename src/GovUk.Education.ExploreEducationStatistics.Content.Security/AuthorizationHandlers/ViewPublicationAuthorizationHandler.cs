#nullable enable
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.AspNetCore.Authorization;

namespace GovUk.Education.ExploreEducationStatistics.Content.Security.AuthorizationHandlers;

public class ViewPublicationRequirement : IAuthorizationRequirement
{
}

public class ViewPublicationAuthorizationHandler : AuthorizationHandler<ViewPublicationRequirement, Publication>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext authContext,
        ViewPublicationRequirement requirement,
        Publication publication)
    {
        if (publication.LatestPublishedReleaseId != null)
        {
            authContext.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
