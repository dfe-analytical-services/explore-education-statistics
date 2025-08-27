using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Publications;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Tests.Publications;

public abstract class PublicationsSitemapServiceTests
{
    private readonly DataFixture _dataFixture = new();

    public class GetSitemapItemsTests : PublicationsSitemapServiceTests
    {
        [Fact]
        public async Task GetSitemapItems_MultiplePublicationsAndReleases_ReturnsExpectedSitemapItems()
        {
            // Arrange
            var (publication1, publication2) = _dataFixture.DefaultPublication()
                .WithReleases(_ =>
                [
                    _dataFixture.DefaultRelease(publishedVersions: 1),
                    _dataFixture.DefaultRelease(publishedVersions: 1)
                ])
                .GenerateTuple2();
            var (publication1Release1, publication1Release2) = publication1.Releases.ToTuple2();
            var (publication2Release1, publication2Release2) = publication2.Releases.ToTuple2();

            // Set explicit published dates to test ordering items in the sitemap
            // Publication 1 has the most recently published release and should appear first
            // In publication 1, release 2 is published after release 1 and should appear first
            publication1Release1.Versions[0].Published = DateTime.Parse("2025-02-01T09:30:00Z");
            publication1Release2.Versions[0].Published = DateTime.Parse("2025-02-02T09:30:00Z");
            // In publication 2, release 1 is published after release 2 and should appear first
            publication2Release1.Versions[0].Published = DateTime.Parse("2025-01-02T09:30:00Z");
            publication2Release2.Versions[0].Published = DateTime.Parse("2025-01-01T09:30:00Z");

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                contentDbContext.Publications.AddRange(publication1, publication2);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = BuildService(contentDbContext);

                // Act
                var result = await service.GetSitemapItems();

                // Assert
                Assert.Equal(2, result.Length);
                Assert.All(result, publicationItem => Assert.Equal(2, publicationItem.Releases.Length));

                // Publications should be ordered by the latest published release date descending
                // Each publication's releases should be ordered by published date descending
                var publicationItem1 = result[0];
                Assert.Equal(publication1.Slug, publicationItem1.Slug);
                Assert.Equal(publication1Release2.Slug, publicationItem1.Releases[0].Slug);
                Assert.Equal(publication1Release2.Versions[0].Published, publicationItem1.Releases[0].LastModified);
                Assert.Equal(publication1Release1.Slug, publicationItem1.Releases[1].Slug);
                Assert.Equal(publication1Release1.Versions[0].Published, publicationItem1.Releases[1].LastModified);

                var publicationItem2 = result[1];
                Assert.Equal(publication2.Slug, publicationItem2.Slug);
                Assert.Equal(publication2Release1.Slug, publicationItem2.Releases[0].Slug);
                Assert.Equal(publication2Release1.Versions[0].Published, publicationItem2.Releases[0].LastModified);
                Assert.Equal(publication2Release2.Slug, publicationItem2.Releases[1].Slug);
                Assert.Equal(publication2Release2.Versions[0].Published, publicationItem2.Releases[1].LastModified);
            }
        }

        [Fact]
        public async Task GetSitemapItems_PublicationWithUpdatedDate_UpdatedDateIsReflectedInSitemapItem()
        {
            // Arrange
            Publication publication = _dataFixture.DefaultPublication()
                .WithReleases([_dataFixture.DefaultRelease(publishedVersions: 1)])
                .WithUpdated(DateTime.Parse("2025-01-01T12:00:00Z"));

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                contentDbContext.Publications.Add(publication);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = BuildService(contentDbContext);

                // Act
                var result = await service.GetSitemapItems();

                // Assert
                var publicationItem = Assert.Single(result);
                Assert.Equal(publication.Updated, publicationItem.LastModified);
            }
        }

        [Fact]
        public async Task GetSitemapItems_SupersededPublications_SupersedingPublicationHasPublishedRelease_AreExcluded()
        {
            // Arrange
            Publication publication = _dataFixture.DefaultPublication()
                .WithReleases([_dataFixture.DefaultRelease(publishedVersions: 1)])
                .WithSupersededBy(_dataFixture
                    .DefaultPublication()
                    .WithReleases([_dataFixture.DefaultRelease(publishedVersions: 1)]));
            var supersedingPublication = publication.SupersededBy!;

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                contentDbContext.Publications.Add(publication);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = BuildService(contentDbContext);

                // Act
                var result = await service.GetSitemapItems();

                // Assert
                // The sitemap should exclude the superseded publication but include the superseding publication
                var publicationItem = Assert.Single(result);
                Assert.Equal(supersedingPublication.Slug, publicationItem.Slug);
            }
        }

        [Fact]
        public async Task
            GetSitemapItems_SupersededPublications_SupersedingPublicationHasNoPublishedRelease_AreIncluded()
        {
            // Arrange
            Publication publication = _dataFixture.DefaultPublication()
                .WithReleases([_dataFixture.DefaultRelease(publishedVersions: 1)])
                .WithSupersededBy(_dataFixture.DefaultPublication());

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                contentDbContext.Publications.Add(publication);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = BuildService(contentDbContext);

                // Act
                var result = await service.GetSitemapItems();

                // Assert
                // The sitemap should include the superseded publication because the superseding publication
                // has no published releases yet
                var publicationItem = Assert.Single(result);
                Assert.Equal(publication.Slug, publicationItem.Slug);
            }
        }

        [Fact]
        public async Task GetSitemapItems_ReleasesWithMultipleVersions_LatestPublishedVersionIsReturned()
        {
            // Arrange
            Publication publication = _dataFixture.DefaultPublication()
                .WithReleases([_dataFixture.DefaultRelease(publishedVersions: 2, draftVersion: true)]);
            var release = publication.Releases[0];

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                contentDbContext.Publications.Add(publication);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = BuildService(contentDbContext);

                // Act
                var result = await service.GetSitemapItems();

                // Assert
                var publicationItem = Assert.Single(result);
                var releaseItem = Assert.Single(publicationItem.Releases);
                Assert.Equal(release.Slug, releaseItem.Slug);
                // The latest modified date should be from the latest published version
                Assert.Equal(release.Versions[1].Published, releaseItem.LastModified);
            }
        }

        [Fact]
        public async Task GetSitemapItems_PublicationsWithoutPublishedReleases_AreExcluded()
        {
            // Arrange
            Publication publication = _dataFixture.DefaultPublication()
                .WithReleases([_dataFixture.DefaultRelease(publishedVersions: 0, draftVersion: true)]);

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                contentDbContext.Publications.Add(publication);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = BuildService(contentDbContext);

                // Act
                var result = await service.GetSitemapItems();

                // Assert
                Assert.Empty(result);
            }
        }

        [Fact]
        public async Task GetSitemapItems_PublicationsWithoutReleases_AreExcluded()
        {
            // Arrange
            Publication publication = _dataFixture.DefaultPublication();

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                contentDbContext.Publications.Add(publication);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = BuildService(contentDbContext);

                // Act
                var result = await service.GetSitemapItems();

                // Assert
                Assert.Empty(result);
            }
        }
    }

    private static PublicationsSitemapService BuildService(ContentDbContext contentDbContext)
    {
        return new PublicationsSitemapService(contentDbContext);
    }
}
