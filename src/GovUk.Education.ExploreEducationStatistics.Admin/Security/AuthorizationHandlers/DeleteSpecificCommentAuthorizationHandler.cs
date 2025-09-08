#nullable enable
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;

public class DeleteSpecificCommentRequirement : IAuthorizationRequirement { }

public class DeleteSpecificCommentAuthorizationHandler : AuthorizationHandler<DeleteSpecificCommentRequirement, Comment>
{
    private readonly ContentDbContext _contentDbContext;
    private readonly AuthorizationHandlerService _authorizationHandlerService;

    public DeleteSpecificCommentAuthorizationHandler(
        ContentDbContext contentDbContext,
        AuthorizationHandlerService authorizationHandlerService
    )
    {
        _contentDbContext = contentDbContext;
        _authorizationHandlerService = authorizationHandlerService;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        DeleteSpecificCommentRequirement requirement,
        Comment resource
    )
    {
        var releaseVersion = GetReleaseVersion(_contentDbContext, resource);
        var updateSpecificReleaseVersionContext = new AuthorizationHandlerContext(
            requirements: [new UpdateSpecificReleaseVersionRequirement()],
            user: context.User,
            resource: releaseVersion
        );
        await new UpdateSpecificReleaseVersionAuthorizationHandler(_authorizationHandlerService).HandleAsync(
            updateSpecificReleaseVersionContext
        );

        if (updateSpecificReleaseVersionContext.HasSucceeded)
        {
            context.Succeed(requirement);
        }
    }

    private static ReleaseVersion? GetReleaseVersion(ContentDbContext context, Comment comment)
    {
        var contentBlock = context
            .ContentBlocks.Include(block => block.ContentSection)
            .ThenInclude(contentSection => contentSection!.ReleaseVersion)
            .First(block => block.Id == comment.ContentBlockId);

        return contentBlock.ContentSection?.ReleaseVersion;
    }
}
