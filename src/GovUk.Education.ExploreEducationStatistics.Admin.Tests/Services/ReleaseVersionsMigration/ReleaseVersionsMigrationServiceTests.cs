#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Options;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.ReleaseVersionsMigration;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.ReleaseVersionsMigration.Dtos;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.ReleaseVersionsMigration;

/// <summary>
/// TODO EES-6885 Remove after the Release Versions migration is complete.
/// </summary>
public abstract class ReleaseVersionsMigrationServiceTests
{
    private readonly DataFixture _dataFixture = new();

    /// <summary>
    /// A date which can be used as the default original ReleaseVersion.Published value for release versions
    /// </summary>
    private static readonly DateTimeOffset DefaultPublished = new(
        year: 2026,
        month: 1,
        day: 1,
        hour: 9,
        minute: 30,
        second: 0,
        offset: TimeSpan.Zero
    );

    /// <summary>
    /// A date which can be used as the default ReleaseVersion.PublishScheduled,
    /// which is midnight UK local time on the same day as DefaultPublished.
    /// </summary>
    private static readonly DateTimeOffset DefaultPublishedScheduled = DefaultPublished.GetUkStartOfDayUtc();

    /// <summary>
    /// A date which can be used as the default timestamp for the latest 'Complete' publishing attempt,
    /// which is just after 09:30 UK local time on the same day as DefaultPublishedScheduled.
    /// </summary>
    private static readonly DateTimeOffset DefaultLatestAttemptTimestamp = DefaultPublishedScheduled
        .GetUkStartOfDayUtc()
        .AddHours(9)
        .AddMinutes(30)
        .AddSeconds(45);

    /// <summary>
    /// A time interval which can be added to a date at midnight to use when setting release update note dates.
    /// The time element of release update notes should be insignificant so this can be any interval,
    /// as long as it's less than 24 hours.
    /// </summary>
    private static readonly TimeSpan UpdateNoteTime = new(hours: 7, minutes: 26, seconds: 35);

    /// <summary>
    /// A date which can be used as the default date for release update notes, which has the same date-only element
    /// as DefaultLatestAttemptTimestamp when converted to UK local time.
    /// </summary>
    private static readonly DateTime DefaultUpdateNoteDate = GetDateTimeForUpdateNoteOnSameUkDay(
        DefaultLatestAttemptTimestamp
    );

    public class MigrateReleaseVersionsPublishedDateCoreTests : ReleaseVersionsMigrationServiceTests
    {
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task WhenMultipleReleaseVersionsExist_ThenReportContainsExpectedDataAndDatabaseIsUpdatedAccordingly(
            bool dryRun
        )
        {
            // Arrange

            // Set up the data for four release versions consisting of:
            // - version 0 which is the first published version, so not relevant to the migration.
            // - versions 1, and 2 which are published amendments and the only versions relevant to the migration.
            // - version 3 is a published amendment with the UpdatePublishedDate flag set to true, so not relevant.
            // - version 4 which is a draft amendment, so not relevant.

            // Version 0, version 3, and version 4 are not relevant to the migration,
            // *so not all of their data needs setting up*

            var releaseVersion0Id = Guid.NewGuid();
            var releaseVersion1Id = Guid.NewGuid();
            var releaseVersion2Id = Guid.NewGuid();
            var releaseVersion3Id = Guid.NewGuid();
            var releaseVersion4Id = Guid.NewGuid();

            // Set up version 1 to be published on 1st June 2025 (which would be scheduled to trigger at 09:30 UK local time)
            // Set up version 2 to be published on 1st Sep 11:34:21 (which would be 12:34:21 UK local time)
            var version0PublishedScheduled = DefaultPublishedScheduled;
            var version1PublishedScheduled = new DateOnly(year: 2025, month: 6, day: 1).GetUkStartOfDayUtc();
            var version2PublishedScheduled = new DateTimeOffset(
                year: 2025,
                month: 9,
                day: 1,
                hour: 11,
                minute: 34,
                second: 21,
                offset: TimeSpan.Zero
            );
            var version3PublishedScheduled = new DateTimeOffset(
                year: 2025,
                month: 12,
                day: 15,
                hour: 10,
                minute: 22,
                second: 8,
                offset: TimeSpan.Zero
            );

            // Published date of version 0 which can be any value and after the migration should not change
            var version0Published = DefaultPublished;

            // Set up the original values of 'Published' for version 1 and 2, inherited from the previous versions
            var version1Published = version0Published;
            var version2Published = version1Published;

            // Version 3's 'Published' value does not inherit from the previous version because it has the
            // 'UpdatePublishedDate' flag set to true, so it can be any value, and after the migration should not change
            var version3Published = version3PublishedScheduled.AddSeconds(45);

            // Set up the publishing methods.
            // Version 1 is set to method 'Scheduled' and version 2 is set to method 'Immediate'
            const bool version1Immediate = false;
            const bool version2Immediate = true;

            // Set up the update note dates - Any time on scheduled date should be fine, as long as it's got the same date-only
            // element as the proposed published date (the publisher's latest successful attempt) when converted to UK local time.
            var version1UpdateNote0Date = GetDateTimeForUpdateNoteOnSameUkDay(version1PublishedScheduled);
            var version2UpdateNote0Date = version1UpdateNote0Date;
            var version2UpdateNote1Date = GetDateTimeForUpdateNoteOnSameUkDay(version2PublishedScheduled);

            // Set up the latest successful publishing attempt timestamps
            // Version 1's latest successful attempt is just after 09:30 UK local time
            // Version 2's latest successful attempt is just after its scheduled trigger time because it's method is 'Immediate'
            var version1LatestAttempt = version1PublishedScheduled
                .GetUkStartOfDayUtc()
                .AddHours(9)
                .AddMinutes(30)
                .AddSeconds(45);
            var version2LatestAttempt = version2PublishedScheduled.AddSeconds(45);

            Publication publication = _dataFixture
                .DefaultPublication()
                .WithTheme(_dataFixture.DefaultTheme())
                .WithReleases(_ =>
                    [
                        _dataFixture
                            .DefaultRelease()
                            .WithVersions(_ =>
                                [
                                    // Version 0 - not relevant
                                    _dataFixture
                                        .DefaultReleaseVersion()
                                        .WithId(releaseVersion0Id)
                                        .WithVersion(0)
                                        .WithPublished(version0Published)
                                        .WithPublishScheduled(version0PublishedScheduled),
                                    // Version 1 - relevant
                                    _dataFixture
                                        .DefaultReleaseVersion()
                                        .WithId(releaseVersion1Id)
                                        .WithVersion(1)
                                        .WithPublished(version1Published)
                                        .WithPublishScheduled(version1PublishedScheduled)
                                        .WithUpdates(
                                            _dataFixture
                                                .DefaultUpdate()
                                                .ForIndex(0, s => s.SetOn(version1UpdateNote0Date))
                                                .GenerateList(1)
                                        ),
                                    // Version 2 - relevant
                                    _dataFixture
                                        .DefaultReleaseVersion()
                                        .WithId(releaseVersion2Id)
                                        .WithVersion(2)
                                        .WithPublished(version2Published)
                                        .WithPublishScheduled(version2PublishedScheduled)
                                        .WithUpdates(
                                            _dataFixture
                                                .DefaultUpdate()
                                                .ForIndex(0, s => s.SetOn(version2UpdateNote0Date))
                                                .ForIndex(1, s => s.SetOn(version2UpdateNote1Date))
                                                .GenerateList(2)
                                        ),
                                    // Version 3 - not relevant
                                    _dataFixture
                                        .DefaultReleaseVersion()
                                        .WithId(releaseVersion3Id)
                                        .WithVersion(3)
                                        .WithPublished(version3Published)
                                        .WithPublishScheduled(version3PublishedScheduled)
                                        .WithUpdatePublishedDisplayDate(true),
                                    // Version 4 - not relevant
                                    _dataFixture.DefaultReleaseVersion().WithId(releaseVersion4Id).WithVersion(4),
                                ]
                            ),
                    ]
                );

            var releasePublishingStatusRepository = new Mock<IReleasePublishingStatusRepository>(MockBehavior.Strict);
            releasePublishingStatusRepository.SetupGetAllByOverallStageCompleteReturnsSingleResult(
                releaseVersion1Id,
                immediate: version1Immediate,
                timestamp: version1LatestAttempt
            );
            releasePublishingStatusRepository.SetupGetAllByOverallStageCompleteReturnsSingleResult(
                releaseVersion2Id,
                immediate: version2Immediate,
                timestamp: version2LatestAttempt
            );

            var contextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryContentDbContext(contextId))
            {
                context.Publications.Add(publication);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var sut = BuildService(
                    context,
                    releasePublishingStatusRepository: releasePublishingStatusRepository.Object
                );

                // Act
                var outcome = await sut.MigrateReleaseVersionsPublishedDate(dryRun: dryRun);

                // Assert - report
                var report = outcome.AssertRight();

                // The count of relevant versions should exclude version 0, version 3, and version 4
                Assert.Equal(2, report.TotalRelevantReleaseVersions);

                // All relevant versions should be updated
                Assert.Equal(2, report.TotalReleaseVersionsToUpdate);
                Assert.Equal(2, report.ReleaseVersionsToUpdate.Count);
                Assert.Contains(releaseVersion1Id, report.ReleaseVersionsToUpdate);
                Assert.Contains(releaseVersion2Id, report.ReleaseVersionsToUpdate);

                // There should be no relevant versions that aren't updated
                Assert.Equal(0, report.TotalReleaseVersionsNotToUpdate);
                Assert.Empty(report.ReleaseVersionsNotToUpdate);

                // There should be no warnings
                AssertReportHasNoReleaseVersionsWithWarnings(report);

                // Deep dive on the report data for version 1
                var releaseVersion1Report = GetReleaseVersionReportFromReport(report, releaseVersion1Id);
                Assert.Equal(version1LatestAttempt, releaseVersion1Report.PublishingInfo.LatestAttemptTimestamp);
                Assert.Equal(
                    version1LatestAttempt.ToUkDateOnly(),
                    releaseVersion1Report.PublishingInfo.LatestAttemptTimestampUkDateOnly
                );
                Assert.Equal(ReleasePublishingMethod.Scheduled, releaseVersion1Report.PublishingInfo.PublishMethod);
                Assert.Equal(version1PublishedScheduled, releaseVersion1Report.PublishingInfo.ScheduledPublishTrigger);
                Assert.Equal(
                    version1PublishedScheduled.AdjustUkLocalMidnightTo0930(),
                    releaseVersion1Report.PublishingInfo.ScheduledPublishFinalStageTrigger
                );
                Assert.Equal(1, releaseVersion1Report.PublishingInfo.SuccessfulPublishingAttempts);
                Assert.Equal(
                    version1LatestAttempt - version1PublishedScheduled.AdjustUkLocalMidnightTo0930(),
                    releaseVersion1Report.PublishingInfo.TimeSinceScheduledTriggerToCompletion
                );
                Assert.Equal(
                    (version1LatestAttempt - version1PublishedScheduled.AdjustUkLocalMidnightTo0930()).PrettyPrint(),
                    releaseVersion1Report.PublishingInfo.TimeSinceScheduledTriggerToCompletionPretty
                );
                Assert.Equal(1, releaseVersion1Report.UpdateNotes.UpdatesCount);
                Assert.Equal(
                    version1PublishedScheduled.ToUkDateOnly(),
                    releaseVersion1Report.UpdateNotes.LatestUpdateNoteUkDateOnly
                );
                Assert.False(releaseVersion1Report.Warnings.HasWarnings);

                // Deep dive on the report data for version 2
                var releaseVersion2Report = GetReleaseVersionReportFromReport(report, releaseVersion2Id);
                Assert.Equal(version2LatestAttempt, releaseVersion2Report.PublishingInfo.LatestAttemptTimestamp);
                Assert.Equal(
                    version2LatestAttempt.ToUkDateOnly(),
                    releaseVersion2Report.PublishingInfo.LatestAttemptTimestampUkDateOnly
                );
                Assert.Equal(ReleasePublishingMethod.Immediate, releaseVersion2Report.PublishingInfo.PublishMethod);
                Assert.Equal(version2PublishedScheduled, releaseVersion2Report.PublishingInfo.ScheduledPublishTrigger);
                Assert.Equal(
                    version2PublishedScheduled,
                    releaseVersion2Report.PublishingInfo.ScheduledPublishFinalStageTrigger
                );
                Assert.Equal(1, releaseVersion2Report.PublishingInfo.SuccessfulPublishingAttempts);
                Assert.Equal(
                    version2LatestAttempt - version2PublishedScheduled,
                    releaseVersion2Report.PublishingInfo.TimeSinceScheduledTriggerToCompletion
                );
                Assert.Equal(
                    (version2LatestAttempt - version2PublishedScheduled).PrettyPrint(),
                    releaseVersion2Report.PublishingInfo.TimeSinceScheduledTriggerToCompletionPretty
                );
                Assert.Equal(2, releaseVersion2Report.UpdateNotes.UpdatesCount);
                Assert.Equal(
                    version2PublishedScheduled.ToUkDateOnly(),
                    releaseVersion2Report.UpdateNotes.LatestUpdateNoteUkDateOnly
                );
                Assert.False(releaseVersion2Report.Warnings.HasWarnings);

                releasePublishingStatusRepository.VerifyAll();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                // Assert - database
                var actualReleaseVersions = await context.ReleaseVersions.AsNoTracking().ToListAsync();

                var actualReleaseVersion0 = actualReleaseVersions.Single(rv => rv.Id == releaseVersion0Id);
                var actualReleaseVersion1 = actualReleaseVersions.Single(rv => rv.Id == releaseVersion1Id);
                var actualReleaseVersion2 = actualReleaseVersions.Single(rv => rv.Id == releaseVersion2Id);
                var actualReleaseVersion3 = actualReleaseVersions.Single(rv => rv.Id == releaseVersion3Id);
                var actualReleaseVersion4 = actualReleaseVersions.Single(rv => rv.Id == releaseVersion4Id);

                // Version 0 should always be unchanged because it was not relevant, being the first version
                Assert.Equal(version0Published, actualReleaseVersion0.Published);

                // The value of 'Published' for version 1 and version 2 should depend on whether it was a dry run.
                if (dryRun)
                {
                    // Version 1 and version 2 should both be unchanged because it was a dry run
                    Assert.Equal(version1Published, actualReleaseVersion1.Published);
                    Assert.Equal(version2Published, actualReleaseVersion2.Published);
                }
                else
                {
                    // Version 1 and version 2 should both be updated to have the same 'Published' value as their latest successful publishing attempt
                    Assert.Equal(version1LatestAttempt, actualReleaseVersion1.Published);
                    Assert.Equal(version2LatestAttempt, actualReleaseVersion2.Published);
                }

                // Version 3 should always be unchanged because it was not relevant, having flag 'UpdatePublishedDate' set to true
                Assert.Equal(version3Published, actualReleaseVersion3.Published);

                // Version 4 should always be unchanged because it was not relevant, being a draft version
                Assert.Null(actualReleaseVersion4.Published);
            }
        }

        [Fact]
        public async Task WhenReleaseVersionsExistWithoutPublishingAttempts_ThenReportContainsExpectedDataAndDatabaseIsNotUpdated()
        {
            // Arrange

            // Set up the data for three release versions consisting of:
            // - version 0 which is the first published version, so not relevant to the migration.
            // - versions 1 which is a published amendment but has no history of publishing attempts.
            // - versions 2 which is a published amendment and the only version that should be updated by the migration.

            // Version 0 is not relevant to the migration, *so not all of its data needs setting up*

            var releaseVersion0Id = Guid.NewGuid();
            var releaseVersion1Id = Guid.NewGuid();
            var releaseVersion2Id = Guid.NewGuid();

            // Set up version 1 to be published on 1st June 2025 (which would be scheduled to trigger at 09:30 UK local time)
            // Set up version 2 to be published on 1st Sep 11:34:21 (which would be 12:34:21 UK local time)
            var version0PublishedScheduled = DefaultPublishedScheduled;
            var version1PublishedScheduled = new DateOnly(year: 2025, month: 6, day: 1).GetUkStartOfDayUtc();
            var version2PublishedScheduled = new DateTimeOffset(
                year: 2025,
                month: 9,
                day: 1,
                hour: 11,
                minute: 34,
                second: 21,
                offset: TimeSpan.Zero
            );

            // Published date of version 0 which can be any value and after the migration should not change
            var version0Published = DefaultPublished;

            // Set up the original values of 'Published' for version 1 and 2, inherited from the previous versions
            var version1Published = version0Published;
            var version2Published = version1Published;

            // Set up the publishing methods.
            // This is irrelevant for version 1 without a history of publishing attempts for it.
            // Version 2 is set to method 'Immediate'
            const bool version2Immediate = true;

            // Set up the update note dates - Any time on scheduled date should be fine, as long as it's got the same date-only
            // element as the proposed published date (the publisher's latest successful attempt) when converted to UK local time.
            var version1UpdateNote0Date = GetDateTimeForUpdateNoteOnSameUkDay(version1PublishedScheduled);
            var version2UpdateNote0Date = version1UpdateNote0Date;
            var version2UpdateNote1Date = GetDateTimeForUpdateNoteOnSameUkDay(version2PublishedScheduled);

            // Set up the latest successful publishing attempt timestamps
            // This is irrelevant for version 1 without a history of publishing attempts for it.
            // Version 2's latest successful attempt is just after its scheduled trigger time because it's method is 'Immediate'.
            var version2LatestAttempt = version2PublishedScheduled.AddSeconds(45);

            Publication publication = _dataFixture
                .DefaultPublication()
                .WithTheme(_dataFixture.DefaultTheme())
                .WithReleases(_ =>
                    [
                        _dataFixture
                            .DefaultRelease()
                            .WithVersions(_ =>
                                [
                                    // Version 0 - not relevant
                                    _dataFixture
                                        .DefaultReleaseVersion()
                                        .WithId(releaseVersion0Id)
                                        .WithVersion(0)
                                        .WithPublished(version0Published)
                                        .WithPublishScheduled(version0PublishedScheduled),
                                    // Version 1 - relevant but has no history of publishing attempts
                                    _dataFixture
                                        .DefaultReleaseVersion()
                                        .WithId(releaseVersion1Id)
                                        .WithVersion(1)
                                        .WithPublished(version1Published)
                                        .WithPublishScheduled(version1PublishedScheduled)
                                        .WithUpdates(
                                            _dataFixture
                                                .DefaultUpdate()
                                                .ForIndex(0, s => s.SetOn(version1UpdateNote0Date))
                                                .GenerateList(1)
                                        ),
                                    // Version 2 - relevant
                                    _dataFixture
                                        .DefaultReleaseVersion()
                                        .WithId(releaseVersion2Id)
                                        .WithVersion(2)
                                        .WithPublished(version2Published)
                                        .WithPublishScheduled(version2PublishedScheduled)
                                        .WithUpdates(
                                            _dataFixture
                                                .DefaultUpdate()
                                                .ForIndex(0, s => s.SetOn(version2UpdateNote0Date))
                                                .ForIndex(1, s => s.SetOn(version2UpdateNote1Date))
                                                .GenerateList(2)
                                        ),
                                ]
                            ),
                    ]
                );

            var releasePublishingStatusRepository = new Mock<IReleasePublishingStatusRepository>(MockBehavior.Strict);
            // Set up no history of publishing attempts to be returned for version 1
            releasePublishingStatusRepository.SetupGetAllByOverallStageCompleteReturnsNoResults(releaseVersion1Id);
            releasePublishingStatusRepository.SetupGetAllByOverallStageCompleteReturnsSingleResult(
                releaseVersion2Id,
                immediate: version2Immediate,
                timestamp: version2LatestAttempt
            );

            var contextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryContentDbContext(contextId))
            {
                context.Publications.Add(publication);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var sut = BuildService(
                    context,
                    releasePublishingStatusRepository: releasePublishingStatusRepository.Object
                );

                // Act
                var outcome = await sut.MigrateReleaseVersionsPublishedDate(dryRun: false);

                // Assert - report
                var report = outcome.AssertRight();

                // The count of relevant versions should exclude version 0, but include version 1 and version 2,
                // even though version 1 has no history of publishing attempts, because it is still relevant to
                // be considered by the migration.
                Assert.Equal(2, report.TotalRelevantReleaseVersions);

                // Only version 2 should be updated
                Assert.Equal(1, report.TotalReleaseVersionsToUpdate);
                Assert.Single(report.ReleaseVersionsToUpdate);
                Assert.Equal(releaseVersion2Id, report.ReleaseVersionsToUpdate[0]);

                // Version 1 should not be updated
                Assert.Equal(1, report.TotalReleaseVersionsNotToUpdate);
                Assert.Single(report.ReleaseVersionsNotToUpdate);
                Assert.Equal(releaseVersion1Id, report.ReleaseVersionsNotToUpdate[0]);

                // There should be warnings for version 1
                Assert.Equal(1, report.TotalReleaseVersionsWithWarnings);
                Assert.Single(report.ReleaseVersionsWithWarnings);
                Assert.Equal(releaseVersion1Id, report.ReleaseVersionsWithWarnings[0]);

                // Deep dive on the report data for version 1
                // Because there is no history of publishing attempts for version 1, a lot of the values are Null
                var releaseVersion1Report = GetReleaseVersionReportFromReport(report, releaseVersion1Id);
                Assert.Null(releaseVersion1Report.PublishingInfo.LatestAttemptTimestamp);
                Assert.Null(releaseVersion1Report.PublishingInfo.LatestAttemptTimestampUkDateOnly);
                Assert.Equal(ReleasePublishingMethod.Unknown, releaseVersion1Report.PublishingInfo.PublishMethod);
                Assert.Equal(version1PublishedScheduled, releaseVersion1Report.PublishingInfo.ScheduledPublishTrigger);
                // Without the history of publishing attempts, the method is 'Unknown'.
                // Even though the scheduled trigger date is at midnight, ScheduledPublishFinalStageTrigger doesn't get adjusted to 09:30.
                Assert.Equal(
                    version1PublishedScheduled,
                    releaseVersion1Report.PublishingInfo.ScheduledPublishFinalStageTrigger
                );
                Assert.Equal(0, releaseVersion1Report.PublishingInfo.SuccessfulPublishingAttempts);
                Assert.Null(releaseVersion1Report.PublishingInfo.TimeSinceScheduledTriggerToCompletion);
                Assert.Null(releaseVersion1Report.PublishingInfo.TimeSinceScheduledTriggerToCompletionPretty);
                Assert.Equal(1, releaseVersion1Report.UpdateNotes.UpdatesCount);
                Assert.Equal(
                    version1PublishedScheduled.ToUkDateOnly(),
                    releaseVersion1Report.UpdateNotes.LatestUpdateNoteUkDateOnly
                );
                Assert.True(releaseVersion1Report.Warnings.HasWarnings);

                // Deep dive on the report data for version 2
                var releaseVersion2Report = GetReleaseVersionReportFromReport(report, releaseVersion2Id);
                Assert.Equal(version2LatestAttempt, releaseVersion2Report.PublishingInfo.LatestAttemptTimestamp);
                Assert.Equal(
                    version2LatestAttempt.ToUkDateOnly(),
                    releaseVersion2Report.PublishingInfo.LatestAttemptTimestampUkDateOnly
                );
                Assert.Equal(ReleasePublishingMethod.Immediate, releaseVersion2Report.PublishingInfo.PublishMethod);
                Assert.Equal(version2PublishedScheduled, releaseVersion2Report.PublishingInfo.ScheduledPublishTrigger);
                Assert.Equal(
                    version2PublishedScheduled,
                    releaseVersion2Report.PublishingInfo.ScheduledPublishFinalStageTrigger
                );
                Assert.Equal(1, releaseVersion2Report.PublishingInfo.SuccessfulPublishingAttempts);
                Assert.Equal(
                    version2LatestAttempt - version2PublishedScheduled,
                    releaseVersion2Report.PublishingInfo.TimeSinceScheduledTriggerToCompletion
                );
                Assert.Equal(
                    (version2LatestAttempt - version2PublishedScheduled).PrettyPrint(),
                    releaseVersion2Report.PublishingInfo.TimeSinceScheduledTriggerToCompletionPretty
                );
                Assert.Equal(2, releaseVersion2Report.UpdateNotes.UpdatesCount);
                Assert.Equal(
                    version2PublishedScheduled.ToUkDateOnly(),
                    releaseVersion2Report.UpdateNotes.LatestUpdateNoteUkDateOnly
                );
                Assert.False(releaseVersion2Report.Warnings.HasWarnings);

                releasePublishingStatusRepository.VerifyAll();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                // Assert - database
                var actualReleaseVersions = await context.ReleaseVersions.AsNoTracking().ToListAsync();

                var actualReleaseVersion0 = actualReleaseVersions.Single(rv => rv.Id == releaseVersion0Id);
                var actualReleaseVersion1 = actualReleaseVersions.Single(rv => rv.Id == releaseVersion1Id);
                var actualReleaseVersion2 = actualReleaseVersions.Single(rv => rv.Id == releaseVersion2Id);

                // Version 0 should be unchanged because it was not relevant, being the first version
                Assert.Equal(version0Published, actualReleaseVersion0.Published);

                // Version 1 should be unchanged because it had no history of publishing attempts,
                // so there was no proposed new value for 'Published'
                Assert.Equal(version1Published, actualReleaseVersion1.Published);

                // Version 2 should be updated to have the same 'Published' value as its latest successful publishing attempt
                Assert.Equal(version2LatestAttempt, actualReleaseVersion2.Published);
            }
        }
    }

    public class MigrateReleaseVersionsPublishedDateRelevanceTests : ReleaseVersionsMigrationServiceTests
    {
        [Fact]
        public async Task WhenReleaseVersionIsFirstVersion_ReleaseVersionIsExcludedFromMigration()
        {
            // Arrange
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithTheme(_dataFixture.DefaultTheme())
                .WithReleases(_ =>
                    [
                        _dataFixture
                            .DefaultRelease()
                            .WithVersions(_ =>
                                [
                                    _dataFixture
                                        .DefaultReleaseVersion()
                                        .WithVersion(0)
                                        .WithPublished(DefaultPublished)
                                        .WithUpdatePublishedDisplayDate(false),
                                ]
                            ),
                    ]
                );
            var releaseVersionId = publication.Releases.Single().Versions.Single().Id;

            var contextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryContentDbContext(contextId))
            {
                context.Publications.Add(publication);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var sut = BuildService(context);

                // Act
                var outcome = await sut.MigrateReleaseVersionsPublishedDate(dryRun: false);

                // Assert - report
                var report = outcome.AssertRight();
                Assert.Empty(report.Publications);

                Assert.Equal(0, report.TotalReleaseVersionsNotToUpdate);
                Assert.Equal(0, report.TotalReleaseVersionsToUpdate);
                Assert.Equal(0, report.TotalRelevantReleaseVersions);
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                // Assert - database
                var actualReleaseVersion = await context
                    .ReleaseVersions.AsNoTracking()
                    .SingleAsync(rv => rv.Id == releaseVersionId);

                Assert.Equal(DefaultPublished, actualReleaseVersion.Published);
            }
        }

        [Fact]
        public async Task WhenReleaseVersionIsNotPublished_ReleaseVersionIsExcludedFromMigration()
        {
            // Arrange
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithTheme(_dataFixture.DefaultTheme())
                .WithReleases(_ =>
                    [
                        _dataFixture
                            .DefaultRelease()
                            .WithVersions(_ =>
                                [
                                    _dataFixture
                                        .DefaultReleaseVersion()
                                        .WithVersion(1)
                                        .WithPublished(null)
                                        .WithUpdatePublishedDisplayDate(false),
                                ]
                            ),
                    ]
                );
            var releaseVersionId = publication.Releases.Single().Versions.Single().Id;

            var contextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryContentDbContext(contextId))
            {
                context.Publications.Add(publication);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var sut = BuildService(context);

                // Act
                var outcome = await sut.MigrateReleaseVersionsPublishedDate(dryRun: false);

                // Assert - report
                var report = outcome.AssertRight();
                Assert.Empty(report.Publications);

                Assert.Equal(0, report.TotalReleaseVersionsNotToUpdate);
                Assert.Equal(0, report.TotalReleaseVersionsToUpdate);
                Assert.Equal(0, report.TotalRelevantReleaseVersions);
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                // Assert - database
                var actualReleaseVersion = await context
                    .ReleaseVersions.AsNoTracking()
                    .SingleAsync(rv => rv.Id == releaseVersionId);

                Assert.Null(actualReleaseVersion.Published);
            }
        }

        [Fact]
        public async Task WhenReleaseVersionUpdatePublishedDateFlagIsTrue_ReleaseVersionIsExcludedFromMigration()
        {
            // Arrange
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithTheme(_dataFixture.DefaultTheme())
                .WithReleases(_ =>
                    [
                        _dataFixture
                            .DefaultRelease()
                            .WithVersions(_ =>
                                [
                                    _dataFixture
                                        .DefaultReleaseVersion()
                                        .WithVersion(1)
                                        .WithPublished(DefaultPublished)
                                        .WithUpdatePublishedDisplayDate(true),
                                ]
                            ),
                    ]
                );
            var releaseVersionId = publication.Releases.Single().Versions.Single().Id;

            var contextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryContentDbContext(contextId))
            {
                context.Publications.Add(publication);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var sut = BuildService(context);

                // Act
                var outcome = await sut.MigrateReleaseVersionsPublishedDate(dryRun: false);

                // Assert
                var report = outcome.AssertRight();
                Assert.Empty(report.Publications);

                Assert.Equal(0, report.TotalReleaseVersionsNotToUpdate);
                Assert.Equal(0, report.TotalReleaseVersionsToUpdate);
                Assert.Equal(0, report.TotalRelevantReleaseVersions);
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                // Assert - database
                var actualReleaseVersion = await context
                    .ReleaseVersions.AsNoTracking()
                    .SingleAsync(rv => rv.Id == releaseVersionId);

                Assert.Equal(DefaultPublished, actualReleaseVersion.Published);
            }
        }
    }

    public class MigrateReleaseVersionsPublishedDateMiscTests : ReleaseVersionsMigrationServiceTests
    {
        public static TheoryData<AppEnvironment> AppEnvironmentValues => new(Enum.GetValues<AppEnvironment>());

        [Theory]
        [MemberData(nameof(AppEnvironmentValues))]
        public async Task ReportIndicatesAppEnvironment(AppEnvironment appEnvironment)
        {
            // Arrange
            // Create the AppOptions with 'Url' property corresponding to the URL of the environment being tested
            var appOptions = CreateAppOptions(appEnvironment);

            await using var context = InMemoryContentDbContext();
            var sut = BuildService(context, appOptions);

            // Act
            var outcome = await sut.MigrateReleaseVersionsPublishedDate();

            // Assert
            var report = outcome.AssertRight();

            // The environment should have been determined using the 'Url' property of the AppOptions.
            Assert.Equal(appEnvironment, report.AppEnvironment);
        }

        [Fact]
        public void WhenAppOptionsUrlIsUnexpected_ThrowsExceptionBuildingService()
        {
            // Arrange
            var appOptions = CreateAppOptions("https://unexpected.url");

            using var context = InMemoryContentDbContext();

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(() => BuildService(context, appOptions));
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task ReportIndicatesDryRun(bool dryRun)
        {
            // Arrange
            await using var context = InMemoryContentDbContext();
            var sut = BuildService(context);

            // Act
            var outcome = await sut.MigrateReleaseVersionsPublishedDate(dryRun: dryRun);

            // Assert
            var report = outcome.AssertRight();

            Assert.Equal(dryRun, report.DryRun);
        }
    }

    public class MigrateReleaseVersionsPublishedDateWarningTests : ReleaseVersionsMigrationServiceTests
    {
        [Fact]
        public async Task WhenReleaseVersionHasNoSuccessfulPublishingAttempts_ReportIncludesWarnings()
        {
            // Arrange
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithTheme(_dataFixture.DefaultTheme())
                .WithReleases(_ =>
                    [
                        _dataFixture
                            .DefaultRelease()
                            .WithVersions(_ =>
                                [
                                    _dataFixture
                                        .DefaultReleaseVersion()
                                        .WithVersion(1)
                                        .WithPublished(DefaultPublished)
                                        .WithPublishScheduled(DefaultPublishedScheduled)
                                        .WithUpdates(
                                            _dataFixture.DefaultUpdate().WithOn(DefaultUpdateNoteDate).GenerateList(1)
                                        ),
                                ]
                            ),
                    ]
                );
            var releaseVersionId = publication.Releases.Single().Versions.Single(rv => rv.Version == 1).Id;

            var releasePublishingStatusRepository = new Mock<IReleasePublishingStatusRepository>(MockBehavior.Strict);
            releasePublishingStatusRepository.SetupGetAllByOverallStageCompleteReturnsNoResults(releaseVersionId);

            var contextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryContentDbContext(contextId))
            {
                context.Publications.Add(publication);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var sut = BuildService(
                    context,
                    releasePublishingStatusRepository: releasePublishingStatusRepository.Object
                );

                // Act
                var outcome = await sut.MigrateReleaseVersionsPublishedDate();

                // Assert
                var report = outcome.AssertRight();
                var releaseVersionReport = GetReleaseVersionReportFromReport(report, releaseVersionId);

                Assert.True(releaseVersionReport.Warnings.NoSuccessfulPublishingAttempts);

                // This also causes the 'ProposedPublishedDateIsNotSimilarToScheduledTriggerDate' warning to be true,
                // as the proposed published date is based on the latest successful publishing attempt,
                // and there isn't one.
                Assert.True(releaseVersionReport.Warnings.ProposedPublishedDateIsNotSimilarToScheduledTriggerDate);

                AssertReportHasSingleReleaseVersionWithWarnings(report, releaseVersionId);

                // There should be no other types of warning for this release version
                Assert.Null(releaseVersionReport.Warnings.MultipleSuccessfulPublishingAttempts);
                Assert.Null(releaseVersionReport.Warnings.UpdatesCountDoesNotMatchVersion);
                Assert.Null(releaseVersionReport.Warnings.ProposedPublishedDateDoesNotMatchLatestUpdateNoteDate);
                Assert.Null(releaseVersionReport.Warnings.ScheduledTriggerDateIsNotMidnightUkLocalTime);

                // Without a proposed published date, the release version should NOT be updated
                AssertReportHasSingleReleaseVersionNotToUpdate(report, releaseVersionId);

                releasePublishingStatusRepository.VerifyAll();
            }
        }

        [Fact]
        public async Task WhenReleaseVersionHasMultipleSuccessfulPublishingAttempts_ReportIncludesWarning()
        {
            // Arrange
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithTheme(_dataFixture.DefaultTheme())
                .WithReleases(_ =>
                    [
                        _dataFixture
                            .DefaultRelease()
                            .WithVersions(_ =>
                                [
                                    _dataFixture
                                        .DefaultReleaseVersion()
                                        .WithVersion(1)
                                        .WithPublished(DefaultPublished)
                                        .WithPublishScheduled(DefaultPublishedScheduled)
                                        .WithUpdates(
                                            _dataFixture.DefaultUpdate().WithOn(DefaultUpdateNoteDate).GenerateList(1)
                                        ),
                                ]
                            ),
                    ]
                );
            var releaseVersionId = publication.Releases.Single().Versions.Single(rv => rv.Version == 1).Id;

            var releasePublishingStatusRepository = new Mock<IReleasePublishingStatusRepository>(MockBehavior.Strict);
            releasePublishingStatusRepository
                .Setup(r => r.GetAllByOverallStage(releaseVersionId, ReleasePublishingStatusOverallStage.Complete))
                .ReturnsAsync([
                    new ReleasePublishingStatus
                    {
                        Created = DateTime.UtcNow,
                        Timestamp = DefaultLatestAttemptTimestamp,
                    },
                    new ReleasePublishingStatus
                    {
                        Created = DateTime.UtcNow,
                        Timestamp = DefaultLatestAttemptTimestamp,
                    },
                ]);

            var contextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryContentDbContext(contextId))
            {
                context.Publications.Add(publication);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var sut = BuildService(
                    context,
                    releasePublishingStatusRepository: releasePublishingStatusRepository.Object
                );

                // Act
                var outcome = await sut.MigrateReleaseVersionsPublishedDate();

                // Assert
                var report = outcome.AssertRight();
                var releaseVersionReport = GetReleaseVersionReportFromReport(report, releaseVersionId);

                Assert.True(releaseVersionReport.Warnings.MultipleSuccessfulPublishingAttempts);
                AssertReportHasSingleReleaseVersionWithWarnings(report, releaseVersionId);

                // There should be no other types of warning for this release version
                Assert.Null(releaseVersionReport.Warnings.NoSuccessfulPublishingAttempts);
                Assert.Null(releaseVersionReport.Warnings.UpdatesCountDoesNotMatchVersion);
                Assert.Null(releaseVersionReport.Warnings.ProposedPublishedDateDoesNotMatchLatestUpdateNoteDate);
                Assert.Null(releaseVersionReport.Warnings.ProposedPublishedDateIsNotSimilarToScheduledTriggerDate);
                Assert.Null(releaseVersionReport.Warnings.ScheduledTriggerDateIsNotMidnightUkLocalTime);

                // This type of warning should not stop the release version from being updated
                AssertReportHasSingleReleaseVersionToUpdate(report, releaseVersionId);

                releasePublishingStatusRepository.VerifyAll();
            }
        }

        [Theory]
        [InlineData(1, 0, true)]
        [InlineData(1, 1, false)] // 1 amendment and 1 update - this is the expected scenario, so no warning
        [InlineData(1, 2, true)]
        [InlineData(2, 0, true)]
        [InlineData(2, 1, true)]
        [InlineData(2, 2, false)] // 2 amendments and 2 updates - this is the expected scenario, so no warning
        [InlineData(2, 3, true)]
        public async Task WhenUpdatesCountDoesNotMatchVersion_ReportIncludesWarning(
            int releaseVersionNumber,
            int updatesCount,
            bool expectedWarning
        )
        {
            // Arrange
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithTheme(_dataFixture.DefaultTheme())
                .WithReleases(_ =>
                    [
                        _dataFixture
                            .DefaultRelease()
                            .WithVersions(_ =>
                                [
                                    _dataFixture
                                        .DefaultReleaseVersion()
                                        .WithVersion(releaseVersionNumber)
                                        .WithPublished(DefaultPublished)
                                        .WithPublishScheduled(DefaultPublishedScheduled)
                                        .WithUpdates(
                                            _dataFixture
                                                .DefaultUpdate()
                                                .WithOn(DefaultUpdateNoteDate)
                                                .GenerateList(updatesCount)
                                        ),
                                ]
                            ),
                    ]
                );
            var releaseVersionId = publication.Releases.Single().Versions.Single().Id;

            var releasePublishingStatusRepository = new Mock<IReleasePublishingStatusRepository>(MockBehavior.Strict);
            releasePublishingStatusRepository.SetupGetAllByOverallStageCompleteReturnsSingleResult(
                releaseVersionId,
                timestamp: DefaultLatestAttemptTimestamp
            );

            var contextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryContentDbContext(contextId))
            {
                context.Publications.Add(publication);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var sut = BuildService(
                    context,
                    releasePublishingStatusRepository: releasePublishingStatusRepository.Object
                );

                // Act
                var outcome = await sut.MigrateReleaseVersionsPublishedDate();

                // Assert
                var report = outcome.AssertRight();
                var releaseVersionReport = GetReleaseVersionReportFromReport(report, releaseVersionId);

                if (expectedWarning)
                {
                    Assert.True(releaseVersionReport.Warnings.UpdatesCountDoesNotMatchVersion);
                    AssertReportHasSingleReleaseVersionWithWarnings(report, releaseVersionId);
                }
                else
                {
                    Assert.Null(releaseVersionReport.Warnings.UpdatesCountDoesNotMatchVersion);
                }

                // There should be no other types of warning for this release version
                Assert.Null(releaseVersionReport.Warnings.NoSuccessfulPublishingAttempts);
                Assert.Null(releaseVersionReport.Warnings.MultipleSuccessfulPublishingAttempts);
                Assert.Null(releaseVersionReport.Warnings.ProposedPublishedDateDoesNotMatchLatestUpdateNoteDate);
                Assert.Null(releaseVersionReport.Warnings.ProposedPublishedDateIsNotSimilarToScheduledTriggerDate);
                Assert.Null(releaseVersionReport.Warnings.ScheduledTriggerDateIsNotMidnightUkLocalTime);

                if (!expectedWarning)
                {
                    AssertReportHasNoReleaseVersionsWithWarnings(report);
                }

                // This type of warning should not stop the release version from being updated
                AssertReportHasSingleReleaseVersionToUpdate(report, releaseVersionId);

                releasePublishingStatusRepository.VerifyAll();
            }
        }

        [Theory]
        [InlineData("2025-01-01T09:30:25 +00:00", "2025-01-02T00:00:00", ReleasePublishingMethod.Scheduled, true)] // Duration is negative (actual duration= -23 hours, 59 minutes, 35 seconds)
        [InlineData("2025-01-01T16:30:25 +00:00", "2025-01-01T17:00:25", ReleasePublishingMethod.Immediate, true)] // Duration is negative (actual duration= -30 minutes)
        [InlineData("2025-01-01T09:30:25 +00:00", "2025-01-01T00:00:00", ReleasePublishingMethod.Scheduled, false)] // Within 5 minute tolerance (actual duration= 25 seconds)
        [InlineData("2025-01-01T09:35:00 +00:00", "2025-01-01T00:00:00", ReleasePublishingMethod.Scheduled, false)] // Within 5 minute tolerance (actual duration= 5 minutes)
        [InlineData("2025-01-01T09:35:01 +00:00", "2025-01-01T00:00:00", ReleasePublishingMethod.Scheduled, true)] // Outside 5 minute tolerance (actual duration= 5 minutes, 1 second)
        [InlineData("2025-01-02T09:30:25 +00:00", "2025-01-01T00:00:00", ReleasePublishingMethod.Scheduled, true)] // Outside 5 minute tolerance (actual duration= 1 day, 25 seconds)
        [InlineData("2025-01-01T16:30:25 +00:00", "2025-01-01T16:30:00", ReleasePublishingMethod.Immediate, false)] // Within 6 hour tolerance (actual duration= 25 seconds)
        [InlineData("2025-01-01T16:30:25 +00:00", "2025-01-01T10:30:25", ReleasePublishingMethod.Immediate, false)] // Within 6 hour tolerance (actual duration= 6 hours)
        [InlineData("2025-01-01T16:30:25 +00:00", "2025-01-01T10:30:24", ReleasePublishingMethod.Immediate, true)] // Outside 6 hour tolerance (actual duration= 6 hours, 1 second)
        [InlineData("2025-01-02T16:30:25 +00:00", "2025-01-01T16:30:00", ReleasePublishingMethod.Immediate, true)] // Outside 6 hour tolerance (actual duration= 1 day, 25 seconds)
        public async Task WhenProposedPublishedDateIsNotSimilarToScheduledTriggerDate_ReportIncludesWarning(
            string latestAttemptTimestampString,
            string publishedScheduledString,
            ReleasePublishingMethod method,
            bool expectedWarning
        )
        {
            // There are differences in how this warning is determined for 'Immediate' vs 'Scheduled' methods,
            // and when the method is 'Scheduled', the AppEnvironment (Local/Dev/Test/Pre-Prod/Prod) is a factor too.
            // The Local, Dev and Test environments should have the same behavior,
            // and the Pre-Prod and Prod environments should have the same behavior, but different to Local, Dev and Test.

            // This test doesn't go to the extent of preparing data for testing all the different environments.
            // It only tests the Pre-Prod/Prod behavior. The AppOptions are set up in the same way as all the other
            // unit tests with the environment defaulting to Prod.

            // Arrange
            var latestAttemptTimestamp = DateTimeOffset.Parse(latestAttemptTimestampString);
            var publishScheduled = DateTimeOffset.Parse(publishedScheduledString);

            // Unrelated to setting up data for this warning, but for the purpose of not causing any other warnings:

            // (1) The release update note needs to have the same date-only element as the latest attempt timestamp
            // to avoid causing the 'ProposedPublishedDateDoesNotMatchLatestUpdateNoteDate' warning.
            var updateNoteDate = GetDateTimeForUpdateNoteOnSameUkDay(latestAttemptTimestamp);

            Publication publication = _dataFixture
                .DefaultPublication()
                .WithTheme(_dataFixture.DefaultTheme())
                .WithReleases(_ =>
                    [
                        _dataFixture
                            .DefaultRelease()
                            .WithVersions(_ =>
                                [
                                    _dataFixture
                                        .DefaultReleaseVersion()
                                        .WithVersion(1)
                                        .WithPublished(DefaultPublished)
                                        .WithPublishScheduled(publishScheduled)
                                        .WithUpdates(
                                            _dataFixture.DefaultUpdate().WithOn(updateNoteDate).GenerateList(1)
                                        ),
                                ]
                            ),
                    ]
                );
            var releaseVersionId = publication.Releases.Single().Versions.Single(rv => rv.Version == 1).Id;

            var releasePublishingStatusRepository = new Mock<IReleasePublishingStatusRepository>(MockBehavior.Strict);
            releasePublishingStatusRepository.SetupGetAllByOverallStageCompleteReturnsSingleResult(
                releaseVersionId,
                immediate: method == ReleasePublishingMethod.Immediate,
                timestamp: latestAttemptTimestamp
            );

            var contextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryContentDbContext(contextId))
            {
                context.Publications.Add(publication);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var sut = BuildService(
                    context,
                    releasePublishingStatusRepository: releasePublishingStatusRepository.Object
                );

                // Act
                var outcome = await sut.MigrateReleaseVersionsPublishedDate();

                // Assert
                var report = outcome.AssertRight();
                var releaseVersionReport = GetReleaseVersionReportFromReport(report, releaseVersionId);

                if (expectedWarning)
                {
                    Assert.True(releaseVersionReport.Warnings.ProposedPublishedDateIsNotSimilarToScheduledTriggerDate);
                    AssertReportHasSingleReleaseVersionWithWarnings(report, releaseVersionId);
                }
                else
                {
                    Assert.Null(releaseVersionReport.Warnings.ProposedPublishedDateIsNotSimilarToScheduledTriggerDate);
                }

                // There should be no other types of warning for this release version
                Assert.Null(releaseVersionReport.Warnings.NoSuccessfulPublishingAttempts);
                Assert.Null(releaseVersionReport.Warnings.MultipleSuccessfulPublishingAttempts);
                Assert.Null(releaseVersionReport.Warnings.UpdatesCountDoesNotMatchVersion);
                Assert.Null(releaseVersionReport.Warnings.ProposedPublishedDateDoesNotMatchLatestUpdateNoteDate);
                Assert.Null(releaseVersionReport.Warnings.ScheduledTriggerDateIsNotMidnightUkLocalTime);

                if (!expectedWarning)
                {
                    AssertReportHasNoReleaseVersionsWithWarnings(report);
                }

                // This type of warning should not stop the release version from being updated
                AssertReportHasSingleReleaseVersionToUpdate(report, releaseVersionId);

                releasePublishingStatusRepository.VerifyAll();
            }
        }

        [Theory]
        [InlineData("2025-01-01T09:30:25 +00:00", "2025-01-01T00:00:00", ReleasePublishingMethod.Scheduled, false)] // Date-only elements match (1st Jan)
        [InlineData("2025-01-01T09:30:25 +00:00", "2025-01-01T07:00:00", ReleasePublishingMethod.Scheduled, false)] // Date-only elements match (1st Jan)
        [InlineData("2025-01-01T09:30:25 +00:00", "2025-01-01T16:00:00", ReleasePublishingMethod.Scheduled, false)] // Date-only elements match (1st Jan)
        [InlineData("2025-01-01T09:30:25 +00:00", "2024-12-31T07:00:00", ReleasePublishingMethod.Scheduled, true)] // Date-only elements mismatch (1st Jan vs 31st Dec)
        [InlineData("2025-01-01T09:30:25 +00:00", "2024-12-31T16:00:00", ReleasePublishingMethod.Scheduled, true)] // Date-only elements mismatch (1st Jan vs 31st Dec)
        [InlineData("2025-01-01T09:30:25 +00:00", "2025-01-02T07:00:00", ReleasePublishingMethod.Scheduled, true)] // Date-only elements mismatch (1st Jan vs 2nd Jan)
        [InlineData("2025-01-01T09:30:25 +00:00", "2025-01-02T16:00:00", ReleasePublishingMethod.Scheduled, true)] // Date-only elements mismatch (1st Jan vs 2nd Jan)
        [InlineData("2025-05-31T23:30:25 +00:00", "2025-05-31T16:00:00", ReleasePublishingMethod.Immediate, true)] // Date-only elements mismatch (1st June vs 31st May when converted to UK local time)
        [InlineData("2025-05-31T23:30:25 +00:00", "2025-05-31T23:00:00", ReleasePublishingMethod.Immediate, false)] // Date-only elements match (1st June when converted to UK local time)
        [InlineData("2025-06-01T08:30:25 +00:00", "2025-05-31T23:00:00", ReleasePublishingMethod.Scheduled, false)] // Date-only elements match (1st June when converted to UK local time)
        public async Task WhenProposedPublishedDateDoesNotMatchLatestUpdateNoteDate_ReportIncludesWarning(
            string latestAttemptTimestampString,
            string updateNoteDateString,
            ReleasePublishingMethod method,
            bool expectedWarning
        )
        {
            // Arrange
            var latestAttemptTimestamp = DateTimeOffset.Parse(latestAttemptTimestampString);
            var updateNoteDate = DateTime.Parse(updateNoteDateString);

            // Unrelated to setting up data for this warning, but for the purpose of not causing any other warnings:

            // (1) The scheduled trigger time (of the first stage because this test defaults to using method 'Scheduled'),
            // i.e. 'PublishScheduled', needs to be at midnight UK local time on the same day as the latest attempt timestamp
            // to avoid causing the 'ProposedPublishedDateIsNotSimilarToScheduledTriggerDate' warning.
            var publishScheduled = latestAttemptTimestamp.GetUkStartOfDayUtc();

            Publication publication = _dataFixture
                .DefaultPublication()
                .WithTheme(_dataFixture.DefaultTheme())
                .WithReleases(_ =>
                    [
                        _dataFixture
                            .DefaultRelease()
                            .WithVersions(_ =>
                                [
                                    _dataFixture
                                        .DefaultReleaseVersion()
                                        .WithVersion(1)
                                        .WithPublished(DefaultPublished)
                                        .WithPublishScheduled(publishScheduled)
                                        .WithUpdates(
                                            _dataFixture.DefaultUpdate().WithOn(updateNoteDate).GenerateList(1)
                                        ),
                                ]
                            ),
                    ]
                );
            var releaseVersionId = publication.Releases.Single().Versions.Single(rv => rv.Version == 1).Id;

            var releasePublishingStatusRepository = new Mock<IReleasePublishingStatusRepository>(MockBehavior.Strict);
            releasePublishingStatusRepository.SetupGetAllByOverallStageCompleteReturnsSingleResult(
                releaseVersionId,
                immediate: method == ReleasePublishingMethod.Immediate,
                timestamp: latestAttemptTimestamp
            );

            var contextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryContentDbContext(contextId))
            {
                context.Publications.Add(publication);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var sut = BuildService(
                    context,
                    releasePublishingStatusRepository: releasePublishingStatusRepository.Object
                );

                // Act
                var outcome = await sut.MigrateReleaseVersionsPublishedDate();

                // Assert
                var report = outcome.AssertRight();
                var releaseVersionReport = GetReleaseVersionReportFromReport(report, releaseVersionId);

                if (expectedWarning)
                {
                    Assert.True(releaseVersionReport.Warnings.ProposedPublishedDateDoesNotMatchLatestUpdateNoteDate);
                    AssertReportHasSingleReleaseVersionWithWarnings(report, releaseVersionId);
                }
                else
                {
                    Assert.Null(releaseVersionReport.Warnings.ProposedPublishedDateDoesNotMatchLatestUpdateNoteDate);
                }

                // There should be no other types of warning for this release version
                Assert.Null(releaseVersionReport.Warnings.NoSuccessfulPublishingAttempts);
                Assert.Null(releaseVersionReport.Warnings.MultipleSuccessfulPublishingAttempts);
                Assert.Null(releaseVersionReport.Warnings.UpdatesCountDoesNotMatchVersion);
                Assert.Null(releaseVersionReport.Warnings.ProposedPublishedDateIsNotSimilarToScheduledTriggerDate);
                Assert.Null(releaseVersionReport.Warnings.ScheduledTriggerDateIsNotMidnightUkLocalTime);

                if (!expectedWarning)
                {
                    AssertReportHasNoReleaseVersionsWithWarnings(report);
                }

                // This type of warning should not stop the release version from being updated
                AssertReportHasSingleReleaseVersionToUpdate(report, releaseVersionId);

                releasePublishingStatusRepository.VerifyAll();
            }
        }

        [Theory]
        [InlineData("2025-01-01T00:00:00 +00:00", false)] // 00:00 UTC on 1st Jan is also 00:00 UK local time (GMT)
        [InlineData("2025-01-01T23:00:00 +00:00", true)] // 23:00 UTC on 1st Jan is also 23:00 UK local time (GMT)
        [InlineData("2025-03-30T00:00:00 +00:00", false)] // 00:00 UTC on 30th Mar is one hour before BST starts and 00:00 UK local time (GMT)
        [InlineData("2025-06-01T00:00:00 +00:00", true)] // 00:00 UTC on 1st June is 01:00 UK local time (BST)
        [InlineData("2025-06-01T23:00:00 +00:00", false)] // 23:00 UTC on 1st June is 00:00 UK local time (BST)
        [InlineData("2025-10-26T00:00:00 +00:00", true)] // 00:00 UTC on 26th Oct is one hour before BST ends and 01:00 UK local time (BST)
        [InlineData("2025-10-26T01:00:00 +00:00", true)] // 01:00 UTC on 26th Oct is the instant BST ends and 01:00 UK local time (GMT)
        public async Task WhenScheduledTriggerDateIsNotMidnightUkLocalTime_ReportIncludesWarning(
            string publishedScheduledString,
            bool expectedWarning
        )
        {
            // Arrange
            var publishScheduled = DateTimeOffset.Parse(publishedScheduledString);

            // Unrelated to setting up data for this warning, but for the purpose of not causing any other warnings:

            // (1) The latest attempt timestamp needs to be the same as the final stage trigger date
            // (i.e. 'PublishScheduled', adjusted from midnight to 09:30 UK local time if it was midnight),
            // or within 'PublishingToleranceScheduled' of that date,
            // to avoid causing the 'ProposedPublishedDateIsNotSimilarToScheduledTriggerDate' warning.
            var latestAttemptTimestamp = publishScheduled
                .AdjustUkLocalMidnightTo0930() // Makes no adjustment if not midnight
                .Add(ReleaseVersionsMigrationService.PublishingToleranceScheduled)
                .AddSeconds(-1);

            // (2) The release update note needs to have the same date-only element as the latest attempt timestamp
            // to avoid causing the 'ProposedPublishedDateDoesNotMatchLatestUpdateNoteDate' warning.
            var updateNoteDate = GetDateTimeForUpdateNoteOnSameUkDay(latestAttemptTimestamp);

            Publication publication = _dataFixture
                .DefaultPublication()
                .WithTheme(_dataFixture.DefaultTheme())
                .WithReleases(_ =>
                    [
                        _dataFixture
                            .DefaultRelease()
                            .WithVersions(_ =>
                                [
                                    _dataFixture
                                        .DefaultReleaseVersion()
                                        .WithVersion(1)
                                        .WithPublished(DefaultPublished)
                                        .WithPublishScheduled(publishScheduled)
                                        .WithUpdates(
                                            _dataFixture.DefaultUpdate().WithOn(updateNoteDate).GenerateList(1)
                                        ),
                                ]
                            ),
                    ]
                );
            var releaseVersionId = publication.Releases.Single().Versions.Single(rv => rv.Version == 1).Id;

            var releasePublishingStatusRepository = new Mock<IReleasePublishingStatusRepository>(MockBehavior.Strict);
            releasePublishingStatusRepository.SetupGetAllByOverallStageCompleteReturnsSingleResult(
                releaseVersionId,
                timestamp: latestAttemptTimestamp
            );

            var contextId = Guid.NewGuid().ToString();
            await using (var context = InMemoryContentDbContext(contextId))
            {
                context.Publications.Add(publication);
                await context.SaveChangesAsync();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                var sut = BuildService(
                    context,
                    releasePublishingStatusRepository: releasePublishingStatusRepository.Object
                );

                // Act
                var outcome = await sut.MigrateReleaseVersionsPublishedDate();

                // Assert
                var report = outcome.AssertRight();
                var releaseVersionReport = GetReleaseVersionReportFromReport(report, releaseVersionId);

                if (expectedWarning)
                {
                    Assert.True(releaseVersionReport.Warnings.ScheduledTriggerDateIsNotMidnightUkLocalTime);
                    AssertReportHasSingleReleaseVersionWithWarnings(report, releaseVersionId);
                }
                else
                {
                    Assert.Null(releaseVersionReport.Warnings.ScheduledTriggerDateIsNotMidnightUkLocalTime);
                }

                // There should be no other types of warning for this release version
                Assert.Null(releaseVersionReport.Warnings.NoSuccessfulPublishingAttempts);
                Assert.Null(releaseVersionReport.Warnings.MultipleSuccessfulPublishingAttempts);
                Assert.Null(releaseVersionReport.Warnings.UpdatesCountDoesNotMatchVersion);
                Assert.Null(releaseVersionReport.Warnings.ProposedPublishedDateDoesNotMatchLatestUpdateNoteDate);
                Assert.Null(releaseVersionReport.Warnings.ProposedPublishedDateIsNotSimilarToScheduledTriggerDate);

                if (!expectedWarning)
                {
                    AssertReportHasNoReleaseVersionsWithWarnings(report);
                }

                // This type of warning should not stop the release version from being updated
                AssertReportHasSingleReleaseVersionToUpdate(report, releaseVersionId);

                releasePublishingStatusRepository.VerifyAll();
            }
        }
    }

    private static DateTime GetDateTimeForUpdateNoteOnSameUkDay(DateTimeOffset input)
    {
        var ukMidnightDateTimeOffset = input.GetUkStartOfDayUtc();

        // The time element of release update notes should be insignificant,
        // but a time is set just because a release update note is unlikely to be created at exactly midnight.
        var withTime = ukMidnightDateTimeOffset.Add(UpdateNoteTime);

        // Return the result as a DateTime since Update.On is a DateTime rather than a DateTimeOffset
        return withTime.DateTime;
    }

    private static ReleaseVersionsMigrationReportReleaseVersionDto GetReleaseVersionReportFromReport(
        ReleaseVersionsMigrationReportDto report,
        Guid releaseVersionId
    ) =>
        report
            .Publications.Single()
            .Releases.Single()
            .ReleaseVersions.Single(rv => rv.ReleaseVersionId == releaseVersionId);

    private static void AssertReportHasNoReleaseVersionsWithWarnings(ReleaseVersionsMigrationReportDto report)
    {
        Assert.Empty(report.ReleaseVersionsWithWarnings);
        Assert.Equal(0, report.TotalReleaseVersionsWithWarnings);
    }

    private static void AssertReportHasSingleReleaseVersionWithWarnings(
        ReleaseVersionsMigrationReportDto report,
        Guid expectedReleaseVersionIdWithWarning
    )
    {
        var releaseVersionIdWithWarning = Assert.Single(report.ReleaseVersionsWithWarnings);
        Assert.Equal(1, report.TotalReleaseVersionsWithWarnings);
        Assert.Equal(expectedReleaseVersionIdWithWarning, releaseVersionIdWithWarning);
    }

    private static void AssertReportHasSingleReleaseVersionToUpdate(
        ReleaseVersionsMigrationReportDto report,
        Guid expectedReleaseVersionIdToBeUpdated
    )
    {
        var releaseVersionIdToUpdate = Assert.Single(report.ReleaseVersionsToUpdate);
        Assert.Equal(1, report.TotalReleaseVersionsToUpdate);
        Assert.Equal(expectedReleaseVersionIdToBeUpdated, releaseVersionIdToUpdate);

        // Assuming this was the only relevant release version to be set up ...
        Assert.Equal(1, report.TotalRelevantReleaseVersions);

        // ... means there should be no release versions NOT to update
        Assert.Empty(report.ReleaseVersionsNotToUpdate);
        Assert.Equal(0, report.TotalReleaseVersionsNotToUpdate);
    }

    private static void AssertReportHasSingleReleaseVersionNotToUpdate(
        ReleaseVersionsMigrationReportDto report,
        Guid expectedReleaseVersionIdNotToBeUpdated
    )
    {
        var releaseVersionIdNotToUpdate = Assert.Single(report.ReleaseVersionsNotToUpdate);
        Assert.Equal(1, report.TotalReleaseVersionsNotToUpdate);
        Assert.Equal(expectedReleaseVersionIdNotToBeUpdated, releaseVersionIdNotToUpdate);

        // Assuming this was the only relevant release version to be set up ...
        Assert.Equal(1, report.TotalRelevantReleaseVersions);

        // ... means there should be no release versions to update
        Assert.Empty(report.ReleaseVersionsToUpdate);
        Assert.Equal(0, report.TotalReleaseVersionsToUpdate);
    }

    private static OptionsWrapper<AppOptions> CreateAppOptions(AppEnvironment appEnvironment) =>
        CreateAppOptions(appEnvironment.GetAppEnvironmentUrl());

    private static OptionsWrapper<AppOptions> CreateAppOptions(string url) =>
        new AppOptions { Url = url }.ToOptionsWrapper();

    private static ReleaseVersionsMigrationService BuildService(
        ContentDbContext contentDbContext,
        IOptions<AppOptions>? appOptions = null,
        IReleasePublishingStatusRepository? releasePublishingStatusRepository = null
    ) =>
        new(
            contentDbContext: contentDbContext,
            appOptions: appOptions ?? CreateAppOptions(AppEnvironment.Prod),
            releasePublishingStatusRepository: releasePublishingStatusRepository
                ?? Mock.Of<IReleasePublishingStatusRepository>(MockBehavior.Strict),
            userService: MockUtils.AlwaysTrueUserService().Object
        );
}
