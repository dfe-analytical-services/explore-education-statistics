using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Publications;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Publications.Dtos;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Tests.Publications;

public abstract class PublicationReleaseServiceTests
{
    private readonly DataFixture _dataFixture = new();

    public class GetPublicationReleasesTests : PublicationReleaseServiceTests
    {
        [Theory]
        [InlineData(1, 1, 10)]
        [InlineData(5, 1, 10)]
        [InlineData(15, 1, 10)]
        [InlineData(15, 2, 5)]
        public async Task WhenPublicationExistsWithPublishedReleases_ReturnsPaginatedReleases(
            int numReleases,
            int page,
            int pageSize)
        {
            // Arrange
            Publication publication = _dataFixture.DefaultPublication()
                .WithReleases(_ => _dataFixture.DefaultRelease(publishedVersions: 1)
                    .Generate(numReleases));

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
                var outcome = await sut.GetPublicationReleases(
                    publicationSlug: publication.Slug,
                    page: page,
                    pageSize: pageSize);

                // Assert
                var pagedResult = outcome.AssertRight();

                pagedResult.AssertHasExpectedPagingAndResultCount(
                    expectedTotalResults: numReleases,
                    expectedPage: page,
                    expectedPageSize: pageSize);
            }
        }

        [Fact]
        public async Task WhenPublishedReleasesExist_MapsReleasesCorrectly()
        {
            // Arrange
            Publication publication = _dataFixture.DefaultPublication()
                .WithReleases(_ =>
                [
                    _dataFixture.DefaultRelease(publishedVersions: 1)
                        .WithLabel("Final")
                ]);
            var release = publication.Releases[0];

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
                var outcome = await sut.GetPublicationReleases(
                    publicationSlug: publication.Slug);

                // Assert
                var pagedResult = outcome.AssertRight();

                pagedResult.AssertHasExpectedPagingAndResultCount(
                    expectedTotalResults: 1,
                    expectedPage: 1,
                    expectedPageSize: 10);

                var releaseEntry = Assert.IsType<PublicationReleaseEntryDto>(pagedResult.Results.Single());

                Assert.Multiple(() =>
                {
                    Assert.Equal(release.Id, releaseEntry.ReleaseId);
                    Assert.True(releaseEntry.IsLatestRelease);
                    Assert.Equal(release.Label, releaseEntry.Label);
                    Assert.Equal(release.Versions[0].Published, releaseEntry.LastUpdated);
                    // TODO EES-6414 'Published' should be the published display date
                    Assert.Equal(release.Versions[0].Published, releaseEntry.Published);
                    Assert.Equal(release.Slug, releaseEntry.Slug);
                    Assert.Equal(release.Title, releaseEntry.Title);
                    Assert.Equal(release.TimePeriodCoverage.GetEnumLabel(), releaseEntry.CoverageTitle);
                    Assert.Equal(release.YearTitle, releaseEntry.YearTitle);
                });
            }
        }

        [Fact]
        public async Task WhenLegacyReleasesExist_MapsLegacyReleasesCorrectly()
        {
            // Arrange
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithReleases([_dataFixture.DefaultRelease(publishedVersions: 1)])
                .FinishWith(p => p.ReleaseSeries =
                [
                    ..
                    p.Releases
                        .Select(r => _dataFixture.DefaultReleaseSeriesItem().WithReleaseId(r.Id))
                        .ToArray(),
                    _dataFixture.DefaultLegacyReleaseSeriesItem()
                ]);

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
                var outcome = await sut.GetPublicationReleases(
                    publicationSlug: publication.Slug);

                // Assert
                var pagedResult = outcome.AssertRight();

                pagedResult.AssertHasExpectedPagingAndResultCount(
                    expectedTotalResults: 2,
                    expectedPage: 1,
                    expectedPageSize: 10);

                var legacyReleaseEntry = Assert.IsType<LegacyPublicationReleaseEntryDto>(pagedResult.Results[1]);
                var expectedLegacyReleaseEntry = publication.ReleaseSeries[1];
                Assert.Multiple(() =>
                {
                    Assert.Equal(expectedLegacyReleaseEntry.LegacyLinkDescription, legacyReleaseEntry.Title);
                    Assert.Equal(expectedLegacyReleaseEntry.LegacyLinkUrl, legacyReleaseEntry.Url);
                });
            }
        }

        [Fact]
        public async Task WhenMultiplePublishedAndLegacyReleasesExist_ReturnsReleasesInPublicationOrder()
        {
            // Arrange
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithReleases(_ =>
                [
                    _dataFixture.DefaultRelease(publishedVersions: 1, year: 2025),
                    _dataFixture.DefaultRelease(publishedVersions: 2, year: 2024),
                    _dataFixture.DefaultRelease(publishedVersions: 3, year: 2023)
                ])
                .FinishWith(p => p.ReleaseSeries =
                [
                    // Specify a custom order for the releases, differing from the chronological year order,
                    // and include a legacy release in the middle of the series
                    _dataFixture.DefaultReleaseSeriesItem().WithReleaseId(p.Releases.Single(r => r.Year == 2023).Id),
                    _dataFixture.DefaultReleaseSeriesItem().WithReleaseId(p.Releases.Single(r => r.Year == 2025).Id),
                    _dataFixture.DefaultLegacyReleaseSeriesItem(),
                    _dataFixture.DefaultReleaseSeriesItem().WithReleaseId(p.Releases.Single(r => r.Year == 2024).Id)
                ]);

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
                var outcome = await sut.GetPublicationReleases(
                    publicationSlug: publication.Slug);

                // Assert
                var pagedResult = outcome.AssertRight();

                pagedResult.AssertHasExpectedPagingAndResultCount(
                    expectedTotalResults: 4,
                    expectedPage: 1,
                    expectedPageSize: 10);

                var expectedReleases = new[] { (Year: 2023, Index: 0), (Year: 2025, Index: 1), (Year: 2024, Index: 3) };

                foreach (var (year, index) in expectedReleases)
                {
                    var releaseEntry = Assert.IsType<PublicationReleaseEntryDto>(pagedResult.Results[index]);
                    Assert.Equal(publication.Releases.Single(r => r.Year == year).Id, releaseEntry.ReleaseId);
                }

                var legacyEntry = Assert.IsType<LegacyPublicationReleaseEntryDto>(pagedResult.Results[2]);
                Assert.Multiple(() =>
                {
                    Assert.Equal(publication.ReleaseSeries[2].LegacyLinkDescription, legacyEntry.Title);
                    Assert.Equal(publication.ReleaseSeries[2].LegacyLinkUrl, legacyEntry.Url);
                });
            }
        }

        [Fact]
        public async Task WhenPublicationHasMultiplePublishedReleases_OnlyFirstIsMarkedAsLatest()
        {
            // Arrange
            Publication publication = _dataFixture.DefaultPublication()
                .WithReleases(_ =>
                [
                    _dataFixture.DefaultRelease(publishedVersions: 1, year: 2025),
                    _dataFixture.DefaultRelease(publishedVersions: 2, year: 2024),
                    _dataFixture.DefaultRelease(publishedVersions: 3, year: 2023)
                ]);

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
                var outcome = await sut.GetPublicationReleases(publication.Slug);

                // Assert
                var pagedResult = outcome.AssertRight();

                pagedResult.AssertHasExpectedPagingAndResultCount(
                    expectedTotalResults: 3,
                    expectedPage: 1,
                    expectedPageSize: 10);

                var releaseEntries = pagedResult.Results
                    .Select(Assert.IsType<PublicationReleaseEntryDto>)
                    .ToList();

                var expectedLatestRelease = publication.Releases.Single(r => r.Year == 2025);
                Assert.Equal(expectedLatestRelease.Id, releaseEntries[0].ReleaseId);
                Assert.True(releaseEntries[0].IsLatestRelease);
                Assert.All(releaseEntries[1..], r => Assert.False(r.IsLatestRelease));
            }
        }

        [Fact]
        public async Task WhenReleaseHasMultiplePublishedVersions_ReturnsDetailsOfLatestPublishedVersion()
        {
            // Arrange
            Publication publication = _dataFixture.DefaultPublication()
                .WithReleases(_ => [_dataFixture.DefaultRelease(publishedVersions: 2, draftVersion: true)]);
            var release = publication.Releases[0];

            // Ensure the generated release versions have different published dates
            Assert.True(release.Versions[0].Published < release.Versions[1].Published,
                "The first version should have an earlier published date than the second version");

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
                var outcome = await sut.GetPublicationReleases(publication.Slug);

                // Assert
                var pagedResult = outcome.AssertRight();

                pagedResult.AssertHasExpectedPagingAndResultCount(
                    expectedTotalResults: 1,
                    expectedPage: 1,
                    expectedPageSize: 10);

                var releaseEntry = Assert.IsType<PublicationReleaseEntryDto>(pagedResult.Results.Single());
                var expectedReleaseVersion = release.Versions[1];
                Assert.Multiple(() =>
                {
                    Assert.Equal(expectedReleaseVersion.Published, releaseEntry.LastUpdated);
                    // TODO EES-6414 'Published' should be the published display date
                    Assert.Equal(expectedReleaseVersion.Published, releaseEntry.Published);
                });
            }
        }

        [Fact]
        public async Task WhenReleaseHasNoPublishedVersions_ReleaseIsExcludedFromResults()
        {
            // Arrange
            Publication publication = _dataFixture.DefaultPublication()
                .WithReleases([
                    _dataFixture.DefaultRelease(publishedVersions: 0, draftVersion: true, year: 2025),
                    _dataFixture.DefaultRelease(publishedVersions: 1, year: 2024),
                    _dataFixture.DefaultRelease(publishedVersions: 0, draftVersion: true, year: 2023)
                ]);

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
                var outcome = await sut.GetPublicationReleases(publication.Slug);

                // Assert
                var pagedResult = outcome.AssertRight();

                // Only the published release should be in the results
                pagedResult.AssertHasExpectedPagingAndResultCount(
                    expectedTotalResults: 1,
                    expectedPage: 1,
                    expectedPageSize: 10);

                var releaseEntry = Assert.IsType<PublicationReleaseEntryDto>(pagedResult.Results.Single());
                var expectedRelease = publication.Releases.Single(r => r.Year == 2024);
                Assert.Equal(expectedRelease.Id, releaseEntry.ReleaseId);
            }
        }

        [Fact]
        public async Task WhenPublicationDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            const string publicationSlug = "publication-that-does-not-exist";

            await using var context = InMemoryContentDbContext();
            var sut = BuildService(context);

            // Act
            var outcome = await sut.GetPublicationReleases(publicationSlug);

            // Assert
            outcome.AssertNotFound();
        }

        [Fact]
        public async Task WhenPublicationHasNoReleases_ReturnsNotFound()
        {
            // Arrange
            Publication publication = _dataFixture.DefaultPublication();

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
                var outcome = await sut.GetPublicationReleases(publication.Slug);

                // Assert
                outcome.AssertNotFound();
            }
        }

        [Fact]
        public async Task WhenPublicationHasNoPublishedReleases_ReturnsNotFound()
        {
            // Arrange
            Publication publication = _dataFixture.DefaultPublication()
                .WithReleases(_ => [_dataFixture.DefaultRelease(publishedVersions: 0, draftVersion: true)]);

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
                var outcome = await sut.GetPublicationReleases(publication.Slug);

                // Assert
                outcome.AssertNotFound();
            }
        }
    }

    private static PublicationReleasesService BuildService(ContentDbContext contentDbContext) => new(contentDbContext);
}
