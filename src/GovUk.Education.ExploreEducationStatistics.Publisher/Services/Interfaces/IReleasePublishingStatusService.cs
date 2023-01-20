using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces
{
    public interface IReleasePublishingStatusService
    {
        Task<ReleasePublishingStatus> CreateAsync(
            Guid releaseId, 
            Guid releaseStatusId, 
            ReleasePublishingStatusState state, 
            bool immediate,
            IEnumerable<ReleasePublishingStatusLogMessage> logMessages = null);

        Task<IEnumerable<ReleasePublishingStatus>> GetWherePublishingDueTodayWithStages(
            ReleasePublishingStatusContentStage? content = null,
            ReleasePublishingStatusFilesStage? files = null,
            ReleasePublishingStatusPublishingStage? publishing = null,
            ReleasePublishingStatusOverallStage? overall = null);
        
        Task<IEnumerable<ReleasePublishingStatus>> GetWherePublishingDueTodayOrInFutureWithStages(
            ReleasePublishingStatusContentStage? content = null,
            ReleasePublishingStatusFilesStage? files = null,
            ReleasePublishingStatusPublishingStage? publishing = null,
            ReleasePublishingStatusOverallStage? overall = null);

        Task<IEnumerable<ReleasePublishingStatus>> GetAllByOverallStage(
            Guid releaseId, 
            params ReleasePublishingStatusOverallStage[] overallStages);

        Task<ReleasePublishingStatus> GetAsync(Guid releaseId, Guid releaseStatusId);

        Task<ReleasePublishingStatus> GetLatestAsync(Guid releaseId);

        Task UpdateStateAsync(Guid releaseId, Guid releaseStatusId, ReleasePublishingStatusState state);

        Task UpdateContentStageAsync(
            Guid releaseId, 
            Guid releaseStatusId, 
            ReleasePublishingStatusContentStage stage,
            ReleasePublishingStatusLogMessage logMessage = null);

        Task UpdateFilesStageAsync(
            Guid releaseId, 
            Guid releaseStatusId, 
            ReleasePublishingStatusFilesStage stage,
            ReleasePublishingStatusLogMessage logMessage = null);

        Task UpdatePublishingStageAsync(
            Guid releaseId, 
            Guid releaseStatusId, 
            ReleasePublishingStatusPublishingStage stage,
            ReleasePublishingStatusLogMessage logMessage = null);
    }
}
