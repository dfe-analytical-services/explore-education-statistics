#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Options;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.ReleaseVersionsMigration;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
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

    public class MigrateReleaseVersionsPublishedDateRelevanceTests : ReleaseVersionsMigrationServiceTests
    {
        [Fact]
        public async Task WhenReleaseVersionIsFirstVersion_ReleaseVersionIsExcludedFromMigration()
        {
            // Arrange
            var published = new DateTimeOffset(2026, 1, 1, 9, 30, 0, TimeSpan.Zero);
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
                                        .WithPublished(published)
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

                Assert.Equal(published, actualReleaseVersion.Published);
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
            var published = new DateTimeOffset(2026, 1, 1, 9, 30, 0, TimeSpan.Zero);
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
                                        .WithPublished(published)
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

                Assert.Equal(published, actualReleaseVersion.Published);
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
