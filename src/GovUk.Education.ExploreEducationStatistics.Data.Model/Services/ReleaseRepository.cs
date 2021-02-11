#nullable enable
using System;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Services
{
    public class ReleaseRepository : AbstractRepository<Release, Guid>, IReleaseRepository
    {
        public ReleaseRepository(StatisticsDbContext context,
            ILogger<ReleaseRepository> logger) : base(context, logger)
        {
        }

        public Release? GetLatestPublishedRelease(Guid publicationId)
        {
            return DbSet()
                .Include(release => release.Publication)
                .Where(release => release.PublicationId.Equals(publicationId))
                .ToList()
                .Where(release => release.Live && IsLatestVersionOfRelease(release.Publication, release.Id))
                .OrderBy(release => release.Year)
                .ThenBy(release => release.TimeIdentifier)
                .LastOrDefault();
        }

        private static bool IsLatestVersionOfRelease(Publication publication, Guid releaseId)
        {
            return !publication.Releases.Any(r => r.PreviousVersionId == releaseId && r.Live && r.Id != releaseId);
        }
    }
}