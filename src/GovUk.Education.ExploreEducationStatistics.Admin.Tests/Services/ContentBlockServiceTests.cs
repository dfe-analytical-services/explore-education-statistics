using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.EntityFrameworkCore;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

public class ContentBlockServiceTests
{
    [Fact]
    public async Task DeleteContentBlockFromContentSection_EmbedBlockLink()
    {
        var release = new Release();
        var contentBlockId = Guid.NewGuid();
        var embedBlockId = Guid.NewGuid();

        var contentBlocks = new List<ContentBlock>
        {
            new HtmlBlock { Order = 1, },
            new HtmlBlock { Order = 2, },
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
            new HtmlBlock { Order = 4, },
            new HtmlBlock { Order = 5, },
        };

        var contentSection = new ContentSection
        {
            Id = Guid.NewGuid(),
            Content = contentBlocks,
        };

        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryContentDbContext(contextId))
        {
            await context.ReleaseContentSections.AddRangeAsync(new ReleaseContentSection
            {
                Release = release,
                ContentSection = contentSection,
            });
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var service = SetupContentBlockService(context);
            await service.DeleteContentBlockAndReorder(contentSection.Content[2].Id);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var dbContentSection = context.ContentSections
                .Include(cs => cs.Content)
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
    public async Task DeleteContentBlockFromContentSection_DataBlock()
    {
        var contentBlockId = Guid.NewGuid();

        var releaseContentSection = new ReleaseContentSection
        {
            Release = new Release(),
            ContentSection = new ContentSection
            {
                Content = new List<ContentBlock>
                {
                    new HtmlBlock { Order = 1, },
                    new HtmlBlock { Order = 2, },
                    new DataBlock
                    {
                        Id = contentBlockId,
                        Order = 3,
                    },
                    new HtmlBlock { Order = 4, },
                    new HtmlBlock { Order = 5, },
                },
            }
        };

        var contentSection = releaseContentSection.ContentSection;
        var contentBlocks = contentSection.Content;

        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryContentDbContext(contextId))
        {
            await context.ReleaseContentSections.AddRangeAsync(releaseContentSection);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var service = SetupContentBlockService(context);
            await service.DeleteContentBlockAndReorder(contentSection.Content[2].Id);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var dbContentSection = context.ContentSections
                .Include(cs => cs.Content)
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

            var dataBlocks = context.DataBlocks.ToList();
            var dataBlock = Assert.Single(dataBlocks);
            Assert.Equal(0, dataBlock.Order);
            Assert.Null(dataBlock.ContentSectionId);
        }
    }

    [Fact]
    public async Task DeleteContentBlockFromContentSection_HtmlBlock()
    {
        var contentBlockId = Guid.NewGuid();

        var releaseContentSection = new ReleaseContentSection
        {
            Release = new Release(),
            ContentSection = new ContentSection
            {
                Content = new List<ContentBlock>
                {
                    new HtmlBlock { Order = 1, },
                    new HtmlBlock { Order = 2, },
                    new HtmlBlock
                    {
                        Id = contentBlockId,
                        Order = 3,
                    },
                    new HtmlBlock { Order = 4, },
                    new HtmlBlock { Order = 5, },
                },
            }
        };

        var contentSection = releaseContentSection.ContentSection;
        var contentBlocks = contentSection.Content;

        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryContentDbContext(contextId))
        {
            await context.ReleaseContentSections.AddRangeAsync(releaseContentSection);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var service = SetupContentBlockService(context);
            await service.DeleteContentBlockAndReorder(contentSection.Content[2].Id);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var dbContentSection = context.ContentSections
                .Include(cs => cs.Content)
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
        }
    }

    [Fact]
    public async Task DeleteContentBlockFromContentSection_NoReorder()
    {
        var contentBlockId = Guid.NewGuid();

        var releaseContentSection = new ReleaseContentSection
        {
            Release = new Release(),
            ContentSection = new ContentSection
            {
                Content = new List<ContentBlock>
                {
                    new HtmlBlock { Order = 1, },
                    new HtmlBlock { Order = 2, },
                    new HtmlBlock
                    {
                        Id = contentBlockId,
                        Order = 3,
                    },
                    new HtmlBlock { Order = 4, },
                    new HtmlBlock { Order = 5, },
                },
            }
        };

        var contentSection = releaseContentSection.ContentSection;
        var contentBlocks = contentSection.Content;

        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryContentDbContext(contextId))
        {
            await context.ReleaseContentSections.AddRangeAsync(releaseContentSection);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var service = SetupContentBlockService(context);
            await service.DeleteContentBlockAndReorder(contentSection.Content[2].Id, reorder: false);
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryContentDbContext(contextId))
        {
            var dbContentSection = context.ContentSections
                .Include(cs => cs.Content)
                .Single(cs => cs.Id == contentSection.Id);

            Assert.Equal(4, dbContentSection.Content.Count);

            Assert.Equal(contentBlocks[0].Id, dbContentSection.Content[0].Id);
            Assert.Equal(1, dbContentSection.Content[0].Order);
            Assert.Equal(contentBlocks[1].Id, dbContentSection.Content[1].Id);
            Assert.Equal(2, dbContentSection.Content[1].Order);
            // NOTE: Skip contentBlocks[2] as it will be deleted
            Assert.Equal(contentBlocks[3].Id, dbContentSection.Content[2].Id);
            Assert.Equal(4, dbContentSection.Content[2].Order);
            Assert.Equal(contentBlocks[4].Id, dbContentSection.Content[3].Id);
            Assert.Equal(5, dbContentSection.Content[3].Order);
        }
    }
    private static ContentBlockService SetupContentBlockService(ContentDbContext contentDbContext)
    {
        return new ContentBlockService(contentDbContext);
    }
}
