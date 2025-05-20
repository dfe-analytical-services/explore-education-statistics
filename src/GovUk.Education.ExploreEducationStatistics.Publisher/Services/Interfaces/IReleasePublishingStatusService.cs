using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces
{
    public interface IReleasePublishingStatusService
    {
        Task Create(
            ReleasePublishingKey releasePublishingKey,
            ReleasePublishingStatusState state,
            bool immediate,
            IEnumerable<ReleasePublishingStatusLogMessage>? logMessages = null);

        /// <summary>
        /// Retrieves a list of release publishing keys for release versions scheduled for publishing,
        /// relative to a specified date.
        /// </summary>
        /// <param name="comparison">The type of date comparison to perform.</param>
        /// <param name="referenceDate">The date used for the comparison.</param>
        /// <returns>
        /// A read-only list of <see cref="ReleasePublishingKey"/> objects representing the release versions
        /// scheduled for publishing relative to the specified date.
        /// </returns>
        Task<IReadOnlyList<ReleasePublishingKey>> GetScheduledReleasesForPublishingRelativeToDate(
            DateComparison comparison,
            DateTimeOffset referenceDate);

        /// <summary>
        /// Retrieves a list of release publishing keys for release versions that are ready to be published at their
        /// scheduled time.
        /// </summary>
        /// <returns>
        /// A read-only list of <see cref="ReleasePublishingKey"/> objects representing the release versions
        /// ready for scheduled publishing.
        /// </returns>
        Task<IReadOnlyList<ReleasePublishingKey>> GetScheduledReleasesReadyForPublishing();

        /// <summary>
        /// Retrieves a list of release publishing keys for a specific release version
        /// that match the provided overall publishing stages.
        /// </summary>
        /// <param name="releaseVersionId">The unique identifier of the release version.</param>
        /// <param name="overallStages">An array of <see cref="ReleasePublishingStatusOverallStage"/> values representing
        /// the overall publishing stages to match.</param>
        /// <returns>
        /// A read-only list of <see cref="ReleasePublishingKey"/> objects that match the specified criteria.
        /// </returns>
        Task<IReadOnlyList<ReleasePublishingKey>> GetReleasesWithOverallStages(
            Guid releaseVersionId,
            ReleasePublishingStatusOverallStage[] overallStages);

        Task<ReleasePublishingStatus> Get(ReleasePublishingKey releasePublishingKey);

        Task<ReleasePublishingStatus?> GetLatest(Guid releaseVersionId);

        Task UpdateState(
            ReleasePublishingKey releasePublishingKey,
            ReleasePublishingStatusState state);

        Task UpdateContentStage(
            ReleasePublishingKey releasePublishingKey,
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
