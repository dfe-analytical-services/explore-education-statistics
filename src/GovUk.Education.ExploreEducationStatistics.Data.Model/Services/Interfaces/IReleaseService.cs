using System;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Services.Interfaces
{
    public interface IReleaseService : IDataService<Release, long>
    {
        long GetLatestRelease(Guid publicationId);
    }
}