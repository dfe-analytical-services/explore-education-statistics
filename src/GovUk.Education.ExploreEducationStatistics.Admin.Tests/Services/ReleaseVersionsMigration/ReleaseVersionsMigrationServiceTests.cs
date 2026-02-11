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
