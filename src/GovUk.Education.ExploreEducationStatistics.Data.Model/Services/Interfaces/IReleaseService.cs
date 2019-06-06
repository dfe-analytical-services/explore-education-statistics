using System;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces
{
    public interface IReleaseService : IRepository<Release, long>
    {
        Guid GetLatestRelease(Guid publicationId);
    }
}