using System;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Services
{
    public class ReleaseService : AbstractRepository<Release, Guid>, IReleaseService
    {
        public ReleaseService(StatisticsDbContext context,
            ILogger<ReleaseService> logger) : base(context, logger)
        {
        }

        public Guid? GetLatestPublishedRelease(Guid publicationId)
        {
            return DbSet()
                .Where(release => release.PublicationId.Equals(publicationId))
                .OrderBy(release => release.Year)
                .ThenBy(release => release.TimeIdentifier)
                .ToList()
                .LastOrDefault(release => release.Live)?.Id;
        }
    }
}