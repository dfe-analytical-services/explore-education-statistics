#nullable enable
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Security.AuthorizationHandlers
{
    public class ViewSubjectDataForPublishedReleasesAuthorizationHandler : AuthorizationHandler<
        ViewSubjectDataRequirement, ReleaseSubject>
    {
        private readonly ContentDbContext _context;

        public ViewSubjectDataForPublishedReleasesAuthorizationHandler(ContentDbContext context)
        {
            _context = context;
        }

        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext authContext,
            ViewSubjectDataRequirement requirement,
            ReleaseSubject releaseSubject)
        {
            var release = await _context.Releases
                .Include(r => r.Publication)
                .ThenInclude(p => p.Releases)
                .SingleAsync(r => r.Id == releaseSubject.ReleaseId);

            if (release.IsLatestPublishedVersionOfRelease())
            {
                authContext.Succeed(requirement);
            }
        }
    }
}
