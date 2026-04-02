#nullable enable
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;

public class ResolveSpecificCommentRequirement : IAuthorizationRequirement { }

public class ResolveSpecificCommentAuthorizationHandler(
    ContentDbContext contentDbContext,
    IAuthorizationHandlerService authorizationHandlerService
) : AuthorizationHandler<ResolveSpecificCommentRequirement, Comment>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ResolveSpecificCommentRequirement requirement,
        Comment resource
    )
    {
        var releaseVersion = await GetReleaseVersion(contentDbContext, resource);

        var updateSpecificReleaseVersionContext = new AuthorizationHandlerContext(
            requirements: [new UpdateSpecificReleaseVersionRequirement()],
            user: context.User,
            resource: releaseVersion
        );

        await new UpdateSpecificReleaseVersionAuthorizationHandler(authorizationHandlerService).HandleAsync(
            updateSpecificReleaseVersionContext
        );

        if (updateSpecificReleaseVersionContext.HasSucceeded)
        {
            context.Succeed(requirement);
        }
    }

    private static async Task<ReleaseVersion> GetReleaseVersion(ContentDbContext context, Comment comment)
    {
        var contentBlock = await context
            .ContentBlocks.Include(cb => cb.ContentSection)
                .ThenInclude(cs => cs!.ReleaseVersion)
                    .ThenInclude(rv => rv.Release)
            .SingleAsync(cb => cb.Id == comment.ContentBlockId);

        return contentBlock.ContentSection!.ReleaseVersion;
    }
}
