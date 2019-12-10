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
        Task UpdateReleaseInfoStage(Guid releaseId, Guid releaseInfoId);
        Task UpdateReleaseInfoStatusAsync(Guid releaseId, string rowKey, ReleaseInfoStatus status);
    }
}