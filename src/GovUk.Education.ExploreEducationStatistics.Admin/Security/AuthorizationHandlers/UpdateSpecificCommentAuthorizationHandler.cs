#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Security.AuthorizationHandlers;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers;

public class UpdateSpecificCommentRequirement : IAuthorizationRequirement { }

public class UpdateSpecificCommentAuthorizationHandler
    : AuthorizationHandler<UpdateSpecificCommentRequirement, Comment>
{
    private readonly ContentDbContext _contentDbContext;
    private readonly AuthorizationHandlerService _authorizationHandlerService;

    public UpdateSpecificCommentAuthorizationHandler(
        ContentDbContext contentDbContext,
        AuthorizationHandlerService authorizationHandlerService
    )
    {
        _contentDbContext = contentDbContext;
        _authorizationHandlerService = authorizationHandlerService;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        UpdateSpecificCommentRequirement requirement,
        Comment resource
    )
    {
        var releaseVersion = GetReleaseVersion(_contentDbContext, resource);
        var updateSpecificReleaseVersionContext = new AuthorizationHandlerContext(
            requirements: [new UpdateSpecificReleaseVersionRequirement()],
            user: context.User,
            resource: releaseVersion
        );

        await new UpdateSpecificReleaseVersionAuthorizationHandler(
            _authorizationHandlerService
        ).HandleAsync(updateSpecificReleaseVersionContext);

        if (!updateSpecificReleaseVersionContext.HasSucceeded)
        {
            return;
        }

        var canUpdateOwnCommentContext = new AuthorizationHandlerContext(
            new[] { requirement },
            context.User,
            resource
        );

        await new CanUpdateOwnCommentAuthorizationHandler().HandleAsync(canUpdateOwnCommentContext);

        if (canUpdateOwnCommentContext.HasSucceeded)
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

public class CanUpdateOwnCommentAuthorizationHandler
    : EntityAuthorizationHandler<UpdateSpecificCommentRequirement, Comment>
{
    public CanUpdateOwnCommentAuthorizationHandler()
        : base(context => context.User.GetUserId() == context.Entity.CreatedById) { }
}
