using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using Microsoft.Azure.Cosmos.Table;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces
{
    public interface IReleaseStatusService
    {
        Task CreateOrUpdateAsync(Guid releaseId, ReleaseStatusState state,
            IEnumerable<ReleaseStatusLogMessage> logMessages = null);

        Task<IEnumerable<ReleaseStatus>> ExecuteQueryAsync(TableQuery<ReleaseStatus> query);

        Task UpdateStateAsync(Guid releaseId, Guid releaseStatusId, ReleaseStatusState state);

        Task UpdateContentStageAsync(Guid releaseId, Guid releaseStatusId, Stage stage);

        Task UpdateDataStageAsync(Guid releaseId, Guid releaseStatusId, Stage stage);

        Task UpdateFilesStageAsync(Guid releaseId, Guid releaseStatusId, Stage stage);

        Task UpdatePublishingStageAsync(Guid releaseId, Guid releaseStatusId, Stage stage);
    }
}