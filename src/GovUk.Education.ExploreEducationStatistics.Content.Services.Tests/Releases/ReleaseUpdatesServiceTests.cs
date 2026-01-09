using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Releases;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Tests.Releases;

public abstract class ReleaseUpdatesServiceTests
{
    private readonly DataFixture _dataFixture = new();

    public class GetReleaseUpdatesTests : ReleaseUpdatesServiceTests
    {
        [Theory]
        [InlineData(0, 1, 10)]
        [InlineData(1, 1, 10)]
        [InlineData(5, 1, 10)]
        [InlineData(15, 1, 10)]
        [InlineData(15, 2, 5)]
        public async Task WhenPublicationAndReleaseExist_ReturnsPaginatedUpdates(int numUpdates, int page, int pageSize)
        {
            // Arrange
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithReleases(_ => [_dataFixture.DefaultRelease(publishedVersions: 1)]);
            var release = publication.Releases[0];
            var releaseVersion = release.Versions[0];

            releaseVersion.Updates = _dataFixture
                .DefaultUpdate()
                .WithReleaseVersionId(releaseVersion.Id)
                .GenerateList(numUpdates);

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
                var outcome = await sut.GetReleaseUpdates(
                    publicationSlug: publication.Slug,
                    releaseSlug: release.Slug,
                    page: page,
                    pageSize: pageSize
                );

                // Assert
                var pagedResult = outcome.AssertRight();

                // Total results should include an extra update for the 'First published' entry
                var expectedTotalResults = numUpdates + 1;

                pagedResult.AssertHasExpectedPagingAndResultCount(
                    expectedTotalResults: expectedTotalResults,
                    expectedPage: page,
                    expectedPageSize: pageSize
                );
            }
        }

        [Fact]
        public async Task WhenUpdatesExist_MapsUpdatesCorrectly()
        {
            // Arrange
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithReleases(_ => [_dataFixture.DefaultRelease(publishedVersions: 1)]);
            var release = publication.Releases[0];
            var releaseVersion = release.Versions[0];

            releaseVersion.Updates = _dataFixture
                .DefaultUpdate()
                .WithReleaseVersionId(releaseVersion.Id)
                .WithOn(releaseVersion.Published!.Value.AddDays(1).UtcDateTime)
                .GenerateList(1);

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
                var outcome = await sut.GetReleaseUpdates(publicationSlug: publication.Slug, releaseSlug: release.Slug);

                // Assert
                var pagedResult = outcome.AssertRight();

                // Total results should include an extra update for the 'First published' entry
                var expectedTotalResults = releaseVersion.Updates.Count + 1;

                pagedResult.AssertHasExpectedPagingAndResultCount(
                    expectedTotalResults: expectedTotalResults,
                    expectedPage: 1,
                    expectedPageSize: 10
                );

                var results = pagedResult.Results;

                Assert.Equal(releaseVersion.Updates[0].On, results[0].Date);
                Assert.Equal(releaseVersion.Updates[0].Reason, results[0].Summary);

                // Check that the last item is the 'First published' entry
                Assert.Equal(releaseVersion.Published, results[^1].Date);
                Assert.Equal("First published", results[^1].Summary);
            }
        }

        [Fact]
        public async Task WhenMultipleUpdatesExist_ReturnsUpdatesInDescendingDateOrder()
        {
            // Arrange
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithReleases(_ => [_dataFixture.DefaultRelease(publishedVersions: 1)]);
            var release = publication.Releases[0];
            var releaseVersion = release.Versions[0];

            // Set a shuffled list of updates dated after the release version published date
            releaseVersion.Updates =
            [
                .. Enumerable
                    .Range(0, 9)
                    .Select(i =>
                        _dataFixture
                            .DefaultUpdate()
                            .WithReleaseVersionId(releaseVersion.Id)
                            .WithOn(releaseVersion.Published!.Value.AddDays(i + 1).UtcDateTime)
                            .Generate()
                    )
                    .ToArray()
                    .Shuffle(),
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
                var outcome = await sut.GetReleaseUpdates(publicationSlug: publication.Slug, releaseSlug: release.Slug);

                // Assert
                var pagedResult = outcome.AssertRight();

                // Total results should include an extra update for the 'First published' entry
                var expectedTotalResults = releaseVersion.Updates.Count + 1;

                pagedResult.AssertHasExpectedPagingAndResultCount(
                    expectedTotalResults: expectedTotalResults,
                    expectedPage: 1,
                    expectedPageSize: 10
                );

                // Check that the results are in descending date order
                var results = pagedResult.Results;
                for (var i = 0; i < results.Count - 1; i++)
                {
                    Assert.True(results[i].Date > results[i + 1].Date);
                }

                // Check that the last item is the 'First published' entry
                Assert.Equal(releaseVersion.Published, results[^1].Date);
                Assert.Equal("First published", results[^1].Summary);
            }
        }

        [Fact]
        public async Task WhenUpdatesExistBeforeReleasePublishedDate_ReturnsUpdatesInDescendingDateOrder()
        {
            // Arrange
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithReleases(_ => [_dataFixture.DefaultRelease(publishedVersions: 1)]);
            var release = publication.Releases[0];
            var releaseVersion = release.Versions[0];

            // There are usually no updates before the first published date, but if they do exist,
            // ensure all updates including the 'First published' entry are correctly ordered.
            var (updateBeforeFirstPublished, updateAfterFirstPublished) = _dataFixture
                .DefaultUpdate()
                .WithReleaseVersionId(releaseVersion.Id)
                .ForIndex(0, setters => setters.SetOn(releaseVersion.Published!.Value.AddDays(-1).UtcDateTime))
                .ForIndex(1, setters => setters.SetOn(releaseVersion.Published!.Value.AddDays(1).UtcDateTime))
                .GenerateTuple2();
            releaseVersion.Updates = [updateBeforeFirstPublished, updateAfterFirstPublished];

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
                var outcome = await sut.GetReleaseUpdates(publicationSlug: publication.Slug, releaseSlug: release.Slug);

                // Assert
                var pagedResult = outcome.AssertRight();

                pagedResult.AssertHasExpectedPagingAndResultCount(
                    expectedTotalResults: 3,
                    expectedPage: 1,
                    expectedPageSize: 10
                );

                var results = pagedResult.Results;

                Assert.Equal(updateAfterFirstPublished.On, results[0].Date);
                Assert.Equal(updateAfterFirstPublished.Reason, results[0].Summary);
                Assert.Equal(releaseVersion.Published!.Value, results[1].Date);
                Assert.Equal("First published", results[1].Summary);
                Assert.Equal(updateBeforeFirstPublished.On, results[2].Date);
                Assert.Equal(updateBeforeFirstPublished.Reason, results[2].Summary);
            }
        }

        [Fact]
        public async Task WhenNoUpdatesExist_ReturnsOnlyFirstPublishedEntry()
        {
            // Arrange
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithReleases(_ => [_dataFixture.DefaultRelease(publishedVersions: 1)]);
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
                var outcome = await sut.GetReleaseUpdates(publicationSlug: publication.Slug, releaseSlug: release.Slug);

                // Assert
                var pagedResult = outcome.AssertRight();

                pagedResult.AssertHasExpectedPagingAndResultCount(
                    expectedTotalResults: 1,
                    expectedPage: 1,
                    expectedPageSize: 10
                );

                Assert.Equal(release.Versions[0].Published, pagedResult.Results[0].Date);
                Assert.Equal("First published", pagedResult.Results[0].Summary);
            }
        }

        [Fact]
        public async Task WhenReleaseHasMultiplePublishedVersions_ReturnsFirstPublishedEntryUsingFirstPublishedVersion()
        {
            // Arrange
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithReleases(_ => [_dataFixture.DefaultRelease(publishedVersions: 2)]);
            var release = publication.Releases[0];

            // Ensure the generated release versions have different published dates
            Assert.True(
                release.Versions[0].Published < release.Versions[1].Published,
                "The first version should have an earlier published date than the second version"
            );

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
                var outcome = await sut.GetReleaseUpdates(publicationSlug: publication.Slug, releaseSlug: release.Slug);

                // Assert
                var pagedResult = outcome.AssertRight();

                pagedResult.AssertHasExpectedPagingAndResultCount(
                    expectedTotalResults: 1,
                    expectedPage: 1,
                    expectedPageSize: 10
                );

                Assert.Equal(release.Versions[0].Published, pagedResult.Results[0].Date);
                Assert.Equal("First published", pagedResult.Results[0].Summary);
            }
        }

        [Fact]
        public async Task WhenPublicationDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            const string publicationSlug = "publication-that-does-not-exist";
            const string releaseSlug = "test-release";

            await using var context = InMemoryContentDbContext();
            var sut = BuildService(context);

            // Act
            var outcome = await sut.GetReleaseUpdates(publicationSlug: publicationSlug, releaseSlug: releaseSlug);

            // Assert
            outcome.AssertNotFound();
        }

        [Fact]
        public async Task WhenReleaseDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            Publication publication = _dataFixture.DefaultPublication();
            const string releaseSlug = "release-that-does-not-exist";

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
                var outcome = await sut.GetReleaseUpdates(publicationSlug: publication.Slug, releaseSlug: releaseSlug);

                // Assert
                outcome.AssertNotFound();
            }
        }

        [Fact]
        public async Task WhenReleaseHasNoPublishedVersion_ReturnsNotFound()
        {
            // Arrange
            Publication publication = _dataFixture
                .DefaultPublication()
                .WithReleases(_ => [_dataFixture.DefaultRelease(publishedVersions: 0, draftVersion: true)]);
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
                var outcome = await sut.GetReleaseUpdates(publicationSlug: publication.Slug, releaseSlug: release.Slug);

                // Assert
                outcome.AssertNotFound();
            }
        }
    }

    private static ReleaseUpdatesService BuildService(ContentDbContext contentDbContext) => new(contentDbContext);
}
