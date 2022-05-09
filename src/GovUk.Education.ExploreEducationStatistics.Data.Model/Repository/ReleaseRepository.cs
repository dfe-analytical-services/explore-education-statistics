#nullable enable
using System;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Repository
{
    public class ReleaseRepository : AbstractRepository<Release, Guid>, IReleaseRepository
    {
        public ReleaseRepository(StatisticsDbContext context) : base(context)
        {
        }

        public Release? GetLatestPublishedRelease(Guid publicationId)
        {
            // NOTE: This method won't get the latest release if it has no subject attached, as no
            // Statistics DB Release table entry will be created
            return DbSet()
                .Where(release => release.PublicationId == publicationId)
                .ToList()
                .Where(release => release.Live && IsLatestVersionOfRelease(release.PublicationId, release.Id))
                .OrderBy(release => release.Year)
                .ThenBy(release => release.TimeIdentifier)
                .LastOrDefault();
        }

        private bool IsLatestVersionOfRelease(Guid publicationId, Guid releaseId)
        {
            var releases = _context.Release
                .Where(r => r.PublicationId == publicationId)
                .ToList();
            return !releases.Any(r => r.PreviousVersionId == releaseId && r.Live && r.Id != releaseId);
        }
    }
}
