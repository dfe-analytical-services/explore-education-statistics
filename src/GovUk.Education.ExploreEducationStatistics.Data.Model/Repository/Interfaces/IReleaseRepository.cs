#nullable enable
using System;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces
{
    public interface IReleaseRepository
    {
        Release? GetLatestPublishedRelease(Guid publicationId);
    }
}
