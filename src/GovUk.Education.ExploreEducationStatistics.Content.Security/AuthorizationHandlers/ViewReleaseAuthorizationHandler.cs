using System.Threading.Tasks;
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
            var hydratedRelease = await _context.Releases
                .Include(r => r.Publication)
                .ThenInclude(p => p.Releases)
                .FirstOrDefaultAsync(r => r.Id == release.Id);

            if (hydratedRelease == null)
            {
                return;
            }

            if (hydratedRelease.IsLatestPublishedVersionOfRelease())
            {
                authContext.Succeed(requirement);
            }
        }
    }
}