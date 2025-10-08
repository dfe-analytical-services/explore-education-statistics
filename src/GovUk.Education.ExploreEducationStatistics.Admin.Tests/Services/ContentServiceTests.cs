using GovUk.Education.ExploreEducationStatistics.Admin.Hubs;
using GovUk.Education.ExploreEducationStatistics.Admin.Hubs.Clients;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.ManageContent;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Utils;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using Microsoft.AspNetCore.SignalR;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.MapperUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.CollectionUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

public class ContentServiceTests
{
    private readonly DataFixture _fixture = new();

    [Fact]
    public async Task GetContentBlocks_NoContentSections()
    {
        var releaseVersion = new ReleaseVersion();

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.Add(releaseVersion);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = SetupContentService(contentDbContext: contentDbContext);

            var result = await service.GetContentBlocks<HtmlBlock>(releaseVersion.Id);

            Assert.True(result.IsRight);

            Assert.Empty(result.Right);
        }
    }

    [Fact]
    public async Task GetContentBlocks_NoContentBlocks()
    {
        var releaseVersion = new ReleaseVersion
        {
            Content = ListOf(
                new ContentSection { Heading = "New section", Order = 1 },
                new ContentSection { Heading = "New section", Order = 2 }
            ),
        };

        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.Add(releaseVersion);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = SetupContentService(contentDbContext: contentDbContext);

            var result = await service.GetContentBlocks<HtmlBlock>(releaseVersion.Id);

            Assert.True(result.IsRight);

            Assert.Empty(result.Right);
        }
    }

    [Fact]
    public async Task GetContentBlocks()
    {
        var releaseVersionId = Guid.NewGuid();

        var releaseVersion = new ReleaseVersion
        {
            Id = releaseVersionId,
            Content = ListOf(
                new ContentSection
                {
                    Heading = "New section",
                    Order = 1,
                    Content = ListOf<ContentBlock>(
                        new HtmlBlock { Body = "Test html block 1", ReleaseVersionId = releaseVersionId },
                        new HtmlBlock { Body = "Test html block 2", ReleaseVersionId = releaseVersionId },
                        new DataBlock { Name = "Test data block 1", ReleaseVersionId = releaseVersionId }
                    ),
                    ReleaseVersionId = releaseVersionId,
                },
                new ContentSection
                {
                    Heading = "New section",
                    Order = 2,
                    Content = ListOf<ContentBlock>(
                        new HtmlBlock { Body = "Test html block 3", ReleaseVersionId = releaseVersionId },
                        new HtmlBlock { Body = "Test html block 4", ReleaseVersionId = releaseVersionId },
                        new DataBlock { Name = "Test data block 2", ReleaseVersionId = releaseVersionId }
                    ),
                    ReleaseVersionId = releaseVersionId,
                }
            ),
        };

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.Add(releaseVersion);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = SetupContentService(contentDbContext: contentDbContext);

            var result = await service.GetContentBlocks<HtmlBlock>(releaseVersion.Id);

            var contentBlocks = result.AssertRight();

            Assert.Equal(4, contentBlocks.Count);
            Assert.Equal(releaseVersion.Content[0].Content[0].Id, contentBlocks[0].Id);
            Assert.Equal(releaseVersion.Content[0].Content[1].Id, contentBlocks[1].Id);
            Assert.Equal(releaseVersion.Content[1].Content[0].Id, contentBlocks[2].Id);
            Assert.Equal(releaseVersion.Content[1].Content[1].Id, contentBlocks[3].Id);

            Assert.Equal("Test html block 1", contentBlocks[0].Body);
            Assert.Equal("Test html block 2", contentBlocks[1].Body);
            Assert.Equal("Test html block 3", contentBlocks[2].Body);
            Assert.Equal("Test html block 4", contentBlocks[3].Body);
        }
    }

    [Fact]
    public async Task RemoveContentSection()
    {
        var releaseVersion = new ReleaseVersion();

        var contentSectionToRemove = new ContentSection
        {
            Order = 1,
            Content = new List<ContentBlock>
            {
                new HtmlBlock { ReleaseVersion = releaseVersion },
                new DataBlock { ReleaseVersion = releaseVersion },
                new EmbedBlockLink { EmbedBlock = new EmbedBlock(), ReleaseVersion = releaseVersion },
            },
            ReleaseVersion = releaseVersion,
        };

        var contentSection2 = new ContentSection
        {
            Order = 2,
            Content = new List<ContentBlock>(),
            ReleaseVersion = releaseVersion,
        };

        var contentSection3 = new ContentSection
        {
            Order = 3,
            Content = new List<ContentBlock>(),
            ReleaseVersion = releaseVersion,
        };

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.AddRange(releaseVersion);
            contentDbContext.ContentSections.AddRange(contentSectionToRemove, contentSection2, contentSection3);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = SetupContentService(contentDbContext: contentDbContext);
            var result = await service.RemoveContentSection(
                releaseVersionId: contentSectionToRemove.ReleaseVersionId,
                contentSectionId: contentSectionToRemove.Id
            );

            var contentSectionList = result.AssertRight();

            Assert.Equal(2, contentSectionList.Count);

            Assert.Equal(contentSection2.Id, contentSectionList[0].Id);
            Assert.Equal(1, contentSectionList[0].Order);
            Assert.Equal(contentSection3.Id, contentSectionList[1].Id);
            Assert.Equal(2, contentSectionList[1].Order);
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var contentSections = contentDbContext.ContentSections.ToList();
            Assert.Equal(2, contentSections.Count);

            Assert.Equal(contentSection2.Id, contentSections[0].Id);
            Assert.Equal(contentSection2.ReleaseVersionId, contentSections[0].ReleaseVersionId);
            Assert.Equal(1, contentSections[0].Order);

            Assert.Equal(contentSection3.Id, contentSections[1].Id);
            Assert.Equal(contentSection3.ReleaseVersionId, contentSections[1].ReleaseVersionId);
            Assert.Equal(2, contentSections[1].Order);

            var contentBlocks = contentDbContext.ContentBlocks.ToList();

            var dataBlock = Assert.Single(contentBlocks); // data blocks are detached, not deleted
            Assert.IsType<DataBlock>(dataBlock);
            Assert.Equal(0, dataBlock.Order);
            Assert.Null(dataBlock.ContentSectionId);

            var embedBlocks = contentDbContext.EmbedBlocks.ToList();
            Assert.Empty(embedBlocks);
        }
    }

    [Fact]
    public async Task RemoveContentSection_NoRelease()
    {
        var contentSection = new ContentSection
        {
            Content = new List<ContentBlock> { new HtmlBlock() },
            ReleaseVersion = new ReleaseVersion(),
        };
        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            await contentDbContext.ContentSections.AddAsync(contentSection);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = SetupContentService(contentDbContext: contentDbContext);
            var result = await service.RemoveContentSection(Guid.NewGuid(), contentSection.Id);

            result.AssertNotFound();
        }
    }

    [Fact]
    public async Task RemoveContentSection_ContentSectionBelongsToAnotherRelease()
    {
        var releaseVersion = new ReleaseVersion();
        var otherReleaseVersion = new ReleaseVersion();
        var relatedContentSection = new ContentSection
        {
            Content = new List<ContentBlock> { new HtmlBlock { ReleaseVersion = releaseVersion } },
            ReleaseVersion = releaseVersion,
        };
        var unrelatedContentSection = new ContentSection
        {
            Id = Guid.NewGuid(),
            Content = new List<ContentBlock> { new HtmlBlock { ReleaseVersion = otherReleaseVersion } },
            ReleaseVersion = otherReleaseVersion,
        };

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            await contentDbContext.ContentSections.AddRangeAsync(relatedContentSection, unrelatedContentSection);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = SetupContentService(contentDbContext: contentDbContext);
            var result = await service.RemoveContentSection(
                releaseVersionId: releaseVersion.Id,
                contentSectionId: unrelatedContentSection.Id
            );

            result.AssertNotFound();
        }
    }

    [Fact]
    public async Task RemoveContentBlock()
    {
        var contentSectionId = Guid.NewGuid();

        var blockToRemove = new HtmlBlock
        {
            Order = 0,
            Comments = new List<Comment>
            {
                new() { Content = "Comment to be removed 1" },
                new() { Content = "Comment to be removed 2" },
            },
        };

        var dataBlockVersionId = Guid.NewGuid();
        var dataBlockVersion = new DataBlockVersion
        {
            Id = dataBlockVersionId,
            ContentBlock = new DataBlock
            {
                Id = dataBlockVersionId,
                Order = 1,
                Comments = new List<Comment>
                {
                    new() { Content = "Comment 1" },
                    new() { Content = "Comment 2" },
                },
            },
            DataBlockParentId = Guid.NewGuid(),
        };

        var contentSection = new ContentSection
        {
            Id = contentSectionId,
            Content = new List<ContentBlock>
            {
                blockToRemove,
                dataBlockVersion.ContentBlock,
                new HtmlBlock { Order = 2 },
            },
            ReleaseVersion = new ReleaseVersion(),
        };

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            await contentDbContext.ContentSections.AddRangeAsync(contentSection);
            await contentDbContext.DataBlockVersions.AddRangeAsync(dataBlockVersion);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = SetupContentService(contentDbContext: contentDbContext);
            var result = await service.RemoveContentBlock(
                releaseVersionId: contentSection.ReleaseVersionId,
                contentSectionId: contentSectionId,
                contentBlockId: blockToRemove.Id
            );

            var viewModelList = result.AssertRight();

            Assert.Equal(2, viewModelList.Count);
            Assert.Null(viewModelList.Find(viewModel => viewModel.Id == blockToRemove.Id));
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var contentBlocks = contentDbContext.ContentBlocks.OrderBy(cb => cb.Order).ToList();

            Assert.Equal(2, contentBlocks.Count);

            Assert.Equal(0, contentBlocks[0].Order);
            Assert.IsType<DataBlock>(contentBlocks[0]);
            Assert.Equal(1, contentBlocks[1].Order);
            Assert.IsType<HtmlBlock>(contentBlocks[1]);

            var comments = contentDbContext
                .Comment.Where(c =>
                    c.ContentBlockId == dataBlockVersion.ContentBlockId || c.ContentBlockId == blockToRemove.Id
                )
                .ToList();
            Assert.Equal(2, comments.Count);
            Assert.Equal("Comment 1", comments[0].Content);
            Assert.Equal("Comment 2", comments[1].Content);
        }
    }

    [Fact]
    public async Task RemoveContentBlock_NoRelease()
    {
        var contentBlockId = Guid.NewGuid();
        var contentSection = new ContentSection
        {
            Content = new List<ContentBlock>
            {
                new HtmlBlock { Id = contentBlockId },
                new DataBlock(),
                new HtmlBlock(),
            },
            ReleaseVersion = new ReleaseVersion(),
        };

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            await contentDbContext.ContentSections.AddRangeAsync(contentSection);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = SetupContentService(contentDbContext: contentDbContext);
            var result = await service.RemoveContentBlock(Guid.NewGuid(), contentSection.Id, contentBlockId);

            result.AssertNotFound();
        }
    }

    [Fact]
    public async Task RemoveContentBlock_NoContentSection()
    {
        var releaseVersion = new ReleaseVersion();
        var contentBlockId = Guid.NewGuid();
        var contentSection = new ContentSection
        {
            Content = new List<ContentBlock>
            {
                new HtmlBlock { Id = contentBlockId, ReleaseVersion = releaseVersion },
                new DataBlock { ReleaseVersion = releaseVersion },
                new HtmlBlock { ReleaseVersion = releaseVersion },
            },
        };

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            await contentDbContext.ContentSections.AddRangeAsync(contentSection);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = SetupContentService(contentDbContext: contentDbContext);
            var result = await service.RemoveContentBlock(
                releaseVersionId: contentSection.ReleaseVersionId,
                contentSectionId: Guid.NewGuid(),
                contentBlockId: contentBlockId
            );

            result.AssertNotFound();
        }
    }

    [Fact]
    public async Task RemoveContentBlock_NoContentBlock()
    {
        var contentBlockId = Guid.NewGuid();
        var contentSection = new ContentSection
        {
            Content = new List<ContentBlock>
            {
                new HtmlBlock { Id = contentBlockId },
                new DataBlock(),
                new HtmlBlock(),
            },
            ReleaseVersion = new ReleaseVersion(),
        };

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            await contentDbContext.ContentSections.AddRangeAsync(contentSection);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = SetupContentService(contentDbContext: contentDbContext);
            var result = await service.RemoveContentBlock(
                releaseVersionId: contentSection.ReleaseVersionId,
                contentSectionId: contentSection.Id,
                contentBlockId: Guid.NewGuid()
            );

            result.AssertNotFound();
        }
    }

    [Fact]
    public async Task RemoveContentBlock_BlockAttachedToIncorrectSection()
    {
        var releaseVersion = new ReleaseVersion();

        var incorrectContentBlockId = Guid.NewGuid();
        var incorrectContentSection = new ContentSection
        {
            Content = new List<ContentBlock> { new HtmlBlock { Id = incorrectContentBlockId } },
            ReleaseVersion = releaseVersion,
        };

        var contentSection = new ContentSection
        {
            Content = new List<ContentBlock> { new HtmlBlock() },
            ReleaseVersion = releaseVersion,
        };

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            await contentDbContext.ContentSections.AddRangeAsync(contentSection, incorrectContentSection);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = SetupContentService(contentDbContext: contentDbContext);
            var result = await service.RemoveContentBlock(
                releaseVersionId: contentSection.ReleaseVersionId,
                contentSectionId: contentSection.Id,
                contentBlockId: incorrectContentBlockId
            );

            result.AssertNotFound();
        }
    }

    [Fact]
    public async Task AttachDataBlock()
    {
        var releaseVersion = _fixture.DefaultReleaseVersion().Generate();

        var dataBlockParent = _fixture
            .DefaultDataBlockParent()
            .WithLatestPublishedVersion(
                _fixture.DefaultDataBlockVersion().WithOrder(1).WithReleaseVersion(releaseVersion)
            )
            .Generate();

        var dataBlockVersion = dataBlockParent.LatestPublishedVersion!;

        releaseVersion.Content = _fixture
            .DefaultContentSection()
            .WithContentBlocks(
                _fixture
                    .DefaultHtmlBlock()
                    .ForIndex(0, s => s.SetOrder(1))
                    .ForIndex(1, s => s.SetOrder(3))
                    .ForIndex(2, s => s.SetOrder(5))
                    .GenerateList()
            )
            .GenerateList(1);

        var contentDbContextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            await contentDbContext.DataBlockParents.AddRangeAsync(dataBlockParent);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            var service = SetupContentService(contentDbContext: contentDbContext);
            var result = await service.AttachDataBlock(
                releaseVersionId: releaseVersion.Id,
                contentSectionId: releaseVersion.Content[0].Id,
                new DataBlockAttachRequest { ContentBlockId = dataBlockVersion.Id, Order = 3 }
            );

            var dataBlockViewModel = result.AssertRight();
            Assert.Equal(dataBlockVersion.Id, dataBlockViewModel.Id);
            Assert.Equal(dataBlockParent.Id, dataBlockViewModel.DataBlockParentId);
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contentDbContextId))
        {
            // Assert that the DataBlock's requested attachment Order has been updated to reflect its new position.
            var dataBlock = Assert.Single(contentDbContext.DataBlocks);
            Assert.Equal(3, dataBlock.Order);

            // Assert that the existing HtmlBlocks' Orders are updated where appropriate.
            var htmlBlocks = contentDbContext.HtmlBlocks.ToList();
            Assert.Equal(3, htmlBlocks.Count);

            // An HtmlBlock before the inserted DataBlock should not have its Order changed.
            Assert.Equal(1, htmlBlocks[0].Order);

            // HtmlBlocks with Orders the same as or greater than the DataBlock's should have their Orders
            // incremented.
            Assert.Equal(4, htmlBlocks[1].Order);
            Assert.Equal(6, htmlBlocks[2].Order);
        }
    }

    private static ContentService SetupContentService(
        ContentDbContext contentDbContext,
        IPersistenceHelper<ContentDbContext> persistenceHelper = null,
        IContentSectionRepository contentSectionRepository = null,
        IContentBlockService contentBlockService = null,
        IHubContext<ReleaseContentHub, IReleaseContentHubClient> hubContext = null,
        IUserService userService = null
    )
    {
        return new ContentService(
            contentDbContext,
            persistenceHelper ?? new PersistenceHelper<ContentDbContext>(contentDbContext),
            contentSectionRepository ?? new ContentSectionRepository(contentDbContext),
            contentBlockService ?? new ContentBlockService(contentDbContext),
            hubContext ?? Mock.Of<IHubContext<ReleaseContentHub, IReleaseContentHubClient>>(MockBehavior.Strict),
            userService ?? MockUtils.AlwaysTrueUserService().Object,
            AdminMapper(contentDbContext)
        );
    }
}
