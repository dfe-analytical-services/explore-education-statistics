using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using Microsoft.Azure.Cosmos.Table;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces
{
    public interface IReleaseInfoService
    {
        Task AddReleaseInfoAsync(ValidateReleaseMessage message, ReleaseInfoStatus status);
        Task<IEnumerable<ReleaseInfo>> ExecuteQueryAsync(TableQuery<ReleaseInfo> query);
        Task UpdateContentStatusAsync(Guid releaseId, Guid releaseInfoId, ReleaseInfoTaskStatus status);
        Task UpdateDataStatusAsync(Guid releaseId, Guid releaseInfoId, ReleaseInfoTaskStatus status);
        Task UpdateFilesStatusAsync(Guid releaseId, Guid releaseInfoId, ReleaseInfoTaskStatus status);
        Task UpdateReleaseInfoStatusAsync(Guid releaseId, Guid releaseInfoId, ReleaseInfoStatus status);
    }
}