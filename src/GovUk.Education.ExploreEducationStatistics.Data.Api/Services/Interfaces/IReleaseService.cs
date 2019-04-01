using System;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Services.Interfaces
{
    public interface IReleaseService
    {
        long GetLatestRelease(Guid publicationId);
    }
}