using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;
using static GovUk.Education.ExploreEducationStatistics.Content.ViewModels.EducationInNumbersContentViewModels;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Tests;

public class EducationInNumbersServiceTests
{
    [Fact]
    public async Task ListEinPages_ReturnsLatestVersionOfEachUniqueSlug_Success()
    {
        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var pages = new List<EducationInNumbersPage>
            {
                new() { Id = Guid.NewGuid(), Title = "Page 1 v1", Slug = "page-1", Order = 2, Version = 1, Published = DateTime.UtcNow },
                new() { Id = Guid.NewGuid(), Title = "Page 1 v2", Slug = "page-1", Order = 2, Version = 2, Published = DateTime.UtcNow },
                new() { Id = Guid.NewGuid(), Title = "Page 2 v1", Slug = "page-2", Order = 1, Version = 1, Published = DateTime.UtcNow },
                new() { Id = Guid.NewGuid(), Title = "Page 2 v2 (unpublished)", Slug = "page-2", Order = 1, Version = 2, Published = null },
                new() { Id = Guid.NewGuid(), Title = "Page 3 v1", Slug = "page-3", Order = 0, Version = 0, Published = DateTime.UtcNow },
            };
            await contentDbContext.EducationInNumbersPages.AddRangeAsync(pages);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var service = new EducationInNumbersService(contentDbContext);

            var result = await service.ListEinPages();

            var viewModels = result.AssertRight();
            Assert.Equal(3, viewModels.Count);
            Assert.Equal("Page 3 v1", viewModels[0].Title); // returned in order
            Assert.Equal("Page 2 v1", viewModels[1].Title);
            Assert.Equal("Page 1 v2", viewModels[2].Title);
        }
    }

    [Fact]
    public async Task ListEinPages_ReturnsEmptyListWhenNoPages_Success()
    {
        var contentDbContextId = Guid.NewGuid().ToString();
        await using var contentDbContext = InMemoryContentDbContext(contentDbContextId);
        var service = new EducationInNumbersService(contentDbContext);

        var result = await service.ListEinPages();

        var navItems = result.AssertRight();

        Assert.Empty(navItems);
    }

    [Fact]
    public async Task GetEinPage_ReturnsLatestPage_Success()
    {
        var contentDbContextId = Guid.NewGuid().ToString();
        var pages = new List<EducationInNumbersPage>
        {
            new() { Id = Guid.NewGuid(), Title = "Test Page v1", Slug = "test-page", Order = 0, Version = 0, Published = DateTime.UtcNow, Content = [] },
            new()
            {
                Id = Guid.NewGuid(),
                Title = "Test Page v2",
                Slug = "test-page",
                Description = "Desc v2",
                Order = 0,
                Version = 1,
                Published = new DateTime(2000, 1, 1),
                Content = [
                    new EinContentSection
                    {
                        Id = Guid.NewGuid(),
                        Heading = "Section heading",
                        Order = 0,
                        Content = [
                            new EinHtmlBlock
                            {
                                Order = 1,
                                Body = "EinHtmlBlock 2",
                            },
                            new EinHtmlBlock
                            {
                                Order = 0,
                                Body = "EinHtmlBlock 1",
                            }
                        ]
                    }
                ]
            },
            new() { Id = Guid.NewGuid(), Title = "Another Page", Slug = "another-page", Order = 1, Published = DateTime.UtcNow, Content = [] }
        };
        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            await contentDbContext.EducationInNumbersPages.AddRangeAsync(pages);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            var service = new EducationInNumbersService(contentDbContext);

            var result = await service.GetEinPage("test-page");

            var viewModel = result.AssertRight();
            Assert.Equal(pages[1].Id, viewModel.Id);
            Assert.Equal("Test Page v2", viewModel.Title);
            Assert.Equal("test-page", viewModel.Slug);
            Assert.Equal("Desc v2", viewModel.Description);
            Assert.Equal(new DateTime(2000, 1, 1), viewModel.Published);

            var section = Assert.Single(viewModel.Content);
            Assert.Equal("Section heading", section.Heading);
            Assert.Equal(0, section.Order);

            var blocks = section.Content;
            Assert.Equal(2, blocks.Count);

            var block1 = blocks[0];
            var einHtmlBlock1 = Assert.IsType<EinHtmlBlockViewModel>(block1);
            Assert.Equal("EinHtmlBlock 1", einHtmlBlock1.Body);
            Assert.Equal(0, einHtmlBlock1.Order);

            var block2 = blocks[1];
            var einHtmlBlock2 = Assert.IsType<EinHtmlBlockViewModel>(block2);
            Assert.Equal("EinHtmlBlock 2", einHtmlBlock2.Body);
            Assert.Equal(1, einHtmlBlock2.Order);
        }
    }

    [Fact]
    public async Task GetEinPage_SlugDoesNotExist_NotFound()
    {
        var contentDbContextId = Guid.NewGuid().ToString();
        await using var contentDbContext = InMemoryContentDbContext(contentDbContextId);
        var service = new EducationInNumbersService(contentDbContext);

        var result = await service.GetEinPage("non-existent-slug");
        result.AssertNotFound();
    }
}
