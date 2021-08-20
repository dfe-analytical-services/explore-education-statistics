#nullable enable
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Content.Security.AuthorizationHandlers
{
    public class ViewReleaseRequirement : IAuthorizationRequirement
    {
    }

    public class ViewReleaseAuthorizationHandler
        : AuthorizationHandler<ViewReleaseRequirement, Release>
    {
        private readonly ContentDbContext _context;

        public ViewReleaseAuthorizationHandler(ContentDbContext context)
        {
            _context = context;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext authContext,
            ViewReleaseRequirement requirement,
            Release release)
        {
            if (!_context.TryReloadEntity(release, out var loadedRelease))
            {
                return;
            }

            await _context.Entry(loadedRelease)
                .Reference(p => p.Publication)
                .Query()
                .Include(p => p.Releases)
                .LoadAsync();

            if (loadedRelease.IsLatestPublishedVersionOfRelease())
            {
                authContext.Succeed(requirement);
            }
        }
    }
}