#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services.KeyStatisticsMigration;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.KeyStatisticsMigration;

public abstract class KeyStatisticsMigrationServiceTests
{
    private readonly DataFixture _dataFixture = new();

    public class MigrateKeyStatisticsGuidanceText : KeyStatisticsMigrationServiceTests
    {
        [Theory]
        // A sample of Markdown cases (not exhaustive as Markdig is expected to convert Markdown to plain text correctly).
        [InlineData("# Level 1 heading\nSome text", "Level 1 heading\nSome text")]
        [InlineData("## Level 2 heading\nSome text", "Level 2 heading\nSome text")]
        [InlineData("### Level 3 heading\nSome text", "Level 3 heading\nSome text")]
        [InlineData("Some *italic text*", "Some italic text")]
        [InlineData("Some _italic text_", "Some italic text")]
        [InlineData("Some **bold text**", "Some bold text")]
        [InlineData("Some __bold text__", "Some bold text")]
        [InlineData("Inline `code` fragment", "Inline code fragment")]
        [InlineData("```\ncode block\n```", "code block")]
        [InlineData("> Single line blockquote", "Single line blockquote")]
        [InlineData("> Multi-line\n> blockquote", "Multi-line\nblockquote")]
        [InlineData("> Outer blockquote\n> > Inner blockquote", "Outer blockquote\nInner blockquote")]
        [InlineData("- List item", "List item")]
        [InlineData("- List item\n- Another item", "List item\nAnother item")]
        [InlineData("* List item", "List item")]
        [InlineData("* List item\n* Another item", "List item\nAnother item")]
        [InlineData("1. Numbered list item", "Numbered list item")]
        [InlineData("1. First item\n2. Second item\n3. Third item", "First item\nSecond item\nThird item")]
        [InlineData("Link to [example](https://example.com)", "Link to example")]
        [InlineData("Image ![alt text](https://example.com/image.png)", "Image alt text")]
        [InlineData("Horizontal rule\n\n---\n\nSome text", "Horizontal rule\nSome text")]
        [InlineData("Horizontal rule\n\n***\n\nSome text", "Horizontal rule\nSome text")]
        [InlineData("Text with a line break  \nNew line", "Text with a line break\nNew line")]
        public async Task WhenGuidanceTextContainsMarkdown_MarkdownIsConvertedToPlainText(
            string originalText,
            string plainText
        )
        {
            // Arrange
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithTheme(_dataFixture.DefaultTheme())
                .WithReleases(_ => [_dataFixture.DefaultRelease(publishedVersions: 1, draftVersion: false)]);
            var release = publication.Releases[0];
            var releaseVersion = release.Versions[0];

            releaseVersion.KeyStatistics = [_dataFixture.DefaultKeyStatisticText().WithGuidanceText(originalText)];

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
                var outcome = await sut.MigrateKeyStatisticsGuidanceText(dryRun: false);

                // Assert - Verify the key statistic is present in the migration results in the report
                var report = outcome.AssertRight();

                var migrationResult = Assert.Single(report.MigrationResults);
                Assert.Equal(1, report.UpdatedKeyStatisticsCount);

                Assert.Equal(releaseVersion.KeyStatistics[0].Id, migrationResult.KeyStatisticId);
                Assert.Equal(originalText, migrationResult.GuidanceTextOriginal);
                Assert.Equal(plainText, migrationResult.GuidanceTextPlain);
            }

            // Assert - Verify the database state was updated
            await using (var context = InMemoryContentDbContext(contextId))
            {
                var keyStatistics = context
                    .KeyStatistics.Where(ks => ks.ReleaseVersionId == releaseVersion.Id)
                    .ToList();

                var keyStatistic = Assert.Single(keyStatistics);
                Assert.Equal(plainText, keyStatistic.GuidanceText);
            }
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("   ")]
        public async Task WhenGuidanceTextIsNullOrEmpty_GuidanceTextIsUnchanged(string? guidanceText)
        {
            // Arrange
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithTheme(_dataFixture.DefaultTheme())
                .WithReleases(_ => [_dataFixture.DefaultRelease(publishedVersions: 1, draftVersion: false)]);
            var release = publication.Releases[0];
            var releaseVersion = release.Versions[0];

            releaseVersion.KeyStatistics = [_dataFixture.DefaultKeyStatisticText().WithGuidanceText(guidanceText)];

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
                var outcome = await sut.MigrateKeyStatisticsGuidanceText(dryRun: false);

                // Assert - Verify the key statistic is absent from the migration results in the report
                var report = outcome.AssertRight();

                Assert.Empty(report.MigrationResults);
                Assert.Equal(0, report.UpdatedKeyStatisticsCount);
            }

            // Assert - Verify the database state was not updated
            await using (var context = InMemoryContentDbContext(contextId))
            {
                var keyStatistics = context
                    .KeyStatistics.Where(ks => ks.ReleaseVersionId == releaseVersion.Id)
                    .ToList();

                var keyStatistic = Assert.Single(keyStatistics);
                Assert.Equal(guidanceText, keyStatistic.GuidanceText);
            }
        }

        [Theory]
        // A sample of plain text cases (not exhaustive as Markdig is expected to leave text unchanged where no valid Markdown applies).
        [InlineData("Plain text.")]
        [InlineData("Some --- inline dashes")]
        [InlineData("#Not a level 1 heading")]
        [InlineData("##Not a level 2 heading")]
        [InlineData("###Not a level 3 heading")]
        [InlineData("-Not a list item")]
        [InlineData("Unclosed inline `code fragment")]
        [InlineData("Unclosed inline code fragment`")]
        [InlineData("*Unclosed pair of asterisks")]
        [InlineData("Unclosed pair of asterisks*")]
        [InlineData("Unclosed pair* of asterisks")]
        [InlineData("**Unclosed pair of asterisks")]
        [InlineData("Unclosed pair of asterisks**")]
        [InlineData("Unclosed pair** of asterisks")]
        [InlineData("_Unclosed pair of underscores")]
        [InlineData("Unclosed pair of underscores_")]
        [InlineData("Unclosed pair_ of underscores")]
        [InlineData("__Unclosed pair of underscores")]
        [InlineData("Unclosed pair of underscores__")]
        [InlineData("Unclosed pair__ of underscores")]
        [InlineData("snake_case_variables")]
        [InlineData("Plain urls https://example.com")]
        public async Task WhenGuidanceTextDoesNotContainMarkdown_GuidanceTextIsUnchanged(string guidanceText)
        {
            // Arrange
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithTheme(_dataFixture.DefaultTheme())
                .WithReleases(_ => [_dataFixture.DefaultRelease(publishedVersions: 1, draftVersion: false)]);
            var release = publication.Releases[0];
            var releaseVersion = release.Versions[0];

            releaseVersion.KeyStatistics = [_dataFixture.DefaultKeyStatisticText().WithGuidanceText(guidanceText)];

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
                var outcome = await sut.MigrateKeyStatisticsGuidanceText(dryRun: false);

                // Assert - Verify the key statistic is absent from the migration results in the report
                var report = outcome.AssertRight();

                Assert.Empty(report.MigrationResults);
                Assert.Equal(0, report.UpdatedKeyStatisticsCount);
            }

            // Assert - Verify the database state was not updated
            await using (var context = InMemoryContentDbContext(contextId))
            {
                var keyStatistics = context
                    .KeyStatistics.Where(ks => ks.ReleaseVersionId == releaseVersion.Id)
                    .ToList();

                var keyStatistic = Assert.Single(keyStatistics);
                Assert.Equal(guidanceText, keyStatistic.GuidanceText);
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task WhenReleaseHasSingleDraftOrPublishedVersion_ReleaseVersionIsIncludedInReportAndKeyStatisticsAreUpdated(
            bool isDraftVersion
        )
        {
            // Arrange
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithTheme(_dataFixture.DefaultTheme())
                .WithReleases(_ =>
                    [
                        _dataFixture.DefaultRelease(
                            publishedVersions: isDraftVersion ? 0 : 1,
                            draftVersion: isDraftVersion
                        ),
                    ]
                );
            var release = publication.Releases[0];
            var releaseVersion = release.Versions[0];

            releaseVersion.KeyStatistics =
            [
                _dataFixture.DefaultKeyStatisticText().WithGuidanceText("**Key** statistic"),
            ];

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
                var outcome = await sut.MigrateKeyStatisticsGuidanceText(dryRun: false);

                // Assert
                var report = outcome.AssertRight();
                Assert.Equal(1, report.UpdatedKeyStatisticsCount);

                var reportPublication = Assert.Single(report.Publications);
                var reportRelease = Assert.Single(reportPublication.Releases);
                var reportReleaseVersion = Assert.Single(reportRelease.ReleaseVersions);
                Assert.Equal(releaseVersion.Id, reportReleaseVersion.ReleaseVersionId);
                Assert.Equal(isDraftVersion, reportReleaseVersion.IsDraft);
            }
        }

        [Fact]
        public async Task WhenReleaseHasMultiplePublishedVersions_KeyStatisticsAreUpdatedOnlyForLatestPublishedVersion()
        {
            // Arrange
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithTheme(_dataFixture.DefaultTheme())
                .WithReleases(_ => [_dataFixture.DefaultRelease(publishedVersions: 2, draftVersion: false)]);
            var release = publication.Releases[0];
            var earlierPublishedVersion = release.Versions[0];
            var latestPublishedVersion = release.Versions[1];

            earlierPublishedVersion.KeyStatistics =
            [
                _dataFixture.DefaultKeyStatisticText().WithGuidanceText("**Key** statistic should not be updated"),
            ];

            latestPublishedVersion.KeyStatistics =
            [
                _dataFixture.DefaultKeyStatisticText().WithGuidanceText("**Key** statistic should be updated"),
            ];

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
                var outcome = await sut.MigrateKeyStatisticsGuidanceText(dryRun: false);

                // Assert
                var report = outcome.AssertRight();

                var migrationResult = Assert.Single(report.MigrationResults);
                Assert.Equal(1, report.UpdatedKeyStatisticsCount);

                Assert.Equal(latestPublishedVersion.KeyStatistics[0].Id, migrationResult.KeyStatisticId);
                Assert.Equal("**Key** statistic should be updated", migrationResult.GuidanceTextOriginal);
                Assert.Equal("Key statistic should be updated", migrationResult.GuidanceTextPlain);

                var reportPublication = Assert.Single(report.Publications);
                var reportRelease = Assert.Single(reportPublication.Releases);
                var reportReleaseVersion = Assert.Single(reportRelease.ReleaseVersions);
                Assert.Equal(latestPublishedVersion.Id, reportReleaseVersion.ReleaseVersionId);
            }
        }

        [Fact]
        public async Task WhenReleaseHasMultiplePublishedVersionsAndDraftVersion_KeyStatisticsAreUpdatedForLatestPublishedAndDraftVersions()
        {
            // Arrange
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithTheme(_dataFixture.DefaultTheme())
                .WithReleases(_ => [_dataFixture.DefaultRelease(publishedVersions: 2, draftVersion: true)]);
            var release = publication.Releases[0];
            var earlierPublishedVersion = release.Versions[0];
            var latestPublishedVersion = release.Versions[1];
            var draftVersion = release.Versions[2];

            earlierPublishedVersion.KeyStatistics =
            [
                _dataFixture.DefaultKeyStatisticText().WithGuidanceText("**Key** statistic should not be updated"),
            ];

            latestPublishedVersion.KeyStatistics =
            [
                _dataFixture.DefaultKeyStatisticText().WithGuidanceText("**Key** statistic should be updated"),
            ];

            draftVersion.KeyStatistics =
            [
                _dataFixture.DefaultKeyStatisticText().WithGuidanceText("**Key** statistic in draft should be updated"),
            ];

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
                var outcome = await sut.MigrateKeyStatisticsGuidanceText(dryRun: false);

                // Assert
                var report = outcome.AssertRight();

                Assert.Equal(2, report.UpdatedKeyStatisticsCount);
                Assert.Equal(2, report.MigrationResults.Count);

                Assert.Equal(latestPublishedVersion.KeyStatistics[0].Id, report.MigrationResults[0].KeyStatisticId);
                Assert.Equal("**Key** statistic should be updated", report.MigrationResults[0].GuidanceTextOriginal);
                Assert.Equal("Key statistic should be updated", report.MigrationResults[0].GuidanceTextPlain);

                var latestPublishedVersionMigrationResult = report.MigrationResults.Single(ks =>
                    ks.KeyStatisticId == latestPublishedVersion.KeyStatistics[0].Id
                );
                Assert.Equal(
                    "**Key** statistic should be updated",
                    latestPublishedVersionMigrationResult.GuidanceTextOriginal
                );
                Assert.Equal(
                    "Key statistic should be updated",
                    latestPublishedVersionMigrationResult.GuidanceTextPlain
                );

                var reportPublication = Assert.Single(report.Publications);
                var reportRelease = Assert.Single(reportPublication.Releases);
                Assert.Equal(2, reportRelease.ReleaseVersions.Count);

                Assert.Equal(latestPublishedVersion.Id, reportRelease.ReleaseVersions[0].ReleaseVersionId);
                Assert.False(reportRelease.ReleaseVersions[1].IsDraft);

                Assert.Equal(draftVersion.Id, reportRelease.ReleaseVersions[1].ReleaseVersionId);
                Assert.True(reportRelease.ReleaseVersions[1].IsDraft);
            }
        }

        [Fact]
        public async Task WhenPublicationHasNoKeyStatistics_PublicationIsExcludedFromReportPublications()
        {
            // Arrange
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithTheme(_dataFixture.DefaultTheme())
                .WithReleases(_ => [_dataFixture.DefaultRelease(publishedVersions: 1, draftVersion: false)]);

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
                var outcome = await sut.MigrateKeyStatisticsGuidanceText(dryRun: false);

                // Assert
                var report = outcome.AssertRight();
                Assert.Empty(report.Publications);
            }
        }

        [Fact]
        public async Task WhenReleaseHasNoKeyStatistics_ReleaseIsExcludedFromReportPublications()
        {
            // Arrange
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithTheme(_dataFixture.DefaultTheme())
                .WithReleases(_ =>
                    [
                        _dataFixture.DefaultRelease(publishedVersions: 1, year: 2024),
                        _dataFixture.DefaultRelease(publishedVersions: 1, year: 2025),
                    ]
                );

            // Add key statistics to only one release
            var releaseWithKeyStatistics = publication.Releases.Single(r => r.Year == 2025);
            releaseWithKeyStatistics.Versions[0].KeyStatistics =
            [
                _dataFixture.DefaultKeyStatisticText().WithGuidanceText("Key statistic"),
            ];

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
                var outcome = await sut.MigrateKeyStatisticsGuidanceText(dryRun: false);

                // Assert
                var report = outcome.AssertRight();

                // Verify only the release with key statistics is included in the report
                var reportPublication = Assert.Single(report.Publications);
                var reportRelease = Assert.Single(reportPublication.Releases);
                Assert.Equal(releaseWithKeyStatistics.Id, reportRelease.ReleaseId);
            }
        }

        [Fact]
        public async Task WhenDryRunIsTrue_DoesNotUpdateKeyStatistics()
        {
            // Arrange
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithTheme(_dataFixture.DefaultTheme())
                .WithReleases(_ => [_dataFixture.DefaultRelease(publishedVersions: 1, draftVersion: false)]);
            var release = publication.Releases[0];
            var releaseVersion = release.Versions[0];

            releaseVersion.KeyStatistics =
            [
                _dataFixture.DefaultKeyStatisticText().WithGuidanceText("**Key** statistic 1").WithOrder(1),
                _dataFixture.DefaultKeyStatisticDataBlock().WithGuidanceText("**Key** statistic 2").WithOrder(2),
            ];

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
                var outcome = await sut.MigrateKeyStatisticsGuidanceText(dryRun: true);

                // Assert - Verify the outcome
                outcome.AssertRight();
            }

            await using (var context = InMemoryContentDbContext(contextId))
            {
                // Assert - Verify the database state was not updated
                var keyStatistics = context
                    .KeyStatistics.Where(ks => ks.ReleaseVersionId == releaseVersion.Id)
                    .ToList();

                Assert.Equal(2, keyStatistics.Count);
                Assert.Equal("**Key** statistic 1", keyStatistics.Single(ks => ks.Order == 1).GuidanceText);
                Assert.Equal("**Key** statistic 2", keyStatistics.Single(ks => ks.Order == 2).GuidanceText);
            }
        }

        [Fact]
        public async Task ReportIndicatesTotalKeyStatisticsCount()
        {
            // Arrange
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithTheme(_dataFixture.DefaultTheme())
                .WithReleases(_ => [_dataFixture.DefaultRelease(publishedVersions: 1, draftVersion: false)]);
            var release = publication.Releases[0];
            var releaseVersion = release.Versions[0];

            releaseVersion.KeyStatistics =
            [
                _dataFixture.DefaultKeyStatisticText().WithGuidanceText("Key statistic 1"),
                _dataFixture.DefaultKeyStatisticDataBlock().WithGuidanceText("Key statistic 2"),
                _dataFixture.DefaultKeyStatisticText().WithGuidanceText("**Key** statistic 3"),
            ];

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
                var outcome = await sut.MigrateKeyStatisticsGuidanceText(dryRun: false);

                // Assert
                var report = outcome.AssertRight();

                Assert.Equal(3, report.KeyStatisticsCount);
            }
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
            var outcome = await sut.MigrateKeyStatisticsGuidanceText(dryRun: dryRun);

            // Assert
            var report = outcome.AssertRight();

            Assert.Equal(dryRun, report.DryRun);
        }
    }

    private static KeyStatisticsMigrationService BuildService(ContentDbContext contentDbContext) =>
        new(contentDbContext: contentDbContext, userService: MockUtils.AlwaysTrueUserService().Object);
}
