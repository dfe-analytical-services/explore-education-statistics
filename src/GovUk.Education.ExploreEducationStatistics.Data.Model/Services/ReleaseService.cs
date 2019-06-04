using System;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Services
{
    public class ReleaseService : AbstractRepository<Release, long>, IReleaseService
    {
        public ReleaseService(ApplicationDbContext context,
            ILogger<ReleaseService> logger) : base(context, logger)
        {
        }

        public Guid GetLatestRelease(Guid publicationId)
        {
            return DbSet()
                .Where(release => release.PublicationId.Equals(publicationId))
                .OrderByDescending(release => release.ReleaseDate)
                .Select(release => release.Id)
                .FirstOrDefault();
        }
    }
}