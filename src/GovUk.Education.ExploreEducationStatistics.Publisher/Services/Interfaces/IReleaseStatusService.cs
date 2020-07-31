﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces
{
    public interface IReleaseStatusService
    {
        Task<ReleaseStatus> CreateAsync(Guid releaseId, ReleaseStatusState state, bool immediate,
            IEnumerable<ReleaseStatusLogMessage> logMessages = null);

        Task<ReleaseStatus> GetAsync(Guid releaseId, Guid releaseStatusId);

        Task<IEnumerable<ReleaseStatus>> GetWherePublishingDueTodayWithStages(ReleaseStatusContentStage? content = null,
            ReleaseStatusDataStage? data = null,
            ReleaseStatusFilesStage? files = null,
            ReleaseStatusPublishingStage? publishing = null,
            ReleaseStatusOverallStage? overall = null);

        Task<IEnumerable<ReleaseStatus>> GetAllByOverallStage(Guid releaseId, params ReleaseStatusOverallStage[] overallStages);

        Task<ReleaseStatus> GetLatestAsync(Guid releaseId);

        Task<bool> IsImmediate(Guid releaseId, Guid releaseStatusId);

        Task UpdateStateAsync(Guid releaseId, Guid releaseStatusId, ReleaseStatusState state);

        Task UpdateStagesAsync(Guid releaseId, Guid releaseStatusId, ReleaseStatusContentStage? content = null,
            ReleaseStatusDataStage? data = null, ReleaseStatusFilesStage? files = null,
            ReleaseStatusPublishingStage? publishing = null, ReleaseStatusOverallStage? overall = null, 
            ReleaseStatusLogMessage logMessage = null);

        Task UpdateContentStageAsync(Guid releaseId, Guid releaseStatusId, ReleaseStatusContentStage stage,
            ReleaseStatusLogMessage logMessage = null);

        Task UpdateDataStageAsync(Guid releaseId, Guid releaseStatusId, ReleaseStatusDataStage stage,
            ReleaseStatusLogMessage logMessage = null);

        Task UpdateFilesStageAsync(Guid releaseId, Guid releaseStatusId, ReleaseStatusFilesStage stage,
            ReleaseStatusLogMessage logMessage = null);

        Task UpdatePublishingStageAsync(Guid releaseId, Guid releaseStatusId, ReleaseStatusPublishingStage stage,
            ReleaseStatusLogMessage logMessage = null);
    }
}