#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

public class ContentBlockServiceTests
{
    [Fact]
    public async Task DeleteContentBlockAndReorder_EmbedBlockLink()
    {
        var releaseVersion = new ReleaseVersion();
        var contentBlockId = Guid.NewGuid();
        var embedBlockId = Guid.NewGuid();

        var contentBlocks = new List<ContentBlock>
        {
            new HtmlBlock { Order = 1 },
            new HtmlBlock { Order = 2 },
            new EmbedBlockLink
            {
                Id = contentBlockId,
                Order = 3,
                EmbedBlock = new EmbedBlock
                {
                    Id = embedBlockId,
                    Title = "Test title",
                    Url = "http://www.test.com",
                },
            },
            new HtmlBlock { Order = 4 },
            new HtmlBlock { Order = 5 },
        };

        var contentSection = new ContentSection
        {
            Id = Guid.NewGuid(),
            Content = contentBlocks,
            ReleaseVersion = releaseVersion,
        };

        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryContentDbContext(contextId))
        {
            await context.ContentSections.AddRangeAsync(contentSection);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var service = SetupContentBlockService(context);
            await service.DeleteContentBlockAndReorder(contentSection.Content[2].Id);
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var dbContentSection = context
                .ContentSections.Include(cs => cs.Content)
                .Single(cs => cs.Id == contentSection.Id);

            Assert.Equal(4, dbContentSection.Content.Count);

            Assert.Equal(contentBlocks[0].Id, dbContentSection.Content[0].Id);
            Assert.Equal(1, dbContentSection.Content[0].Order);
            Assert.Equal(contentBlocks[1].Id, dbContentSection.Content[1].Id);
            Assert.Equal(2, dbContentSection.Content[1].Order);
            // NOTE: Skip contentBlocks[2] as it will be deleted
            Assert.Equal(contentBlocks[3].Id, dbContentSection.Content[2].Id);
            Assert.Equal(3, dbContentSection.Content[2].Order);
            Assert.Equal(contentBlocks[4].Id, dbContentSection.Content[3].Id);
            Assert.Equal(4, dbContentSection.Content[3].Order);

            var embedBlockLinks = context.EmbedBlockLinks.ToList();
            Assert.Empty(embedBlockLinks);

            var embedBlocks = context.EmbedBlocks.ToList();
            Assert.Empty(embedBlocks);
        }
    }

    [Fact]
    public async Task DeleteContentBlockAndReorder_DataBlock()
    {
        var contentBlockId = Guid.NewGuid();

        var contentSection = new ContentSection
        {
            Content =
            [
                new HtmlBlock { Order = 1 },
                new HtmlBlock { Order = 2 },
                new DataBlockVersionLink
                {
                    Id = contentBlockId,
                    Order = 3,
                    DataBlockVersionId = contentBlockId,
                    DataBlockVersion = new DataBlockVersion { Id = contentBlockId },
                },
                new HtmlBlock { Order = 4 },
                new HtmlBlock { Order = 5 },
            ],
            ReleaseVersion = new ReleaseVersion(),
        };

        var contentBlocks = contentSection.Content;

        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryContentDbContext(contextId))
        {
            await context.ContentSections.AddRangeAsync(contentSection);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var service = SetupContentBlockService(context);
            await service.DeleteContentBlockAndReorder(contentSection.Content[2].Id);
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var dbContentSection = context
                .ContentSections.Include(cs => cs.Content)
                .Single(cs => cs.Id == contentSection.Id);

            Assert.Equal(4, dbContentSection.Content.Count);

            Assert.Equal(contentBlocks[0].Id, dbContentSection.Content[0].Id);
            Assert.Equal(1, dbContentSection.Content[0].Order);
            Assert.Equal(contentBlocks[1].Id, dbContentSection.Content[1].Id);
            Assert.Equal(2, dbContentSection.Content[1].Order);

            // Data blocks are detached, not deleted - verify that the content block's associated data block version still exists
            Assert.Single(context.DataBlockVersions);
            Assert.Empty(context.DataBlockVersionLinks);

            Assert.Equal(contentBlocks[3].Id, dbContentSection.Content[2].Id);
            Assert.Equal(3, dbContentSection.Content[2].Order);
            Assert.Equal(contentBlocks[4].Id, dbContentSection.Content[3].Id);
            Assert.Equal(4, dbContentSection.Content[3].Order);
        }
    }

    [Fact]
    public async Task DeleteContentBlockAndReorder_HtmlBlock()
    {
        var contentBlockId = Guid.NewGuid();

        var contentSection = new ContentSection
        {
            Content = new List<ContentBlock>
            {
                new HtmlBlock { Order = 1 },
                new HtmlBlock { Order = 2 },
                new HtmlBlock { Id = contentBlockId, Order = 3 },
                new HtmlBlock { Order = 4 },
                new HtmlBlock { Order = 5 },
            },
            ReleaseVersion = new ReleaseVersion(),
        };

        var contentBlocks = contentSection.Content;

        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryContentDbContext(contextId))
        {
            await context.ContentSections.AddRangeAsync(contentSection);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var service = SetupContentBlockService(context);
            await service.DeleteContentBlockAndReorder(contentSection.Content[2].Id);
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var dbContentSection = context
                .ContentSections.Include(cs => cs.Content)
                .Single(cs => cs.Id == contentSection.Id);

            Assert.Equal(4, dbContentSection.Content.Count);

            Assert.Equal(contentBlocks[0].Id, dbContentSection.Content[0].Id);
            Assert.Equal(1, dbContentSection.Content[0].Order);
            Assert.Equal(contentBlocks[1].Id, dbContentSection.Content[1].Id);
            Assert.Equal(2, dbContentSection.Content[1].Order);
            // Skip contentBlocks[2] as it will be deleted
            Assert.Equal(contentBlocks[3].Id, dbContentSection.Content[2].Id);
            Assert.Equal(3, dbContentSection.Content[2].Order);
            Assert.Equal(contentBlocks[4].Id, dbContentSection.Content[3].Id);
            Assert.Equal(4, dbContentSection.Content[3].Order);
        }
    }

    [Fact]
    public async Task DeleteSectionContentBlocks()
    {
        var dataBlockVersionId = Guid.NewGuid();

        var contentSection = new ContentSection
        {
            Content =
            [
                new HtmlBlock(),
                new DataBlockVersionLink { Id = dataBlockVersionId },
                new EmbedBlockLink
                {
                    EmbedBlock = new EmbedBlock { Title = "title", Url = "https://" },
                },
                new HtmlBlock(),
                new HtmlBlock(),
            ],
            ReleaseVersion = new ReleaseVersion(),
        };

        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryContentDbContext(contextId))
        {
            await context.ContentSections.AddRangeAsync(contentSection);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var service = SetupContentBlockService(context);
            await service.DeleteSectionContentBlocks(contentSection.Id);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var dbContentSection = context
                .ContentSections.Include(cs => cs.Content)
                .Single(cs => cs.Id == contentSection.Id);

            Assert.Empty(dbContentSection.Content);

            var dbContentBlocks = context.ContentBlocks.ToList();
            Assert.Empty(context.ContentBlocks);
            Assert.Empty(context.DataBlockVersionLinks);

            var embedBlocks = context.EmbedBlocks.ToList();
            Assert.Empty(embedBlocks);
        }
    }

    private static ContentBlockService SetupContentBlockService(ContentDbContext contentDbContext)
    {
        return new ContentBlockService(contentDbContext);
    }
}
