#nullable enable
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Security.AuthorizationHandlers
{
    public class ResolveSpecificCommentRequirement : IAuthorizationRequirement
    {
    }

    public class ResolveSpecificCommentAuthorizationHandler
        : AuthorizationHandler<ResolveSpecificCommentRequirement, Comment>
    {
        private readonly ContentDbContext _contentDbContext;
        private readonly IReleaseStatusRepository _releaseStatusRepository;
        private readonly IUserPublicationRoleRepository _userPublicationRoleRepository;
        private readonly IUserReleaseRoleRepository _userReleaseRoleRepository;

        public ResolveSpecificCommentAuthorizationHandler(ContentDbContext contentDbContext,
            IReleaseStatusRepository releaseStatusRepository,
            IUserPublicationRoleRepository userPublicationRoleRepository,
            IUserReleaseRoleRepository userReleaseRoleRepository)
        {
            _contentDbContext = contentDbContext;
            _releaseStatusRepository = releaseStatusRepository;
            _userPublicationRoleRepository = userPublicationRoleRepository;
            _userReleaseRoleRepository = userReleaseRoleRepository;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context,
            ResolveSpecificCommentRequirement requirement,
            Comment resource)
        {
            var release = GetRelease(_contentDbContext, resource);
            var updateSpecificReleaseContext = new AuthorizationHandlerContext(
                new[] {new UpdateSpecificReleaseRequirement()}, context.User, release);
            await new UpdateSpecificReleaseAuthorizationHandler(_releaseStatusRepository,
                _userPublicationRoleRepository,
                _userReleaseRoleRepository)
                .HandleAsync(updateSpecificReleaseContext);

            if (updateSpecificReleaseContext.HasSucceeded)
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
}
