using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces
{
    public interface IReleasePublishingStatusService
    {
        Task<ReleasePublishingKeyOld> Create(
            ReleasePublishingKeyOld releasePublishingKeyOld,
            ReleasePublishingStatusState state,
            bool immediate,
            IEnumerable<ReleasePublishingStatusLogMessage>? logMessages = null);

        Task<IReadOnlyList<ReleasePublishingKeyOld>> GetWherePublishingDueTodayWithStages(
            ReleasePublishingStatusContentStage? content = null,
            ReleasePublishingStatusFilesStage? files = null,
            ReleasePublishingStatusPublishingStage? publishing = null,
            ReleasePublishingStatusOverallStage? overall = null);

        Task<IReadOnlyList<ReleasePublishingKeyOld>> GetWherePublishingDueTodayOrInFutureWithStages(
            IReadOnlyList<Guid> releaseVersionIds,
            ReleasePublishingStatusContentStage? content = null,
            ReleasePublishingStatusFilesStage? files = null,
            ReleasePublishingStatusPublishingStage? publishing = null,
            ReleasePublishingStatusOverallStage? overall = null);

        Task<IReadOnlyList<ReleasePublishingStatusOld>> GetAllByOverallStage(
            Guid releaseVersionId,
            params ReleasePublishingStatusOverallStage[] overallStages);

        Task<ReleasePublishingStatusOld> GetOld(ReleasePublishingKeyOld releasePublishingKeyOld);

        Task<ReleasePublishingStatus?> GetLatest(Guid releaseVersionId);

        Task UpdateState(
            ReleasePublishingKeyOld releasePublishingKeyOld,
            ReleasePublishingStatusState state);

        Task UpdateContentStage(
            ReleasePublishingKeyOld releasePublishingKeyOld,
            ReleasePublishingStatusContentStage stage,
            ReleasePublishingStatusLogMessage? logMessage = null);

        Task UpdateFilesStage(
            ReleasePublishingKey releasePublishingKey,
            ReleasePublishingStatusFilesStage stage,
            ReleasePublishingStatusLogMessage? logMessage = null);

        Task UpdatePublishingStage(
            ReleasePublishingKey releasePublishingKey,
            ReleasePublishingStatusPublishingStage stage,
            ReleasePublishingStatusLogMessage? logMessage = null);
    }
}
