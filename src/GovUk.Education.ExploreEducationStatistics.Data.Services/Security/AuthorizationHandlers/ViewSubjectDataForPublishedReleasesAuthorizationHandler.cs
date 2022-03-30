using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Security.AuthorizationHandlers
{
    public class ViewSubjectDataForPublishedReleasesAuthorizationHandler : AuthorizationHandler<
        ViewSubjectDataRequirement, Subject>
    {
        private readonly StatisticsDbContext _context;

        public ViewSubjectDataForPublishedReleasesAuthorizationHandler(StatisticsDbContext context)
        {
            _context = context;
        }

        protected override Task HandleRequirementAsync(AuthorizationHandlerContext authContext, 
            ViewSubjectDataRequirement requirement,
            Subject subject)
        {
            var attachedToPublishedRelease = _context
                .ReleaseSubject
                .Include(r => r.Release)
                .Where(r => r.SubjectId == subject.Id)
                .ToList()
                .Any(r => r.Release.Live);

            if (attachedToPublishedRelease)
            {
                authContext.Succeed(requirement);
            }

            return Task.CompletedTask;

        }
    }
}
