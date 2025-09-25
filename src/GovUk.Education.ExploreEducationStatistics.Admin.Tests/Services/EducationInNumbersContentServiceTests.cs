#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Validators;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

public class EducationInNumbersContentServiceTests
{
    private readonly Guid _pageId = Guid.NewGuid();
    private readonly Guid _sectionAId = Guid.NewGuid();
    private readonly Guid _sectionBId = Guid.NewGuid();
    private readonly Guid _blockAId = Guid.NewGuid();
    private readonly Guid _blockBId = Guid.NewGuid();
    private readonly Guid _tileAId = Guid.NewGuid();
    private readonly Guid _tileBId = Guid.NewGuid();

    [Fact]
    public async Task GetPageContent_Success()
    {
        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            await context.EducationInNumbersPages.AddAsync(new EducationInNumbersPage
            {
                Id = _pageId,
                Title = "Test Page",
                Slug = "test-page",
                Content =
                [
                    new EinContentSection
                    {
                        Id = _sectionBId,
                        Order = 1,
                        Heading = "Section B",
                        Content =
                        [
                            new EinHtmlBlock
                            {
                                Id = _blockBId,
                                Order = 0,
                                Body = "Block B Body"
                            }
                        ]
                    },
                    new EinContentSection
                    {
                        Id = _sectionAId,
                        Order = 0,
                        Heading = "Section A",
                        Content =
                        [
                            new EinTileGroupBlock()
                            {
                                Id = _blockAId,
                                Order = 0,
                                Title = "TileGroupBlock title",
                                Tiles =
                                [
                                    new EinFreeTextStatTile
                                    {
                                        Id = Guid.NewGuid(),
                                        Title = "Tile title",
                                        Statistic = "Over 9000!",
                                        Trend = "It's up",
                                        Order = 0,
                                        LinkText = "Link text",
                                        LinkUrl = "http://link.url",
                                    }
                                ]
                            }
                        ]
                    }
                ]
            });
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            var service = new EducationInNumbersContentService(context);
            var result = await service.GetPageContent(_pageId);

            var viewModel = result.AssertRight();
            Assert.Equal(_pageId, viewModel.Id);
            Assert.Equal("Test Page", viewModel.Title);
            Assert.Equal("test-page", viewModel.Slug);

            Assert.Equal(2, viewModel.Content.Count);
            Assert.Equal(_sectionAId, viewModel.Content[0].Id);
            Assert.Equal(0, viewModel.Content[0].Order);
            Assert.Equal("Section A", viewModel.Content[0].Heading);
            Assert.Equal(_sectionBId, viewModel.Content[1].Id);
            Assert.Equal(1, viewModel.Content[1].Order);
            Assert.Equal("Section B", viewModel.Content[1].Heading);

            var blockAGenericType = Assert.Single(viewModel.Content[0].Content);
            var blockA = Assert.IsType<EinTileGroupBlockViewModel>(blockAGenericType);
            Assert.Equal(_blockAId, blockA.Id);
            Assert.Equal(0, blockA.Order);
            Assert.Equal("TileGroupBlock title", blockA.Title);

            var tile = Assert.Single(blockA.Tiles);
            var freeTextStatTile = Assert.IsType<EinFreeTextStatTileViewModel>(tile);
            Assert.Equal("Tile title", freeTextStatTile.Title);
            Assert.Equal("Over 9000!", freeTextStatTile.Statistic);
            Assert.Equal("It's up", freeTextStatTile.Trend);
            Assert.Equal(0, freeTextStatTile.Order);
            Assert.Equal("Link text", freeTextStatTile.LinkText);
            Assert.Equal("http://link.url", freeTextStatTile.LinkUrl);

            var blockBGenericType = Assert.Single(viewModel.Content[1].Content);
            var blockB = Assert.IsType<EinHtmlBlockViewModel>(blockBGenericType);
            Assert.Equal(_blockBId, blockB.Id);
            Assert.Equal(0, blockB.Order);
            Assert.Equal("Block B Body", blockB.Body);
        }
    }

    [Fact]
    public async Task GetPageContent_NotFound()
    {
        var contextId = Guid.NewGuid().ToString();
        await using var context = InMemoryApplicationDbContext(contextId);
        var service = new EducationInNumbersContentService(context);

        var result = await service.GetPageContent(Guid.NewGuid());
        result.AssertNotFound();
    }

    [Fact]
    public async Task AddSection_Success()
    {
        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            await context.EducationInNumbersPages.AddAsync(new EducationInNumbersPage { Id = _pageId });
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            var service = new EducationInNumbersContentService(context);
            var result = await service.AddSection(_pageId, 0);

            var viewModel = result.AssertRight();
            Assert.Equal(0, viewModel.Order);
            Assert.Equal("New section", viewModel.Heading);
            Assert.Empty(viewModel.Content);

            var dbSection = await context.EinContentSections.SingleAsync();
            Assert.Equal(viewModel.Id, dbSection.Id);
            Assert.Equal(_pageId, dbSection.EducationInNumbersPageId);
        }
    }

    [Fact]
    public async Task AddSection_OrderClashesWithPreExistingSection_Success()
    {
        var preExistingSectionId = Guid.NewGuid();
        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            await context.EducationInNumbersPages.AddAsync(new EducationInNumbersPage
            {
                Id = _pageId,
                Content =
                [
                    new EinContentSection
                    {
                        Id = preExistingSectionId,
                        Order = 0,
                        Heading = "PreExisting section",
                        EducationInNumbersPageId = _pageId,
                    }
                ]
            });
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            var service = new EducationInNumbersContentService(context);
            var result = await service.AddSection(_pageId, 0);

            var viewModel = result.AssertRight();
            Assert.Equal(0, viewModel.Order);
            Assert.Equal("New section", viewModel.Heading);
            Assert.Empty(viewModel.Content);

            var newSectionId = viewModel.Id;

            var dbSectionList = await context.EinContentSections.ToListAsync();
            Assert.Equal(2, dbSectionList.Count);

            var newSection = dbSectionList
                .Single(block => block.Id == newSectionId);
            Assert.Equal(0, newSection.Order);

            var preExistingSection = dbSectionList
                .Single(block => block.Id == preExistingSectionId);
            Assert.Equal(1, preExistingSection.Order);
            Assert.Equal("PreExisting section", preExistingSection.Heading);
        }
    }

    [Fact]
    public async Task UpdateSectionHeading_Success()
    {
        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            await context.EducationInNumbersPages.AddAsync(new EducationInNumbersPage
            {
                Id = _pageId,
                Content =
                [
                    new EinContentSection
                    {
                        Id = _sectionAId,
                        Heading = "Old Heading",
                        Content =
                        [
                            new EinTileGroupBlock { Tiles = [new EinFreeTextStatTile { Title = "Test tile" }] }
                        ],
                    }
                ]
            });
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            var service = new EducationInNumbersContentService(context);
            var result = await service.UpdateSectionHeading(
                _pageId,
                _sectionAId,
                "New Heading");

            var viewModel = result.AssertRight();
            Assert.Equal("New Heading", viewModel.Heading);

            var block = Assert.Single(viewModel.Content);
            var groupBlock = Assert.IsType<EinTileGroupBlockViewModel>(block);
            var tile = Assert.Single(groupBlock.Tiles);
            var freeTextStatTile = Assert.IsType<EinFreeTextStatTileViewModel>(tile);
            Assert.Equal("Test tile", freeTextStatTile.Title);

            var dbSection = await context.EinContentSections.SingleAsync();
            Assert.Equal("New Heading", dbSection.Heading);
        }
    }

    [Fact]
    public async Task UpdateSectionHeading_NotFound()
    {
        var contextId = Guid.NewGuid().ToString();
        await using var context = InMemoryApplicationDbContext(contextId);
        var service = new EducationInNumbersContentService(context);

        var result = await service.UpdateSectionHeading(
            _pageId,
            Guid.NewGuid(),
            "New Heading");
        result.AssertNotFound();
    }

    [Fact]
    public async Task ReorderSections_Success()
    {
        var sectionCId = Guid.NewGuid();
        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            await context.EducationInNumbersPages.AddAsync(new EducationInNumbersPage
            {
                Id = _pageId,
                Content =
                [
                    new EinContentSection
                    {
                        Id = _sectionAId,
                        Order = 0
                    },
                    new EinContentSection
                    {
                        Id = _sectionBId,
                        Order = 1
                    },
                    new EinContentSection
                    {
                        Id = sectionCId,
                        Order = 2
                    },
                ]
            });
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            var service = new EducationInNumbersContentService(context);
            var newOrder = new List<Guid>
            {
                _sectionBId,
                sectionCId,
                _sectionAId
            };
            var result = await service.ReorderSections(_pageId, newOrder);

            var viewModels = result.AssertRight();
            Assert.Equal(3, viewModels.Count);
            Assert.Equal(_sectionBId, viewModels[0].Id);
            Assert.Equal(0, viewModels[0].Order);
            Assert.Equal(sectionCId, viewModels[1].Id);
            Assert.Equal(1, viewModels[1].Order);
            Assert.Equal(_sectionAId, viewModels[2].Id);
            Assert.Equal(2, viewModels[2].Order);

            var sections = await context.EinContentSections.ToListAsync();
            Assert.Equal(0, sections.Single(s => s.Id == _sectionBId).Order);
            Assert.Equal(1, sections.Single(s => s.Id == sectionCId).Order);
            Assert.Equal(2, sections.Single(s => s.Id == _sectionAId).Order);
        }
    }

    [Fact]
    public async Task ReorderSections_PageNotFound()
    {
        var contextId = Guid.NewGuid().ToString();
        await using var context = InMemoryApplicationDbContext(contextId);
        var service = new EducationInNumbersContentService(context);

        var result = await service.ReorderSections(Guid.NewGuid(), new List<Guid>());
        result.AssertNotFound();
    }

    [Fact]
    public async Task ReorderSections_IdsDoNotMatch_ValidationError()
    {
        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            await context.EducationInNumbersPages.AddAsync(new EducationInNumbersPage
            {
                Id = _pageId,
                Content =
                [
                    new EinContentSection
                    {
                        Id = _sectionAId,
                        Order = 0
                    },
                ]
            });
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            var service = new EducationInNumbersContentService(context);
            // Providing a different set of IDs
            var newOrder = new List<Guid>
            {
                _sectionAId,
                _sectionBId
            };
            var result = await service.ReorderSections(_pageId, newOrder);

            var validationResult = result.AssertBadRequestWithValidationProblem();
            validationResult.AssertHasGlobalError(ValidationErrorMessages
                .EinProvidedSectionIdsDifferFromActualSectionIds);
        }
    }

    [Fact]
    public async Task DeleteSection_Success()
    {
        var sectionCId = Guid.NewGuid();
        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            await context.EducationInNumbersPages.AddAsync(new EducationInNumbersPage
            {
                Id = _pageId,
                Content =
                [
                    new EinContentSection
                    {
                        Id = _sectionAId,
                        Order = 0
                    },
                    new EinContentSection
                    {
                        Id = _sectionBId,
                        Order = 1
                    },
                    new EinContentSection
                    {
                        Id = sectionCId,
                        Order = 2
                    },
                ]
            });
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            var service = new EducationInNumbersContentService(context);
            var result = await service.DeleteSection(_pageId, _sectionBId);

            var viewModels = result.AssertRight();
            Assert.Equal(2, viewModels.Count);
            Assert.DoesNotContain(viewModels, vm => vm.Id == _sectionBId);

            // Assert remaining sections are re-ordered
            Assert.Equal(_sectionAId, viewModels[0].Id);
            Assert.Equal(0, viewModels[0].Order);
            Assert.Equal(sectionCId, viewModels[1].Id);
            Assert.Equal(1, viewModels[1].Order);

            var sections = await context.EinContentSections.ToListAsync();
            Assert.Equal(2, sections.Count);
            Assert.Equal(0, sections.Single(s => s.Id == _sectionAId).Order);
            Assert.Equal(1, sections.Single(s => s.Id == sectionCId).Order);

            // NOTE: Blocks and Tiles are deleted by DB cascade delete, so cannot be tested with inmemorydb.
        }
    }

    [Fact]
    public async Task DeleteSection_SectionNotFound()
    {
        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            await context.EducationInNumbersPages.AddAsync(new EducationInNumbersPage { Id = _pageId });
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            var service = new EducationInNumbersContentService(context);
            var result = await service.DeleteSection(_pageId, Guid.NewGuid());
            result.AssertNotFound();
        }
    }

    [Fact]
    public async Task AddBlock_HtmlBlock_OrderProvided_Success()
    {
        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            await context.EinContentSections.AddAsync(new EinContentSection
            {
                Id = _sectionAId,
                EducationInNumbersPageId = _pageId
            });
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            var service = new EducationInNumbersContentService(context);
            var result = await service.AddBlock(
                _pageId, _sectionAId, EinBlockType.HtmlBlock, 2);

            var viewModel = result.AssertRight();
            Assert.IsType<EinHtmlBlockViewModel>(viewModel);
            Assert.Equal(2, viewModel.Order);

            var dbBlock = await context.EinContentBlocks.SingleAsync();
            Assert.Equal(viewModel.Id, dbBlock.Id);
            Assert.Equal(2, dbBlock.Order);
            Assert.Equal(_sectionAId, dbBlock.EinContentSectionId);
        }
    }

    [Fact]
    public async Task AddBlock_HtmlBlock_OrderNotProvided_Success()
    {
        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            await context.EinContentSections.AddAsync(new EinContentSection
            {
                Id = _sectionAId,
                EducationInNumbersPageId = _pageId,
                Content =
                [
                    new EinHtmlBlock
                    {
                        Id = _blockAId,
                        Order = 0
                    }
                ]
            });
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            var service = new EducationInNumbersContentService(context);
            // order is null, so should be added to the end
            var result = await service.AddBlock(
                _pageId, _sectionAId, EinBlockType.HtmlBlock, null);

            var viewModel = result.AssertRight();
            Assert.Equal(1, viewModel.Order);

            var dbBlocks = await context.EinContentBlocks.ToListAsync();
            Assert.Equal(2, dbBlocks.Count);

            var htmlBlock = dbBlocks.Single(b => b.Id == viewModel.Id);
            Assert.IsType<EinHtmlBlock>(htmlBlock);
            Assert.Equal(1, htmlBlock.Order);
        }
    }

    [Fact]
    public async Task AddBlock_TileGroupBlock_OrderNotProvided_Success()
    {
        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            await context.EinContentSections.AddAsync(new EinContentSection
            {
                Id = _sectionAId,
                EducationInNumbersPageId = _pageId,
                Content =
                [
                    new EinHtmlBlock
                    {
                        Id = _blockAId,
                        Order = 0
                    }
                ]
            });
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            var service = new EducationInNumbersContentService(context);
            // order is null, so should be added to the end
            var result = await service.AddBlock(
                _pageId, _sectionAId, EinBlockType.TileGroupBlock, null);

            var viewModel = result.AssertRight();
            Assert.Equal(1, viewModel.Order);

            var dbBlocks = await context.EinContentBlocks.ToListAsync();
            Assert.Equal(2, dbBlocks.Count);

            var tileGroupBlock = dbBlocks.Single(b => b.Id == viewModel.Id);
            Assert.IsType<EinTileGroupBlock>(tileGroupBlock);
            Assert.Equal(1, tileGroupBlock.Order);
        }
    }

    [Fact]
    public async Task AddBlock_HtmlBlock_OrderProvidedClashesWithExistingBlock_Success()
    {
        var preExistingBlockId = Guid.NewGuid();
        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            await context.EinContentSections.AddAsync(new EinContentSection
            {
                Id = _sectionAId,
                EducationInNumbersPageId = _pageId,
                Content =
                [
                    new EinHtmlBlock
                    {
                        Id = preExistingBlockId,
                        Order = 0,
                        Body = "PreExisting block",
                    }
                ]
            });
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            var service = new EducationInNumbersContentService(context);
            var result = await service.AddBlock(
                _pageId, _sectionAId, EinBlockType.HtmlBlock, 0);

            var viewModel = result.AssertRight();
            Assert.IsType<EinHtmlBlockViewModel>(viewModel);
            Assert.Equal(0, viewModel.Order);

            var newBlockId = viewModel.Id;

            var dbBlockList = await context.EinContentBlocks.ToListAsync();
            Assert.Equal(2, dbBlockList.Count);

            var newBlock = dbBlockList
                .OfType<EinHtmlBlock>()
                .Single(block => block.Id == newBlockId);
            Assert.Equal(0, newBlock.Order);
            Assert.Equal(_sectionAId, newBlock.EinContentSectionId);

            var preExistingBlock = dbBlockList
                .OfType<EinHtmlBlock>()
                .Single(block => block.Id == preExistingBlockId);
            Assert.Equal(1, preExistingBlock.Order);
            Assert.Equal("PreExisting block", preExistingBlock.Body);
        }
    }

    [Fact]
    public async Task UpdateHtmlBlock_Success()
    {
        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            await context.EinContentSections.AddAsync(new EinContentSection
            {
                Id = _sectionAId,
                EducationInNumbersPageId = _pageId,
                Content =
                [
                    new EinHtmlBlock
                    {
                        Id = _blockAId,
                        Body = "Old body"
                    }
                ]
            });
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            var service = new EducationInNumbersContentService(context);
            var request = new EinHtmlBlockUpdateRequest { Body = "New body" };
            var result = await service.UpdateHtmlBlock(_pageId, _sectionAId, _blockAId, request);

            var viewModel = result.AssertRight() as EinHtmlBlockViewModel;
            Assert.NotNull(viewModel);
            Assert.Equal(_blockAId, viewModel.Id);
            Assert.Equal("New body", viewModel.Body);

            var dbBlock = await context.EinContentBlocks.OfType<EinHtmlBlock>().SingleAsync();
            Assert.Equal(_blockAId, dbBlock.Id);
            Assert.Equal("New body", dbBlock.Body);
        }
    }

    [Fact]
    public async Task UpdateHtmlBlock_NotFound()
    {
        var contextId = Guid.NewGuid().ToString();
        await using var context = InMemoryApplicationDbContext(contextId);
        var service = new EducationInNumbersContentService(context);
        var request = new EinHtmlBlockUpdateRequest { Body = "New body" };

        var result = await service.UpdateHtmlBlock(_pageId, _sectionAId, Guid.NewGuid(), request);
        result.AssertNotFound();
    }

    [Fact]
    public async Task UpdateTileGroupBlock_Success()
    {
        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            await context.EinContentBlocks.AddAsync(new EinTileGroupBlock
            {
                Id = _blockAId,
                Title = "Old title",
                EinContentSection = new EinContentSection
                {
                    Id = _sectionAId,
                    EducationInNumbersPageId = _pageId
                },
                Tiles = [],
            });
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            var service = new EducationInNumbersContentService(context);
            var request = new EinTileGroupBlockUpdateRequest { Title = "New title" };
            var result = await service.UpdateTileGroupBlock(
                _pageId, _sectionAId, _blockAId, request);

            var viewModel = result.AssertRight();
            var tileGroupBlock = Assert.IsType<EinTileGroupBlockViewModel>(viewModel);
            Assert.Equal(_blockAId, tileGroupBlock.Id);
            Assert.Equal("New title", tileGroupBlock.Title);

            var dbBlock = await context.EinContentBlocks.OfType<EinTileGroupBlock>().SingleAsync();
            Assert.Equal(_blockAId, dbBlock.Id);
            Assert.Equal("New title", dbBlock.Title);
        }
    }

    [Fact]
    public async Task UpdateTileGroupBlock_NotFound()
    {
        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            context.EinContentSections.Add(new EinContentSection
            {
                Id = _sectionAId,
            });
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            var service = new EducationInNumbersContentService(context);
            var request = new EinTileGroupBlockUpdateRequest { Title = "New title" };

            var result = await service.UpdateTileGroupBlock(
                _pageId, _sectionAId, Guid.NewGuid(), request);
            result.AssertNotFound();
        }
    }

    [Fact]
    public async Task ReorderBlocks_Success()
    {
        var blockCId = Guid.NewGuid();
        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            await context.EinContentSections.AddAsync(new EinContentSection
            {
                Id = _sectionAId,
                EducationInNumbersPageId = _pageId,
                Content =
                [
                    new EinHtmlBlock
                    {
                        Id = _blockAId,
                        Order = 0
                    },
                    new EinHtmlBlock
                    {
                        Id = _blockBId,
                        Order = 1
                    },
                    new EinHtmlBlock
                    {
                        Id = blockCId,
                        Order = 2
                    }
                ]
            });
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            var service = new EducationInNumbersContentService(context);
            var newOrder = new List<Guid>
            {
                _blockBId,
                blockCId,
                _blockAId
            };
            var result = await service.ReorderBlocks(_pageId, _sectionAId, newOrder);

            var viewModels = result.AssertRight();
            Assert.Equal(3, viewModels.Count);
            Assert.Equal(_blockBId, viewModels[0].Id);
            Assert.Equal(0, viewModels[0].Order);
            Assert.Equal(blockCId, viewModels[1].Id);
            Assert.Equal(1, viewModels[1].Order);
            Assert.Equal(_blockAId, viewModels[2].Id);
            Assert.Equal(2, viewModels[2].Order);
        }
    }

    [Fact]
    public async Task ReorderBlocks_IdsDoNotMatch_ValidationError()
    {
        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            await context.EinContentSections.AddAsync(new EinContentSection
            {
                Id = _sectionAId,
                EducationInNumbersPageId = _pageId,
                Content =
                [
                    new EinHtmlBlock
                    {
                        Id = _blockAId,
                        Order = 0
                    },
                ]
            });
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            var service = new EducationInNumbersContentService(context);

            var newOrder = new List<Guid>
            {
                _blockAId,
                _blockBId
            }; // _blockBId does not exist in the section
            var result = await service.ReorderBlocks(_pageId, _sectionAId, newOrder);

            var validationResult = result.AssertBadRequestWithValidationProblem();
            validationResult.AssertHasGlobalError(ValidationErrorMessages.EinProvidedBlockIdsDifferFromActualBlockIds);
        }
    }

    [Fact]
    public async Task DeleteBlock_Success()
    {
        var blockCId = Guid.NewGuid();
        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            await context.EinContentSections.AddAsync(new EinContentSection
            {
                Id = _sectionAId,
                EducationInNumbersPageId = _pageId,
                Content =
                [
                    new EinHtmlBlock
                    {
                        Id = _blockAId,
                        Order = 0
                    },
                    new EinHtmlBlock
                    {
                        Id = _blockBId,
                        Order = 1
                    },
                    new EinHtmlBlock
                    {
                        Id = blockCId,
                        Order = 2
                    },
                ]
            });
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            var service = new EducationInNumbersContentService(context);
            var result = await service.DeleteBlock(_pageId, _sectionAId, _blockBId);

            result.AssertRight();

            var section = await context.EinContentSections
                .Include(s => s.Content)
                .SingleAsync();

            Assert.Equal(2, section.Content.Count);
            Assert.DoesNotContain(section.Content, b => b.Id == _blockBId);

            // Assert remaining blocks are reordered
            Assert.Equal(0, section.Content.Single(b => b.Id == _blockAId).Order);
            Assert.Equal(1, section.Content.Single(b => b.Id == blockCId).Order);
        }
    }

    [Fact]
    public async Task DeleteBlock_SectionNotFound()
    {
        var contextId = Guid.NewGuid().ToString();
        await using var context = InMemoryApplicationDbContext(contextId);
        var service = new EducationInNumbersContentService(context);

        var result = await service.DeleteBlock(_pageId, Guid.NewGuid(), _blockAId);
        result.AssertNotFound();
    }

    [Fact]
    public async Task DeleteBlock_BlockNotFound()
    {
        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            await context.EinContentSections.AddAsync(new EinContentSection
            {
                Id = _sectionAId,
                EducationInNumbersPageId = _pageId
            });
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            var service = new EducationInNumbersContentService(context);
            var result = await service.DeleteBlock(_pageId, _sectionAId, Guid.NewGuid());
            result.AssertNotFound();
        }
    }

    [Fact]
    public async Task AddTile_OrderProvided_Success()
    {
        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            await context.EinContentBlocks.AddAsync(new EinTileGroupBlock
            {
                Id = _blockAId,
                EinContentSection = new EinContentSection
                {
                    Id = _sectionAId,
                    EducationInNumbersPageId = _pageId
                }
            });
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            var service = new EducationInNumbersContentService(context);
            var result = await service.AddTile(
                _pageId, _blockAId, EinTileType.FreeTextStatTile, 1);

            var viewModel = result.AssertRight();
            var freeTextTile = Assert.IsType<EinFreeTextStatTileViewModel>(viewModel);
            Assert.Equal(1, freeTextTile.Order);
            Assert.Equal("", freeTextTile.Title);
            Assert.Equal("", freeTextTile.Statistic);

            var dbTile = await context.EinTiles.SingleAsync();
            Assert.Equal(viewModel.Id, dbTile.Id);
            Assert.Equal(1, dbTile.Order);
            Assert.Equal(_blockAId, dbTile.EinParentBlockId);
        }
    }

    [Fact]
    public async Task AddTile_OrderNotProvided_Success()
    {
        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            await context.EinContentBlocks.AddAsync(new EinTileGroupBlock
            {
                Id = _blockAId,
                Tiles =
                [
                    new EinFreeTextStatTile
                    {
                        Id = _tileAId,
                        Order = 0
                    }
                ],
                EinContentSection = new EinContentSection { EducationInNumbersPageId = _pageId }
            });
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            var service = new EducationInNumbersContentService(context);
            // order is null, so should be added to the end of the list
            var result = await service.AddTile(
                _pageId, _blockAId, EinTileType.FreeTextStatTile, null);

            var viewModel = result.AssertRight();
            Assert.Equal(1, viewModel.Order);

            var dbTiles = await context.EinTiles.ToListAsync();
            Assert.Equal(2, dbTiles.Count);
            Assert.Equal(1, dbTiles.Single(b => b.Id == viewModel.Id).Order);
        }
    }

    [Fact]
    public async Task UpdateFreeTextStatTile_Success()
    {
        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            await context.EinContentBlocks.AddAsync(new EinTileGroupBlock
            {
                Id = _blockAId,
                Tiles =
                [
                    new EinFreeTextStatTile
                    {
                        Id = _tileAId,
                        Title = "Old title",
                        Statistic = "Old stat",
                        Trend = "Old trend",
                        LinkUrl = "http://old.url",
                        LinkText = "Old link text",
                    },
                ],
                EinContentSection = new EinContentSection { EducationInNumbersPageId = _pageId }
            });
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            var service = new EducationInNumbersContentService(context);
            var request = new EinFreeTextStatTileUpdateRequest
            {
                Title = "New title",
                Statistic = "New stat",
                Trend = "New trend",
                LinkUrl = "http://new.url",
                LinkText = "New link text",
            };
            var result = await service.UpdateFreeTextStatTile(_pageId, _tileAId, request);

            var viewModel = result.AssertRight();
            var freeTextTile = Assert.IsType<EinFreeTextStatTileViewModel>(viewModel);
            Assert.Equal(_tileAId, freeTextTile.Id);
            Assert.Equal("New title", freeTextTile.Title);
            Assert.Equal("New stat", freeTextTile.Statistic);
            Assert.Equal("New trend", freeTextTile.Trend);
            Assert.Equal("http://new.url", freeTextTile.LinkUrl);
            Assert.Equal("New link text", freeTextTile.LinkText);

            var dbTile = await context.EinTiles
                .OfType<EinFreeTextStatTile>()
                .SingleAsync();

            Assert.Equal(_tileAId, dbTile.Id);
            Assert.Equal("New title", dbTile.Title);
        }
    }

    [Fact]
    public async Task UpdateFreeTextStatTile_NotFound()
    {
        var contextId = Guid.NewGuid().ToString();
        await using var context = InMemoryApplicationDbContext(contextId);
        var service = new EducationInNumbersContentService(context);
        var request = new EinFreeTextStatTileUpdateRequest();

        var result = await service.UpdateFreeTextStatTile(
            _pageId, Guid.NewGuid(), request);
        result.AssertNotFound();
    }

    [Fact]
    public async Task ReorderTiles_Success()
    {
        var tileCId = Guid.NewGuid();
        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            await context.EinContentBlocks.AddAsync(new EinTileGroupBlock
            {
                Id = _blockAId,
                Tiles =
                [
                    new EinFreeTextStatTile
                    {
                        Id = _tileAId,
                        Order = 0
                    },
                    new EinFreeTextStatTile
                    {
                        Id = _tileBId,
                        Order = 1
                    },
                    new EinFreeTextStatTile
                    {
                        Id = tileCId,
                        Order = 2
                    },
                ],
                EinContentSection = new EinContentSection { EducationInNumbersPageId = _pageId }
            });
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            var service = new EducationInNumbersContentService(context);
            var newOrder = new List<Guid>
            {
                _tileBId,
                tileCId,
                _tileAId
            };
            var result = await service.ReorderTiles(_pageId, _blockAId, newOrder);

            var viewModels = result.AssertRight();
            Assert.Equal(3, viewModels.Count);
            Assert.Equal(_tileBId, viewModels[0].Id);
            Assert.Equal(0, viewModels[0].Order);
            Assert.Equal(tileCId, viewModels[1].Id);
            Assert.Equal(1, viewModels[1].Order);
            Assert.Equal(_tileAId, viewModels[2].Id);
            Assert.Equal(2, viewModels[2].Order);

            var tiles = await context.EinTiles.ToListAsync();
            Assert.Equal(0, tiles.Single(s => s.Id == _tileBId).Order);
            Assert.Equal(1, tiles.Single(s => s.Id == tileCId).Order);
            Assert.Equal(2, tiles.Single(s => s.Id == _tileAId).Order);
        }
    }

    [Fact]
    public async Task ReorderTiles_IdsDoNotMatch_ValidationError()
    {
        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            await context.EinContentBlocks.AddAsync(new EinTileGroupBlock
            {
                Id = _blockAId,
                Tiles =
                [
                    new EinFreeTextStatTile
                    {
                        Id = _tileAId,
                        Order = 0
                    },
                ],
                EinContentSection = new EinContentSection { EducationInNumbersPageId = _pageId }
            });
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            var service = new EducationInNumbersContentService(context);

            var newOrder = new List<Guid>
            {
                _tileAId,
                _tileBId
            }; // _tileBId does not exist
            var result = await service.ReorderTiles(_pageId, _blockAId, newOrder);

            var validationResult = result.AssertBadRequestWithValidationProblem();
            validationResult.AssertHasGlobalError(ValidationErrorMessages.EinProvidedTileIdsDifferFromActualTileIds);
        }
    }

    [Fact]
    public async Task DeleteTile_Success()
    {
        var tileCId = Guid.NewGuid();
        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            await context.EinContentBlocks.AddAsync(new EinTileGroupBlock
            {
                Id = _blockAId,
                Tiles =
                [
                    new EinFreeTextStatTile
                    {
                        Id = _tileAId,
                        Order = 0
                    },
                    new EinFreeTextStatTile
                    {
                        Id = _tileBId,
                        Order = 1
                    },
                    new EinFreeTextStatTile
                    {
                        Id = tileCId,
                        Order = 2
                    },
                ],
                EinContentSection = new EinContentSection { EducationInNumbersPageId = _pageId }
            });
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            var service = new EducationInNumbersContentService(context);
            var result = await service.DeleteTile(_pageId, _blockAId, _tileBId);

            result.AssertRight();

            var block = await context.EinContentBlocks
                .OfType<EinTileGroupBlock>()
                .Include(b => b.Tiles)
                .SingleAsync();

            Assert.Equal(2, block.Tiles.Count);
            Assert.DoesNotContain(block.Tiles, t => t.Id == _tileBId);

            // Assert remaining tiles are reordered
            Assert.Equal(0, block.Tiles.Single(b => b.Id == _tileAId).Order);
            Assert.Equal(1, block.Tiles.Single(b => b.Id == tileCId).Order);
        }
    }

    [Fact]
    public async Task DeleteTile_ParentBlockNotFound()
    {
        var contextId = Guid.NewGuid().ToString();
        await using var context = InMemoryApplicationDbContext(contextId);
        var service = new EducationInNumbersContentService(context);

        var result = await service.DeleteTile(_pageId, Guid.NewGuid(), _tileAId);
        result.AssertNotFound();
    }

    [Fact]
    public async Task DeleteTile_TileNotFound()
    {
        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            await context.EinContentBlocks.AddAsync(new EinTileGroupBlock
            {
                Id = _blockAId,
                EinContentSection = new EinContentSection { EducationInNumbersPageId = _pageId }
            });
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            var service = new EducationInNumbersContentService(context);
            var result = await service.DeleteTile(_pageId, _blockAId, Guid.NewGuid());
            result.AssertNotFound();
        }
    }
}
