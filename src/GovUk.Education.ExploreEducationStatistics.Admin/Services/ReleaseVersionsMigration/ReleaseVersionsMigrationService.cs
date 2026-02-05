#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.ReleaseVersionsMigration.Dtos;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.ReleaseVersionsMigration;

/// <summary>
/// TODO EES-6885 Remove after the Release Versions migration is complete.
/// </summary>
public class ReleaseVersionsMigrationService(
    ContentDbContext contentDbContext,
    IReleasePublishingStatusRepository releasePublishingStatusRepository,
    IUserService userService
) : IReleaseVersionsMigrationService
{
    private readonly ComparerUtils.NullSafePropertyComparer<Publication> _publicationIdComparer =
        ComparerUtils.CreateComparerByProperty<Publication>(p => p.Id);
    private readonly ComparerUtils.NullSafePropertyComparer<Release> _releaseIdComparer =
        ComparerUtils.CreateComparerByProperty<Release>(r => r.Id);

    public async Task<Either<ActionResult, ReleaseVersionsMigrationReportDto>> MigrateReleaseVersionsPublishedDate(
        bool dryRun = true,
        CancellationToken cancellationToken = default
    ) =>
        await userService
            .CheckIsBauUser()
            .OnSuccess(async _ =>
            {
                var releaseVersions = await GetRelevantReleaseVersions(cancellationToken);

                // Key the release versions by ID for lookup during updates later
                var releaseVersionsDict = releaseVersions.ToDictionary(rv => rv.Id);

                // For each release version, get the release publishing info, for all 'Complete' publishing attempts.
                var publishingInfoTasks = releaseVersions.Select(rv => GetReleaseVersionPublishingInfo(rv.Id));
                var releaseVersionPublishingInfos = await Task.WhenAll(publishingInfoTasks);

                // Key the publishing infos by release version ID for lookup during report mapping later
                var releaseVersionPublishingInfosDict = releaseVersionPublishingInfos.ToDictionary(i =>
                    i.ReleaseVersionId
                );

                // Group the release versions by release and then by publication to build a report structure.
                // This enriches each release version with a proposed new published date, and details of the publishing info, update notes, and warnings.
                var publications = releaseVersions
                    .GroupBy(rv => rv.Release.Publication, rv => rv, _publicationIdComparer)
                    .Select(MapPublication(releaseVersionPublishingInfosDict))
                    .OrderBy(p => p.Title)
                    .ToList();

                // Flatten the list of enriched release versions from every publication in the report.
                var reportReleaseVersions = publications
                    .SelectMany(p => p.Releases)
                    .SelectMany(r => r.ReleaseVersions)
                    .ToList();

                // Get any release versions that have warnings so they can be highlighted in the report summary.
                var releaseVersionsWithWarnings = reportReleaseVersions
                    .Where(rv => rv.Warnings.HasWarnings)
                    .Select(rv => rv.ReleaseVersionId)
                    .ToList();

                // Get any release versions that won't be updated because they have no proposed new published date,
                // so they can be highlighted in the report summary.
                var releaseVersionsNotToUpdate = reportReleaseVersions
                    .Where(rv => !rv.PublishedProposed.HasValue)
                    .Select(rv => rv.ReleaseVersionId)
                    .ToList();

                // Get all the release versions that will be updated
                var releaseVersionsToUpdate = reportReleaseVersions.Where(rv => rv.PublishedProposed.HasValue).ToList();

                // The count of reported release versions should match the count of relevant release versions initially fetched.
                if (releaseVersionsNotToUpdate.Count + releaseVersionsToUpdate.Count != releaseVersions.Count)
                {
                    throw new InvalidOperationException(
                        $"Mismatch in reported release versions counts. Not to update: {releaseVersionsNotToUpdate.Count}, To update: {releaseVersionsToUpdate.Count}, Total relevant: {releaseVersions.Count}."
                    );
                }

                var report = new ReleaseVersionsMigrationReportDto
                {
                    DryRun = dryRun,
                    ReleaseVersionsWithWarnings = releaseVersionsWithWarnings,
                    TotalReleaseVersionsWithWarnings = releaseVersionsWithWarnings.Count,
                    ReleaseVersionsNotToUpdate = releaseVersionsNotToUpdate,
                    TotalReleaseVersionsNotToUpdate = releaseVersionsNotToUpdate.Count,
                    ReleaseVersionsToUpdate = releaseVersionsToUpdate.Select(rv => rv.ReleaseVersionId).ToList(),
                    TotalReleaseVersionsToUpdate = releaseVersionsToUpdate.Count,
                    TotalRelevantReleaseVersions = releaseVersions.Count,
                    Publications = publications,
                };

                if (!dryRun)
                {
                    // Update each release version that has a proposed new published date.
                    foreach (var rv in releaseVersionsToUpdate)
                    {
                        var releaseVersionEntity = releaseVersionsDict[rv.ReleaseVersionId];
                        releaseVersionEntity.Published = rv.PublishedProposed!.Value;
                    }

                    await contentDbContext.SaveChangesAsync(cancellationToken);
                }

                return report;
            });

    private async Task<List<ReleaseVersion>> GetRelevantReleaseVersions(CancellationToken cancellationToken) =>
        // There's no need to explicitly include soft-deleted release versions as they will have never been published.
        await contentDbContext
            .ReleaseVersions.Include(rv => rv.Release.Publication.Theme)
            // Include associated release updates to allow reporting on the most recent update note date.
            .Include(rv => rv.Updates)
            // Ignore release versions that have not been published yet which have no value to update.
            .Where(rv => rv.Published.HasValue)
            // Ignore first versions (where `Version` is 0) as these already have `Published` set as the actual published date.
            .Where(rv => rv.Version > 0)
            // Ignore versions where `UpdatePublishedDisplayDate` (formerly named `UpdateDisplayDate`) is true as these already have `Published` set as the actual published date.
            .Where(rv => rv.UpdatePublishedDisplayDate == false)
            .ToListAsync(cancellationToken: cancellationToken);

    private Func<IGrouping<Publication, ReleaseVersion>, ReleaseVersionsMigrationReportPublicationDto> MapPublication(
        IReadOnlyDictionary<Guid, ReleaseVersionPublishingInfo> releaseVersionPublishingInfos
    ) =>
        publicationGroup =>
        {
            var publication = publicationGroup.Key;
            var releases = publicationGroup
                .GroupBy(rv => rv.Release, rv => rv, _releaseIdComparer)
                .Select(MapRelease(releaseVersionPublishingInfos))
                .OrderBy(r => r.Title)
                .ToList();

            return new ReleaseVersionsMigrationReportPublicationDto
            {
                PublicationId = publication.Id,
                Releases = releases,
                Slug = publication.Slug,
                Theme = publication.Theme.Title,
                Title = publication.Title,
            };
        };

    private static Func<IGrouping<Release, ReleaseVersion>, ReleaseVersionsMigrationReportReleaseDto> MapRelease(
        IReadOnlyDictionary<Guid, ReleaseVersionPublishingInfo> releaseVersionPublishingInfos
    ) =>
        releaseGroup =>
        {
            var release = releaseGroup.Key;
            var releaseVersions = releaseGroup
                .Select(MapReleaseVersion(releaseVersionPublishingInfos))
                .OrderBy(rv => rv.Version)
                .ToList();

            return new ReleaseVersionsMigrationReportReleaseDto
            {
                ReleaseId = release.Id,
                ReleaseVersions = releaseVersions,
                Slug = release.Slug,
                Title = release.Title,
            };
        };

    private static Func<ReleaseVersion, ReleaseVersionsMigrationReportReleaseVersionDto> MapReleaseVersion(
        IReadOnlyDictionary<Guid, ReleaseVersionPublishingInfo> releaseVersionPublishingInfos
    ) =>
        releaseVersion =>
        {
            var publishingInfo = MapPublishingInfo(releaseVersion, releaseVersionPublishingInfos[releaseVersion.Id]);
            var updateNotes = MapUpdateNotes(releaseVersion);
            var warnings = GetWarnings(releaseVersion, updateNotes, publishingInfo);

            return new ReleaseVersionsMigrationReportReleaseVersionDto
            {
                ReleaseVersionId = releaseVersion.Id,
                Version = releaseVersion.Version,
                PublishingInfo = publishingInfo,
                PublishedOriginal = releaseVersion.Published,
                // The proposed published date is the last updated timestamp of the latest successful publishing attempt
                // which reached overall stage 'Complete'. If this doesn't exist, the release version won't be updated.
                PublishedProposed = publishingInfo.LatestAttemptTimestamp,
                PublishedOriginalUkDateOnly = releaseVersion.Published?.ToUkDateOnly(),
                PublishedProposedUkDateOnly = publishingInfo.LatestAttemptTimestampUkDateOnly,
                UpdateNotes = updateNotes,
                Warnings = warnings,
            };
        };

    private static ReleaseVersionsMigrationReportPublishingInfoDto MapPublishingInfo(
        ReleaseVersion releaseVersion,
        ReleaseVersionPublishingInfo publishingInfo
    )
    {
        // Determine the final stage trigger date.
        // For method 'Scheduled', ReleaseVersion.PublishScheduled represents the
        // first stage trigger at midnight UK local time and needs adjusting to the final stage at 09:30 UK local time.
        // If the value isn't at midnight UK local time, leave it unchanged. Rather than throwing an exception,
        // the warning 'ScheduledTriggerDateIsNotMidnightUkLocalTime' will be added to the report, allowing a 'dry run'
        // to proceed.
        // For method 'Immediate', ReleaseVersion.PublishScheduled needs no adjustment because all stages trigger together.
        var scheduledPublishFinalStageTrigger =
            publishingInfo.LatestAttemptMethod == ReleasePublishingMethod.Scheduled
                ? releaseVersion.PublishScheduled.AdjustUkLocalMidnightTo0930()
                : releaseVersion.PublishScheduled;

        // See the comment in ReleaseVersionsMigrationReportPublishingInfoDto for the rationale behind including this calculation.
        TimeSpan? timeSinceScheduledPublishFinalStageTriggerToCompletion =
            publishingInfo.LatestAttemptTimestamp.HasValue && scheduledPublishFinalStageTrigger.HasValue
                ? publishingInfo.LatestAttemptTimestamp.Value - scheduledPublishFinalStageTrigger.Value
                : null;
        var timeSinceScheduledPublishFinalStageTriggerToCompletionPretty =
            timeSinceScheduledPublishFinalStageTriggerToCompletion?.PrettyPrint();

        return new ReleaseVersionsMigrationReportPublishingInfoDto
        {
            PublishMethod = publishingInfo.LatestAttemptMethod,
            LatestAttemptTimestamp = publishingInfo.LatestAttemptTimestamp,
            LatestAttemptTimestampUkDateOnly = publishingInfo.LatestAttemptTimestamp?.ToUkDateOnly(),
            ScheduledPublishTrigger = releaseVersion.PublishScheduled,
            ScheduledPublishFinalStageTrigger = scheduledPublishFinalStageTrigger,
            SuccessfulPublishingAttempts = publishingInfo.SuccessfulPublishingAttempts,
            TimeSinceScheduledTriggerToCompletion = timeSinceScheduledPublishFinalStageTriggerToCompletion,
            TimeSinceScheduledTriggerToCompletionPretty = timeSinceScheduledPublishFinalStageTriggerToCompletionPretty,
        };
    }

    private static ReleaseVersionsMigrationReportUpdateNotesDto MapUpdateNotes(ReleaseVersion releaseVersion)
    {
        var latestUpdateNoteUkDateOnly = GetLatestUpdateNoteUkDateOnly(releaseVersion);
        var updatesCount = releaseVersion.Updates.Count;

        return new ReleaseVersionsMigrationReportUpdateNotesDto
        {
            LatestUpdateNoteUkDateOnly = latestUpdateNoteUkDateOnly,
            UpdatesCount = updatesCount,
        };
    }

    private static ReleaseVersionsMigrationReportWarningsDto GetWarnings(
        ReleaseVersion releaseVersion,
        ReleaseVersionsMigrationReportUpdateNotesDto updateNotes,
        ReleaseVersionsMigrationReportPublishingInfoDto publishingInfo
    )
    {
        // See the comments in ReleaseVersionsMigrationReportWarningsDto for the rationale behind these warnings.

        var noSuccessfulPublishingAttempts = publishingInfo.SuccessfulPublishingAttempts == 0;
        var multipleSuccessfulPublishingAttempts = publishingInfo.SuccessfulPublishingAttempts > 1;
        var updatesCountDoesNotMatchVersion = releaseVersion.Updates.Count != releaseVersion.Version;

        var proposedPublishedDateDoesNotMatchLatestUpdateNoteDate =
            publishingInfo.LatestAttemptTimestampUkDateOnly.HasValue
            && updateNotes.LatestUpdateNoteUkDateOnly.HasValue
            && publishingInfo.LatestAttemptTimestampUkDateOnly.Value != updateNotes.LatestUpdateNoteUkDateOnly.Value;

        var publishingTolerance =
            publishingInfo.PublishMethod == ReleasePublishingMethod.Immediate
                ? TimeSpan.FromHours(6)
                : TimeSpan.FromMinutes(5);
        var proposedPublishedDateIsNotSimilarToScheduledTriggerDate =
            publishingInfo.TimeSinceScheduledTriggerToCompletion > publishingTolerance;

        var scheduledTriggerDateIsNotMidnightUkLocalTime =
            publishingInfo.PublishMethod == ReleasePublishingMethod.Scheduled
            // Note, publishingInfo.ScheduledPublishTrigger is interchangeable with releaseVersion.PublishScheduled here
            && !publishingInfo.ScheduledPublishTrigger.IsUkLocalMidnight();

        // Use nullable flags with null values to represent absent warnings.
        // The JSON serializer is configured to ignore nulls and omitting these fields produces a more compact report.
        return new ReleaseVersionsMigrationReportWarningsDto
        {
            NoSuccessfulPublishingAttempts = noSuccessfulPublishingAttempts ? true : null,
            MultipleSuccessfulPublishingAttempts = multipleSuccessfulPublishingAttempts ? true : null,
            UpdatesCountDoesNotMatchVersion = updatesCountDoesNotMatchVersion ? true : null,
            ProposedPublishedDateDoesNotMatchLatestUpdateNoteDate =
                proposedPublishedDateDoesNotMatchLatestUpdateNoteDate ? true : null,
            ProposedPublishedDateIsNotSimilarToScheduledTriggerDate =
                proposedPublishedDateIsNotSimilarToScheduledTriggerDate ? true : null,
            ScheduledTriggerDateIsNotMidnightUkLocalTime = scheduledTriggerDateIsNotMidnightUkLocalTime ? true : null,
        };
    }

    private async Task<ReleaseVersionPublishingInfo> GetReleaseVersionPublishingInfo(Guid releaseVersionId)
    {
        // Get all the history of the Publisher function app's publishing attempts for the release version
        // where those attempts reached overall stage 'Complete'.
        var successfulAttemptHistory = await releasePublishingStatusRepository.GetAllByOverallStage(
            releaseVersionId,
            ReleasePublishingStatusOverallStage.Complete
        );

        var latestSuccessfulAttempt = successfulAttemptHistory.OrderByDescending(rps => rps.Created).FirstOrDefault();

        var latestAttemptMethod =
            latestSuccessfulAttempt == null ? ReleasePublishingMethod.Unknown
            : latestSuccessfulAttempt.Immediate ? ReleasePublishingMethod.Immediate
            : ReleasePublishingMethod.Scheduled;

        return new ReleaseVersionPublishingInfo
        {
            ReleaseVersionId = releaseVersionId,
            SuccessfulPublishingAttempts = successfulAttemptHistory.Count,
            LatestAttemptTimestamp = latestSuccessfulAttempt?.Timestamp,
            LatestAttemptMethod = latestAttemptMethod,
        };
    }

    private static DateOnly? GetLatestUpdateNoteUkDateOnly(ReleaseVersion releaseVersion)
    {
        var latestUpdateNote = releaseVersion.Updates.MaxBy(u => u.On);

        if (latestUpdateNote == null)
        {
            return null;
        }

        var lastUpdateNoteDateTimeOffset = new DateTimeOffset(
            latestUpdateNote.On,
            TimeZoneInfo.Local.GetUtcOffset(latestUpdateNote.On)
        );

        return lastUpdateNoteDateTimeOffset.ToUkDateOnly();
    }

    /// <summary>
    /// Details of the Publisher function app's publishing info for a release version.
    /// </summary>
    private record ReleaseVersionPublishingInfo
    {
        public required Guid ReleaseVersionId { get; init; }

        public int SuccessfulPublishingAttempts { get; init; }

        public required DateTimeOffset? LatestAttemptTimestamp { get; init; }

        public required ReleasePublishingMethod LatestAttemptMethod { get; init; }
    }
}
