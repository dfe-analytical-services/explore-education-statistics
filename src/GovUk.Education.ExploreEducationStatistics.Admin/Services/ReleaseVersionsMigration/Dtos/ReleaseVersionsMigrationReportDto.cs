#nullable enable
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.ReleaseVersionsMigration.Dtos;

/// <summary>
/// TODO EES-6885 Remove after the Release Versions migration is complete.
/// </summary>
public record ReleaseVersionsMigrationReportDto
{
    public required bool DryRun { get; init; }

    public required List<Guid> ReleaseVersionsWithWarnings { get; init; }

    public required int TotalReleaseVersionsWithWarnings { get; init; }

    public required List<Guid> ReleaseVersionsNotToUpdate { get; init; }

    public required int TotalReleaseVersionsNotToUpdate { get; init; }

    public required List<Guid> ReleaseVersionsToUpdate { get; init; }

    public required int TotalReleaseVersionsToUpdate { get; init; }

    public required int TotalRelevantReleaseVersions { get; init; }

    public required List<ReleaseVersionsMigrationReportPublicationDto> Publications { get; init; }
}

public record ReleaseVersionsMigrationReportPublicationDto
{
    public required Guid PublicationId { get; init; }

    public required List<ReleaseVersionsMigrationReportReleaseDto> Releases { get; init; }

    public required string Slug { get; init; }

    public required string Title { get; init; }

    public required string Theme { get; init; }
}

public record ReleaseVersionsMigrationReportReleaseDto
{
    public required Guid ReleaseId { get; init; }

    public required List<ReleaseVersionsMigrationReportReleaseVersionDto> ReleaseVersions { get; init; }

    public required string Slug { get; init; }

    public required string Title { get; init; }
}

public record ReleaseVersionsMigrationReportReleaseVersionDto
{
    public required Guid ReleaseVersionId { get; init; }

    public required int Version { get; init; }

    public required DateTimeOffset? PublishedOriginal { get; init; }

    public required DateTimeOffset? PublishedProposed { get; init; }

    public required DateOnly? PublishedOriginalUkDateOnly { get; init; }

    public required DateOnly? PublishedProposedUkDateOnly { get; init; }

    public required ReleaseVersionsMigrationReportPublishingInfoDto PublishingInfo { get; init; }

    public required ReleaseVersionsMigrationReportUpdateNotesDto UpdateNotes { get; init; }

    public required ReleaseVersionsMigrationReportWarningsDto Warnings { get; init; }
}

public record ReleaseVersionsMigrationReportUpdateNotesDto
{
    /// <summary>
    /// The date-only element in UK local time of the latest update note associated with the release version.
    /// </summary>
    public required DateOnly? LatestUpdateNoteUkDateOnly { get; init; }

    public int UpdatesCount { get; init; }
}

public record ReleaseVersionsMigrationReportPublishingInfoDto
{
    [JsonConverter(typeof(StringEnumConverter))]
    public required ReleasePublishingMethod PublishMethod { get; init; }

    /// <summary>
    /// The last updated timestamp of the latest 'Complete' publishing attempt.
    /// </summary>
    public required DateTimeOffset? LatestAttemptTimestamp { get; init; }

    /// <summary>
    /// The time when publishing the release version was due to start in UTC. This is midnight UK local time on the scheduled date
    /// when <c>PublishMethod</c> is <c>Scheduled</c>, or the time when the release version was approved when it is <c>Immediate</c>.
    /// The source of this value is <see cref="ReleaseVersion.PublishScheduled"/>.
    /// </summary>
    public required DateTimeOffset? ScheduledPublishTrigger { get; init; }

    /// <summary>
    /// This is an alternative version of <c>ScheduledPublishTrigger</c> for cases when <c>PublishMethod</c> is <c>Scheduled</c>.
    /// Instead of midnight UK local time when the initial stages were due to trigger,
    /// this is the time in UTC at 09:30 UK local time when the final publishing stage was due to trigger.
    /// </summary>
    public required DateTimeOffset? ScheduledPublishFinalStageTrigger { get; init; }

    public required int SuccessfulPublishingAttempts { get; init; }

    /// <summary>
    /// The duration between the <c>ScheduledPublishFinalStageTrigger</c> and the last updated timestamp of the latest
    /// 'Complete' publishing attempt <c>LatestAttemptTimestamp</c>.
    /// This is included because if it is very long it could indicate an issue with using <c>LatestAttemptTimestamp</c> as
    /// the proposed published date. For example, this could be the case if a row has been updated in table storage by a
    /// maintenance task at a later time and the timestamp no longer reflects the actual publishing completion time.
    /// </summary>
    [JsonIgnore]
    public required TimeSpan? TimeSinceScheduledTriggerToCompletion { get; init; }

    /// <summary>
    /// Pretty version of <c>ScheduledPublishTriggerToCompletionTimeSpan</c>.
    /// </summary>
    public required string? TimeSinceScheduledTriggerToCompletionPretty { get; init; }
}

public record ReleaseVersionsMigrationReportWarningsDto
{
    /// <summary>
    /// True when there is no history of 'Complete' publishing attempts for the release version, otherwise null.
    /// This is an issue because the proposed published date is derived from the latest 'Complete' publishing attempt.
    /// </summary>
    public required bool? NoSuccessfulPublishingAttempts { get; init; }

    /// <summary>
    /// True when there are multiple 'Complete' publishing attempts for the release version, otherwise null.
    /// The proposed published date references the latest attempt so this should not be an issue.
    /// It's included in the report to indicate there was a re-publish at some point.
    /// </summary>
    public required bool? MultipleSuccessfulPublishingAttempts { get; init; }

    /// <summary>
    /// True when the number of updates associated with the release version does not match the version number, otherwise null.
    /// This is expected in a few cases because some releases were intentionally given update notes to their initial version.
    /// Multiple update notes may have also been added to a single version, and update notes may have been deleted.
    /// It's included in the report because it could be useful in understanding discrepancies where the
    /// <c>LatestUpdateNoteDateDoesNotMatchPublishedDate</c> warning is true.
    /// </summary>
    public required bool? UpdatesCountDoesNotMatchVersion { get; init; }

    /// <summary>
    /// True if the date-only element of the latest update note does not match the date-only element of the proposed published date, otherwise null.
    /// This is expected in some cases because there's been no warning given to analysts that the latest update note date
    /// should be set to match the expected published date on approval,
    /// or an explanation that the date has been used to show the public facing 'last updated' date of the release.
    /// It's included in the report because it indicates that public facing 'last updated' date will differ when EES-6833
    /// alters that field to use the actual published date.
    /// </summary>
    public required bool? ProposedPublishedDateDoesNotMatchLatestUpdateNote { get; init; }

    /// <summary>
    /// True if the proposed published date is not similar to the publishing trigger date.
    /// It's included in the report because it may indicate an issue with the proposed date that needs manually reviewing.
    /// It allows a tolerance for the duration taken to complete publishing and differs depending on the publishing method:
    /// Scheduled = 5 minutes (The final stage trigger is at 09:30 UK local time and would normally be expected to complete within a minute or so).
    /// Immediate = 6 hours (The difference between the trigger date and the published date would be the time taken to
    /// complete ALL stages of publishing including the long-running 'Data' stage for early releases before it was removed).
    /// </summary>
    public required bool? ProposedPublishedDateIsNotSimilarToScheduledTriggerDate { get; init; }

    [JsonIgnore]
    public bool HasWarnings =>
        NoSuccessfulPublishingAttempts == true
        || MultipleSuccessfulPublishingAttempts == true
        || UpdatesCountDoesNotMatchVersion == true
        || ProposedPublishedDateDoesNotMatchLatestUpdateNote == true
        || ProposedPublishedDateIsNotSimilarToScheduledTriggerDate == true;
}
