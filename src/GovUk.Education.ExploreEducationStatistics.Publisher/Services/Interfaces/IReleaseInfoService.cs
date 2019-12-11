using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces
{
    public interface IReleaseInfoService
    {
        Task AddReleaseInfoAsync(QueueReleaseMessage message, ReleaseInfoStatus status);
        Task<IEnumerable<ReleaseInfo>> GetScheduledReleasesAsync();
        Task UpdateContentStatusAsync(Guid releaseId, Guid releaseInfoId, ReleaseInfoTaskStatus status);
        Task UpdateDataStatusAsync(Guid releaseId, Guid releaseInfoId, ReleaseInfoTaskStatus status);
        Task UpdateFilesStatusAsync(Guid releaseId, Guid releaseInfoId, ReleaseInfoTaskStatus status);
        Task UpdateReleaseInfoStatusAsync(Guid releaseId, Guid releaseInfoId, ReleaseInfoStatus status);
    }
}