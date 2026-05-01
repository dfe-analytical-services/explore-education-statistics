using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.ViewModels.EducationInNumbersContentViewModels;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Tests;

public class EducationInNumbersServiceTests
{
    private readonly DataFixture _fixture = new();

    [Fact]
    public async Task ListEinPages_Success()
    {
        var page1Id = Guid.NewGuid();
        var page1PublishedVersionId = Guid.NewGuid();
        var page1 = new EinPage
        {
            Id = page1Id,
            Title = "Page 1",
            Slug = "page-1",
            Order = 2,
            LatestPublishedVersionId = page1PublishedVersionId,
            PageVersions =
            [
                new()
                {
                    Id = Guid.NewGuid(),
                    Version = 0,
                    Published = DateTimeOffset.UtcNow,
                    EinPageId = page1Id,
                },
                new()
                {
                    Id = page1PublishedVersionId,
                    Version = 1,
                    Published = DateTimeOffset.UtcNow,
                    EinPageId = page1Id,
                },
            ],
        };
        var page2Id = Guid.NewGuid();
        var page2PublishedVersionId = Guid.NewGuid();
        var page2 = new EinPage
        {
            Id = page2Id,
            Title = "Page 2",
            Slug = "page-2",
            Order = 1,
            LatestPublishedVersionId = page2PublishedVersionId,
            PageVersions =
            [
                new()
                {
                    Id = page2PublishedVersionId,
                    Version = 1,
                    Published = DateTimeOffset.UtcNow,
                    EinPageId = page2Id,
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Version = 2,
                    Published = null,
                    EinPageId = page2Id,
                },
                new()
                {
                    Id = Guid.NewGuid(),
                    Version = 0,
                    Published = DateTimeOffset.UtcNow,
                    EinPageId = page2Id,
                },
            ],
        };
        var page3Id = Guid.NewGuid();
        var page3PublishedVersionId = Guid.NewGuid();
        var page3 = new EinPage
        {
            Id = page3Id,
            Title = "Page 3",
            Slug = "page-3",
            Order = 0,
            LatestPublishedVersionId = page3PublishedVersionId,
            PageVersions =
            [
                new()
                {
                    Id = page3PublishedVersionId,
                    Version = 0,
                    Published = DateTimeOffset.UtcNow,
                    EinPageId = page3Id,
                },
            ],
        };

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            contentDbContext.EinPages.AddRange(page1, page2, page3);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var service = new EducationInNumbersService(contentDbContext);

            var result = await service.ListEinPages(CancellationToken.None);

            var viewModels = result.AssertRight();
            Assert.Equal(3, viewModels.Count);
            Assert.Equal("Page 3", viewModels[0].Title); // returned in order
            Assert.Equal("Page 2", viewModels[1].Title);
            Assert.Equal("Page 1", viewModels[2].Title);
        }
    }

    [Fact]
    public async Task ListEinPages_ReturnsEmptyListWhenNoPages_Success()
    {
        await using var contentDbContext = InMemoryContentDbContext();
        var service = new EducationInNumbersService(contentDbContext);

        var result = await service.ListEinPages(CancellationToken.None);

        var navItems = result.AssertRight();

        Assert.Empty(navItems);
    }

    [Fact]
    public async Task GetEinPage_ReturnsLatestPage_Success()
    {
        var contentDbContextId = Guid.NewGuid().ToString();
        var pageLatestVersionId = Guid.NewGuid();
        var page = new EinPage
        {
            Id = Guid.NewGuid(),
            Title = "Test Page",
            Slug = "test-page",
            Description = "Description",
            Order = 0,
            LatestPublishedVersionId = pageLatestVersionId,
        };
        var anotherPageLatestVersionId = Guid.NewGuid();
        var anotherPage = new EinPage
        {
            Id = Guid.NewGuid(),
            Title = "Another Page",
            Slug = "another-page",
            Order = 1,
            LatestPublishedVersionId = anotherPageLatestVersionId,
        };
        var pageVersions = new List<EinPageVersion>
        {
            new()
            {
                Id = Guid.NewGuid(),
                EinPageId = page.Id,
                EinPage = page,
                Version = 0,
                Published = new DateTimeOffset(2000, 1, 1, 0, 0, 0, TimeSpan.Zero),
                Content = [],
            },
            new()
            {
                Id = pageLatestVersionId,
                EinPageId = page.Id,
                EinPage = page,
                Version = 1,
                Published = DateTimeOffset.UtcNow,
                Content =
                [
                    new EinContentSection
                    {
                        Id = Guid.NewGuid(),
                        Heading = "Section heading",
                        Order = 0,
                        Content =
                        [
                            new EinHtmlBlock { Order = 1, Body = "EinHtmlBlock 2" },
                            new EinHtmlBlock { Order = 0, Body = "EinHtmlBlock 1" },
                            new EinTileGroupBlock
                            {
                                Title = "tile group block",
                                Order = 2,
                                Tiles =
                                [
                                    new EinFreeTextStatTile
                                    {
                                        Title = "free text stat tile",
                                        Order = 1,
                                        Statistic = "Over 9000!",
                                        Trend = "It's up!",
                                        LinkText = "Link text",
                                        LinkUrl = "https://example.com/",
                                    },
                                    new EinApiQueryStatTile
                                    {
                                        Title = "api query stat tile",
                                        Order = 0,
                                        Statistic = "9393",
                                        DecimalPlaces = 2,
                                        IndicatorUnit = IndicatorUnit.MillionPounds,
                                        DataSetId = Guid.NewGuid(),
                                        DataSetVersionId = Guid.NewGuid(),
                                        LatestDataSetVersionId = Guid.NewGuid(),
                                        Query = "some query",
                                        Version = "1.0.0",
                                        QueryResult = "some query result",
                                        Release = _fixture
                                            .DefaultRelease()
                                            .WithSlug("release-slug")
                                            .WithPublication(
                                                _fixture.DefaultPublication().WithSlug("publication-slug")
                                            ),
                                    },
                                ],
                            },
                        ],
                    },
                ],
            },
            new()
            {
                Id = anotherPageLatestVersionId,
                EinPageId = anotherPage.Id,
                EinPage = anotherPage,
                Published = DateTimeOffset.UtcNow,
                Content = [],
            },
        };
        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            contentDbContext.EinPageVersions.AddRange(pageVersions);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var service = new EducationInNumbersService(contentDbContext);

            var result = await service.GetEinPage("test-page", CancellationToken.None);

            var viewModel = result.AssertRight();
            Assert.Equal(pageVersions[1].Id, viewModel.Id);
            Assert.Equal("Test Page", viewModel.Title);
            Assert.Equal("test-page", viewModel.Slug);
            Assert.Equal("Description", viewModel.Description);
            viewModel.Published.AssertUtcNow();

            var section = Assert.Single(viewModel.Content);
            Assert.Equal("Section heading", section.Heading);
            Assert.Equal(0, section.Order);

            var blocks = section.Content;
            Assert.Equal(3, blocks.Count);

            var block1 = blocks[0];
            var einHtmlBlock1 = Assert.IsType<EinHtmlBlockViewModel>(block1);
            Assert.Equal("EinHtmlBlock 1", einHtmlBlock1.Body);
            Assert.Equal(0, einHtmlBlock1.Order);

            var block2 = blocks[1];
            var einHtmlBlock2 = Assert.IsType<EinHtmlBlockViewModel>(block2);
            Assert.Equal("EinHtmlBlock 2", einHtmlBlock2.Body);
            Assert.Equal(1, einHtmlBlock2.Order);

            var block3 = blocks[2];
            var einTileGroupBlock = Assert.IsType<EinTileGroupBlockViewModel>(block3);
            Assert.Equal("tile group block", einTileGroupBlock.Title);

            Assert.Equal(2, einTileGroupBlock.Tiles.Count);

            var tile1 = Assert.IsType<EinApiQueryStatTileViewModel>(einTileGroupBlock.Tiles[0]);
            Assert.Equal("api query stat tile", tile1.Title);
            Assert.Equal("9393", tile1.Statistic);
            Assert.Equal("some query", tile1.Query);
            Assert.Equal("1.0.0", tile1.Version);
            Assert.Equal(IndicatorUnit.MillionPounds, tile1.IndicatorUnit);
            Assert.False(tile1.IsLatestVersion);
            Assert.Equal("publication-slug", tile1.PublicationSlug);
            Assert.Equal("release-slug", tile1.ReleaseSlug);

            var tile2 = Assert.IsType<EinFreeTextStatTileViewModel>(einTileGroupBlock.Tiles[1]);
            Assert.Equal("free text stat tile", tile2.Title);
            Assert.Equal("Over 9000!", tile2.Statistic);
            Assert.Equal("It's up!", tile2.Trend);
            Assert.Equal("Link text", tile2.LinkText);
            Assert.Equal("https://example.com/", tile2.LinkUrl);
        }
    }

    [Fact]
    public async Task GetEinPage_ReturnsRootPage_Success()
    {
        var contentDbContextId = Guid.NewGuid().ToString();
        var pageLatestVersionId = Guid.NewGuid();
        var page = new EinPage
        {
            Id = Guid.NewGuid(),
            Title = "Root Page",
            Slug = null,
            Description = "Description",
            Order = 0,
            LatestPublishedVersionId = pageLatestVersionId,
        };
        var pageVersions = new List<EinPageVersion>
        {
            new()
            {
                Id = Guid.NewGuid(),
                EinPageId = page.Id,
                EinPage = page,
                Version = 0,
                Published = new DateTimeOffset(2000, 1, 1, 0, 0, 0, TimeSpan.Zero),
                Content = [],
            },
            new()
            {
                Id = pageLatestVersionId,
                EinPageId = page.Id,
                EinPage = page,
                Version = 1,
                Published = DateTimeOffset.UtcNow,
                Content = [],
            },
        };
        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            contentDbContext.EinPageVersions.AddRange(pageVersions);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var service = new EducationInNumbersService(contentDbContext);

            var result = await service.GetEinPage(null, CancellationToken.None);

            var viewModel = result.AssertRight();
            Assert.Equal(pageVersions[1].Id, viewModel.Id);
            Assert.Equal("Root Page", viewModel.Title);
            Assert.Null(viewModel.Slug);
            Assert.Equal("Description", viewModel.Description);
            viewModel.Published.AssertUtcNow();

            Assert.Empty(viewModel.Content);
        }
    }

    [Fact]
    public async Task GetEinPage_NoPublishedPageVersion_NotFound()
    {
        var pageId = Guid.NewGuid();
        var pageVersionId = Guid.NewGuid();
        var pageVersion = new EinPageVersion
        {
            Id = pageVersionId,
            EinPageId = pageId,
            EinPage = new EinPage
            {
                Id = pageId,
                Slug = "test-page",
                LatestVersionId = pageVersionId,
                LatestPublishedVersionId = null, // unpublished
            },
            Created = DateTime.UtcNow,
            Published = null,
            Content = [],
        };

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            contentDbContext.EinPageVersions.AddRange(pageVersion);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var service = new EducationInNumbersService(contentDbContext);

            var result = await service.GetEinPage("test-page", CancellationToken.None);
            result.AssertNotFound();
        }
    }

    [Fact]
    public async Task GetEinPage_SlugNotFound_NotFound()
    {
        var pageId = Guid.NewGuid();
        var pageVersionId = Guid.NewGuid();
        var pageVersion = new EinPageVersion
        {
            Id = pageVersionId,
            EinPageId = pageId,
            EinPage = new EinPage
            {
                Id = pageId,
                Slug = "test-page",
                LatestVersionId = pageVersionId,
                LatestPublishedVersionId = pageVersionId,
            },
            Created = DateTime.UtcNow,
            Published = DateTime.UtcNow,
            Content = [],
        };

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            contentDbContext.EinPageVersions.AddRange(pageVersion);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var service = new EducationInNumbersService(contentDbContext);

            var result = await service.GetEinPage("non-existent-slug", CancellationToken.None);
            result.AssertNotFound();
        }
    }

    [Fact]
    public async Task ListSitemapItems_Success()
    {
        var page1Id = Guid.NewGuid();
        var page1VersionId = Guid.NewGuid();
        var page2Id = Guid.NewGuid();
        var page2VersionId = Guid.NewGuid();
        var page3Id = Guid.NewGuid();
        var page3VersionId = Guid.NewGuid();
        var page4Id = Guid.NewGuid();
        var page4VersionId = Guid.NewGuid();

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            contentDbContext.EinPageVersions.AddRange([
                new EinPageVersion
                {
                    Id = page1VersionId,
                    EinPage = new EinPage
                    {
                        Id = page1Id,
                        Slug = "test-1",
                        LatestPublishedVersionId = page1VersionId,
                    },
                    Published = new DateTimeOffset(2000, 01, 01, 0, 0, 0, TimeSpan.Zero),
                    Created = new DateTimeOffset(2000, 01, 01, 0, 0, 0, TimeSpan.Zero),
                },
                new EinPageVersion
                {
                    Id = page2VersionId,
                    EinPage = new EinPage
                    {
                        Id = page2Id,
                        Slug = "test-2",
                        LatestPublishedVersionId = page2VersionId,
                    },
                    Published = new DateTimeOffset(2000, 01, 01, 0, 0, 0, TimeSpan.Zero),
                    Created = new DateTimeOffset(2000, 01, 01, 0, 0, 0, TimeSpan.Zero),
                    Updated = new DateTimeOffset(2001, 01, 01, 0, 0, 0, TimeSpan.Zero),
                },
                new EinPageVersion
                {
                    Id = page3VersionId,
                    EinPage = new EinPage
                    {
                        Id = page3Id,
                        Slug = "test-3",
                        LatestPublishedVersionId = page3VersionId,
                    },
                    Published = new DateTimeOffset(2000, 01, 01, 0, 0, 0, TimeSpan.Zero),
                    Created = new DateTimeOffset(2000, 01, 01, 0, 0, 0, TimeSpan.Zero),
                    Updated = new DateTimeOffset(2002, 01, 01, 0, 0, 0, TimeSpan.Zero),
                },
                // unpublished so won't appear
                new EinPageVersion
                {
                    Id = page4VersionId,
                    EinPage = new EinPage
                    {
                        Id = page4Id,
                        Slug = "test-4",
                        LatestPublishedVersionId = null,
                    },
                    Created = new DateTimeOffset(2003, 01, 01, 0, 0, 0, TimeSpan.Zero),
                },
            ]);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var service = new EducationInNumbersService(contentDbContext);

            var result = await service.ListSitemapItems(CancellationToken.None);
            var viewModels = result.AssertRight();
            Assert.Equal(3, viewModels.Count);

            Assert.Equal("test-1", viewModels[0].Slug);
            Assert.Equal(new DateTimeOffset(2000, 1, 1, 0, 0, 0, TimeSpan.Zero), viewModels[0].LastModified); // Created date because no Updated date
            Assert.Equal("test-2", viewModels[1].Slug);
            Assert.Equal(new DateTimeOffset(2001, 1, 1, 0, 0, 0, TimeSpan.Zero), viewModels[1].LastModified);
            Assert.Equal("test-3", viewModels[2].Slug);
            Assert.Equal(new DateTimeOffset(2002, 1, 1, 0, 0, 0, TimeSpan.Zero), viewModels[2].LastModified);
        }
    }

    [Fact]
    public async Task ListSitemapItems_Empty_Success()
    {
        await using var contentDbContext = InMemoryContentDbContext();
        var service = new EducationInNumbersService(contentDbContext);

        var result = await service.ListSitemapItems(CancellationToken.None);
        var viewModels = result.AssertRight();
        Assert.Empty(viewModels);
    }
}
