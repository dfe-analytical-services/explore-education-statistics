#nullable enable
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;

public class ResolveSpecificCommentRequirement : IAuthorizationRequirement
{
}

public class ResolveSpecificCommentAuthorizationHandler
    : AuthorizationHandler<ResolveSpecificCommentRequirement, Comment>
{
    private readonly ContentDbContext _contentDbContext;
    private readonly AuthorizationHandlerService _authorizationHandlerService;

    public ResolveSpecificCommentAuthorizationHandler(ContentDbContext contentDbContext,
        AuthorizationHandlerService authorizationHandlerService)
    {
        _contentDbContext = contentDbContext;
        _authorizationHandlerService = authorizationHandlerService;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
        ResolveSpecificCommentRequirement requirement,
        Comment resource)
    {
        var releaseVersion = GetReleaseVersion(_contentDbContext, resource);
        var updateSpecificReleaseContext = new AuthorizationHandlerContext(
            new[] { new UpdateSpecificReleaseRequirement() }, context.User, releaseVersion);
        await new UpdateSpecificReleaseAuthorizationHandler(
                _authorizationHandlerService)
            .HandleAsync(updateSpecificReleaseContext);

        if (updateSpecificReleaseContext.HasSucceeded)
        {
            context.Succeed(requirement);
        }
    }

    private static ReleaseVersion? GetReleaseVersion(ContentDbContext context, Comment comment)
    {
        var contentBlock = context
            .ContentBlocks
            .Include(block => block.ContentSection)
            .ThenInclude(contentSection => contentSection!.ReleaseVersion)
            .First(block => block.Id == comment.ContentBlockId);

        return contentBlock.ContentSection?.ReleaseVersion;
    }
}
