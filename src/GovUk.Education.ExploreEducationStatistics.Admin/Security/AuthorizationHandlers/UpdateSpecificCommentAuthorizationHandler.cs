#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Security.AuthorizationHandlers;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;

public class UpdateSpecificCommentRequirement : IAuthorizationRequirement { }

public class UpdateSpecificCommentAuthorizationHandler(
    ContentDbContext contentDbContext,
    IAuthorizationHandlerService authorizationHandlerService
) : AuthorizationHandler<UpdateSpecificCommentRequirement, Comment>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        UpdateSpecificCommentRequirement requirement,
        Comment resource
    )
    {
        var releaseVersion = GetReleaseVersion(contentDbContext, resource);
        var updateSpecificReleaseVersionContext = new AuthorizationHandlerContext(
            requirements: [new UpdateSpecificReleaseVersionRequirement()],
            user: context.User,
            resource: releaseVersion
        );

        await new UpdateSpecificReleaseVersionAuthorizationHandler(authorizationHandlerService).HandleAsync(
            updateSpecificReleaseVersionContext
        );

        if (!updateSpecificReleaseVersionContext.HasSucceeded)
        {
            return;
        }

        var canUpdateOwnCommentContext = new AuthorizationHandlerContext([requirement], context.User, resource);

        await new CanUpdateOwnCommentAuthorizationHandler().HandleAsync(canUpdateOwnCommentContext);

        if (canUpdateOwnCommentContext.HasSucceeded)
        {
            context.Succeed(requirement);
        }
    }

    private static ReleaseVersion? GetReleaseVersion(ContentDbContext context, Comment comment)
    {
        var contentBlock = context
            .ContentBlocks.Include(cb => cb.ContentSection)
                .ThenInclude(cs => cs!.ReleaseVersion)
                    .ThenInclude(rv => rv.Release)
            .First(cb => cb.Id == comment.ContentBlockId);

        return contentBlock.ContentSection?.ReleaseVersion;
    }
}

public class CanUpdateOwnCommentAuthorizationHandler
    : EntityAuthorizationHandler<UpdateSpecificCommentRequirement, Comment>
{
    public CanUpdateOwnCommentAuthorizationHandler()
        : base(context => context.User.GetUserId() == context.Entity.CreatedById) { }
}
