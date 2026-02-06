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

    private static readonly DateTimeOffset DefaultPublished = new(2026, 1, 1, 9, 30, 0, TimeSpan.Zero);
    private static readonly DateTimeOffset DefaultPublishedScheduled = DefaultPublished.GetUkStartOfDayUtc();

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
                                        .WithUpdates(_dataFixture.DefaultUpdate().GenerateList(1)),
                                ]
                            ),
                    ]
                );
            var releaseVersionId = publication.Releases.Single().Versions.Single(rv => rv.Version == 1).Id;

            var releasePublishingStatusRepository = new Mock<IReleasePublishingStatusRepository>(MockBehavior.Strict);
            releasePublishingStatusRepository.SetupGetAllByOverallStageReturnsNoResults(
                releaseVersionId,
                ReleasePublishingStatusOverallStage.Complete
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

                Assert.True(releaseVersionReport.Warnings.NoSuccessfulPublishingAttempts);

                // This also causes the 'ProposedPublishedDateIsNotSimilarToScheduledTriggerDate' warning to be true,
                // as the proposed published date is based on the latest successful publishing attempt,
                // and there isn't one.
                Assert.True(releaseVersionReport.Warnings.ProposedPublishedDateIsNotSimilarToScheduledTriggerDate);

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
                                        .WithUpdates(_dataFixture.DefaultUpdate().GenerateList(1)),
                                ]
                            ),
                    ]
                );
            var releaseVersionId = publication.Releases.Single().Versions.Single(rv => rv.Version == 1).Id;

            var releasePublishingStatusRepository = new Mock<IReleasePublishingStatusRepository>(MockBehavior.Strict);
            releasePublishingStatusRepository
                .Setup(r => r.GetAllByOverallStage(releaseVersionId, ReleasePublishingStatusOverallStage.Complete))
                .ReturnsAsync([
                    new ReleasePublishingStatus { Created = DateTime.UtcNow, Timestamp = DateTimeOffset.UtcNow },
                    new ReleasePublishingStatus { Created = DateTime.UtcNow, Timestamp = DateTimeOffset.UtcNow },
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
                                        .WithUpdates(_dataFixture.DefaultUpdate().GenerateList(updatesCount)),
                                ]
                            ),
                    ]
                );
            var releaseVersionId = publication.Releases.Single().Versions.Single().Id;

            var releasePublishingStatusRepository = new Mock<IReleasePublishingStatusRepository>(MockBehavior.Strict);
            releasePublishingStatusRepository.SetupGetAllByOverallStageReturnsSingleResult(
                releaseVersionId,
                ReleasePublishingStatusOverallStage.Complete
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
                }
                else
                {
                    Assert.Null(releaseVersionReport.Warnings.UpdatesCountDoesNotMatchVersion);
                }

                releasePublishingStatusRepository.VerifyAll();
            }
        }

        [Theory]
        [InlineData("2025-01-01T09:30:25 +00:00", "2025-01-01T00:00:00", false)] // Date-only elements match (1st Jan)
        [InlineData("2025-01-01T09:30:25 +00:00", "2025-01-01T07:00:00", false)] // Date-only elements match (1st Jan)
        [InlineData("2025-01-01T09:30:25 +00:00", "2025-01-01T16:00:00", false)] // Date-only elements match (1st Jan)
        [InlineData("2025-01-01T09:30:25 +00:00", "2024-12-31T07:00:00", true)] // Date-only elements mismatch (1st Jan vs 31st Dec)
        [InlineData("2025-01-01T09:30:25 +00:00", "2024-12-31T16:00:00", true)] // Date-only elements mismatch (1st Jan vs 31st Dec)
        [InlineData("2025-01-01T09:30:25 +00:00", "2025-01-02T07:00:00", true)] // Date-only elements mismatch (1st Jan vs 2nd Jan)
        [InlineData("2025-01-01T09:30:25 +00:00", "2025-01-02T16:00:00", true)] // Date-only elements mismatch (1st Jan vs 2nd Jan)
        [InlineData("2025-01-06T08:30:25 +00:00", "2025-05-30T23:00:00", false)] // Date-only elements match (1st June when converted to UK local time)
        public async Task WhenProposedPublishedDateDoesNotMatchLatestUpdateNoteDate_ReportIncludesWarning(
            string latestAttemptTimestampString,
            string updateNoteOnString,
            bool expectedWarning
        )
        {
            // Arrange
            var latestAttemptTimestamp = DateTimeOffset.Parse(latestAttemptTimestampString);
            var updateNoteOn = DateTime.Parse(updateNoteOnString);
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
                                        .WithUpdates(_dataFixture.DefaultUpdate().WithOn(updateNoteOn).GenerateList(1)),
                                ]
                            ),
                    ]
                );
            var releaseVersionId = publication.Releases.Single().Versions.Single(rv => rv.Version == 1).Id;

            var releasePublishingStatusRepository = new Mock<IReleasePublishingStatusRepository>(MockBehavior.Strict);
            releasePublishingStatusRepository.SetupGetAllByOverallStageReturnsSingleResult(
                releaseVersionId,
                ReleasePublishingStatusOverallStage.Complete,
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
                }
                else
                {
                    Assert.Null(releaseVersionReport.Warnings.ProposedPublishedDateDoesNotMatchLatestUpdateNoteDate);
                }

                releasePublishingStatusRepository.VerifyAll();
            }
        }

        [Fact]
        public async Task WhenProposedPublishedDateIsNotSimilarToScheduledTriggerDate_ReportIncludesWarning()
        {
            // EES-6830 TODO need to test permutations of Immediate/Scheduled and for Scheduled, different environments.
            Assert.Fail();
        }

        [Theory]
        [InlineData("2025-01-01T00:00:00 +00:00", false)] // 00:00 UTC on 1st Jan is also 00:00 UK local time (GMT)
        [InlineData("2025-01-01T23:00:00 +00:00", true)] // 23:00 UTC on 1st Jan is also 23:00 UK local time (GMT)
        [InlineData("2025-03-30T00:00:00 +00:00", false)] // 00:00 UTC on 30th Mar is one hour before BST starts and 00:00 UK local time (GMT)
        [InlineData("2025-06-01T00:00:00 +00:00", true)] // 00:00 UTC on 1st June is 01:00 UK local time (BST)
        [InlineData("2025-06-01T23:00:00 +00:00", false)] // 23:00 UTC on 1st June is 00:00 UK local time (BST)
        [InlineData("2025-10-26T00:00:00 +00:00", true)] // 00:00 UTC on 26th Oct is one hour before BST ends and 01:00 UK local time (BST)
        [InlineData("2025-10-26T01:00:00 +00:00", true)] // 01:00 UTC on 26th Oct is the instant BST ends and 01:00 UK local time (GMT)
        public async Task WhenScheduledTriggerDateIsNotMidnightUkLocalTime(
            string publishedScheduledString,
            bool expectedWarning
        )
        {
            // Arrange
            var publishScheduled = DateTimeOffset.Parse(publishedScheduledString);
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
                                        .WithUpdates(_dataFixture.DefaultUpdate().GenerateList(1)),
                                ]
                            ),
                    ]
                );
            var releaseVersionId = publication.Releases.Single().Versions.Single(rv => rv.Version == 1).Id;

            var releasePublishingStatusRepository = new Mock<IReleasePublishingStatusRepository>(MockBehavior.Strict);
            releasePublishingStatusRepository.SetupGetAllByOverallStageReturnsSingleResult(
                releaseVersionId,
                ReleasePublishingStatusOverallStage.Complete
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
                }
                else
                {
                    Assert.Null(releaseVersionReport.Warnings.ScheduledTriggerDateIsNotMidnightUkLocalTime);
                }

                releasePublishingStatusRepository.VerifyAll();
            }
        }
    }

    private static ReleaseVersionsMigrationReportReleaseVersionDto GetReleaseVersionReportFromReport(
        ReleaseVersionsMigrationReportDto report,
        Guid releaseVersionId
    ) =>
        report
            .Publications.Single()
            .Releases.Single()
            .ReleaseVersions.Single(rv => rv.ReleaseVersionId == releaseVersionId);

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
