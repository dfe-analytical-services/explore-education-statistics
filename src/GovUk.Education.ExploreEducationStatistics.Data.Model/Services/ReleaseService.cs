using System;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Services
{
    public class ReleaseService : AbstractDataService<Release, long>, IReleaseService
    {

        public ReleaseService(ApplicationDbContext context, ILogger<ReleaseService> logger) :
            base(context, logger)
        {
        }

        public long GetLatestRelease(Guid publicationId)
        {
            return TopWithPredicate(data => data.Id, data => data.PublicationId == publicationId);
        }
    }
}