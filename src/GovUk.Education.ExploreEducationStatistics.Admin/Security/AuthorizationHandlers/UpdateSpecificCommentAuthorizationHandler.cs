#nullable enable
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Security.AuthorizationHandlers;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers
{
    public class UpdateSpecificCommentRequirement : IAuthorizationRequirement
    {
    }

    public class UpdateSpecificCommentAuthorizationHandler
        : AuthorizationHandler<UpdateSpecificCommentRequirement, Comment>
    {
        private readonly ContentDbContext _contentDbContext;
        private readonly IReleasePublishingStatusRepository _releasePublishingStatusRepository;
        private readonly AuthorizationHandlerResourceRoleService _authorizationHandlerResourceRoleService;

        public UpdateSpecificCommentAuthorizationHandler(ContentDbContext contentDbContext,
            IReleasePublishingStatusRepository releasePublishingStatusRepository,
            AuthorizationHandlerResourceRoleService authorizationHandlerResourceRoleService)
        {
            _contentDbContext = contentDbContext;
            _releasePublishingStatusRepository = releasePublishingStatusRepository;
            _authorizationHandlerResourceRoleService = authorizationHandlerResourceRoleService;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
            UpdateSpecificCommentRequirement requirement,
            Comment resource)
        {
            var release = GetRelease(_contentDbContext, resource);
            var updateSpecificReleaseContext = new AuthorizationHandlerContext(
                new[] {new UpdateSpecificReleaseRequirement()}, context.User, release);
            await new UpdateSpecificReleaseAuthorizationHandler(
                    _releasePublishingStatusRepository,
                    _authorizationHandlerResourceRoleService)
                .HandleAsync(updateSpecificReleaseContext);

            if (!updateSpecificReleaseContext.HasSucceeded)
            {
                return;
            }

            var canUpdateOwnCommentContext =
                new AuthorizationHandlerContext(new[] {requirement}, context.User, resource);
            
            await new CanUpdateOwnCommentAuthorizationHandler().HandleAsync(canUpdateOwnCommentContext);

            if (canUpdateOwnCommentContext.HasSucceeded)
            {
                context.Succeed(requirement);
            }
        }

        private static Release? GetRelease(ContentDbContext context, Comment comment)
        {
            var contentBlock = context.ContentBlocks
                .Include(block => block.ContentSection)
                .ThenInclude(contentSection => contentSection!.Release)
                .ThenInclude(section => section.Release)
                .First(block => block.Id == comment.ContentBlockId);

            return contentBlock.ContentSection?.Release.Release;
        }
    }

    public class CanUpdateOwnCommentAuthorizationHandler :
        EntityAuthorizationHandler<UpdateSpecificCommentRequirement, Comment>
    {
        public CanUpdateOwnCommentAuthorizationHandler() : base(
            context => context.User.GetUserId() == context.Entity.CreatedById)
        {
        }
    }
}
