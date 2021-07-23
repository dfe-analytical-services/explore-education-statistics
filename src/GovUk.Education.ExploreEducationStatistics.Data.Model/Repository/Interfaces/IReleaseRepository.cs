#nullable enable
using System;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces
{
    public interface IReleaseRepository : IRepository<Release, Guid>
    {
        Release? GetLatestPublishedRelease(Guid publicationId);
    }
}