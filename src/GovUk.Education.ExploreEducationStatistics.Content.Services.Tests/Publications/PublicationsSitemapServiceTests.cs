using System;
using System.Threading.Tasks;
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
        public async Task GetSitemapItems_ReturnsExpectedSitemapItems()
        {
            Publication publication = _dataFixture.DefaultPublication()
                .WithReleases([
                    _dataFixture.DefaultRelease(publishedVersions: 2), // Two versions to test de-duping
                    _dataFixture.DefaultRelease(publishedVersions: 0, draftVersion: true)
                ])
                .WithUpdated(DateTime.UtcNow.AddDays(-1));

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                contentDbContext.Publications.Add(publication);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = BuildService(contentDbContext);

                var result = await service.GetSitemapItems();

                var publicationItem = Assert.Single(result);
                Assert.Equal(publication.Slug, publicationItem.Slug);
                Assert.Equal(publication.Updated, publicationItem.LastModified);

                Assert.NotNull(publicationItem.Releases);
                var releaseItem = Assert.Single(publicationItem.Releases);

                Assert.Equal(publication.Releases[0].Slug, releaseItem.Slug);
                Assert.Equal(publication.Releases[0].Versions[1].Published, releaseItem.LastModified);
            }
        }

        [Fact]
        public async Task GetSitemapItems_ExcludesSupersededPublications()
        {
            Publication publication = _dataFixture.DefaultPublication()
                .WithReleases([_dataFixture.DefaultRelease(publishedVersions: 1)])
                .WithSupersededBy(_dataFixture
                    .DefaultPublication()
                    .WithReleases([_dataFixture.DefaultRelease(publishedVersions: 1)]));

            var contentDbContextId = Guid.NewGuid().ToString();
            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                contentDbContext.Publications.Add(publication);
                await contentDbContext.SaveChangesAsync();
            }

            await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
            {
                var service = BuildService(contentDbContext);

                var result = await service.GetSitemapItems();

                // The sitemap should only include the superseding publication,
                // not the superseded publication
                var publicationItem = Assert.Single(result);
                Assert.Equal(publication.SupersededBy!.Slug, publicationItem.Slug);
            }
        }

        [Fact]
        public async Task GetSitemapItems_ExcludesPublicationsWithNoPublishedReleases()
        {
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

                var result = await service.GetSitemapItems();

                Assert.Empty(result);
            }
        }

        [Fact]
        public async Task GetSitemapItems_PublicationHasNoReleases()
        {
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

                var result = await service.GetSitemapItems();

                Assert.Empty(result);
            }
        }
    }

    private static PublicationsSitemapService BuildService(ContentDbContext contentDbContext)
    {
        return new PublicationsSitemapService(contentDbContext);
    }
}
