using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using Microsoft.Azure.Cosmos.Table;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces
{
    public interface IReleaseStatusService
    {
        Task CreateOrUpdateAsync(Guid releaseId, (Stage Content, Stage Files, Stage Data, Stage Overall) stage);

        Task<IEnumerable<ReleaseStatus>> ExecuteQueryAsync(TableQuery<ReleaseStatus> query);

        Task UpdateStageAsync(Guid releaseId, Guid releaseStatusId,
            (Stage Content, Stage Files, Stage Data, Stage Overall) stage);

        Task UpdateContentStageAsync(Guid releaseId, Guid releaseStatusId, Stage stage);

        Task UpdateDataStageAsync(Guid releaseId, Guid releaseStatusId, Stage stage);

        Task UpdateFilesStageAsync(Guid releaseId, Guid releaseStatusId, Stage stage);
    }
}