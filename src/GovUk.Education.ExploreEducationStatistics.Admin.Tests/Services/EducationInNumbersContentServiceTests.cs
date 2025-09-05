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
                            new EinHtmlBlock
                            {
                                Id = _blockAId,
                                Order = 0,
                                Body = "Block A Body"
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
            var blockA = Assert.IsType<EinHtmlBlockViewModel>(blockAGenericType);
            Assert.Equal(_blockAId, blockA.Id);
            Assert.Equal(0, blockA.Order);
            Assert.Equal("Block A Body", blockA.Body);

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
                Content = [new EinContentSection { Id = _sectionAId, Heading = "Old Heading" }]
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
                    new EinContentSection { Id = _sectionAId, Order = 0 },
                    new EinContentSection { Id = _sectionBId, Order = 1 },
                    new EinContentSection { Id = sectionCId, Order = 2 },
                ]
            });
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            var service = new EducationInNumbersContentService(context);
            var newOrder = new List<Guid> { _sectionBId, sectionCId, _sectionAId };
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
                    new EinContentSection { Id = _sectionAId, Order = 0 },
                ]
            });
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            var service = new EducationInNumbersContentService(context);
            // Providing a different set of IDs
            var newOrder = new List<Guid> { _sectionAId, _sectionBId };
            var result = await service.ReorderSections(_pageId, newOrder);

            var validationResult = result.AssertBadRequestWithValidationProblem();
            validationResult.AssertHasGlobalError(ValidationErrorMessages.EinProvidedSectionIdsDifferFromActualSectionIds);
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
                    new EinContentSection { Id = _sectionAId, Order = 0 },
                    new EinContentSection { Id = _sectionBId, Order = 1 },
                    new EinContentSection { Id = sectionCId, Order = 2 },
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
    public async Task AddBlock_OrderProvided_Success()
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
    public async Task AddBlock_OrderNotProvided_Success()
    {
        var contextId = Guid.NewGuid().ToString();
        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            await context.EinContentSections.AddAsync(new EinContentSection
            {
                Id = _sectionAId,
                EducationInNumbersPageId = _pageId,
                Content = [new EinHtmlBlock { Id = _blockAId, Order = 0 }]
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
            Assert.Equal(1, dbBlocks.Single(b => b.Id == viewModel.Id).Order);
        }
    }

    [Fact]
    public async Task AddBlock_OrderProvidedClashesWithExistingBlock_Success()
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
                Content = [new EinHtmlBlock { Id = _blockAId, Body = "Old body" }]
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
                    new EinHtmlBlock { Id = _blockAId, Order = 0 },
                    new EinHtmlBlock { Id = _blockBId, Order = 1 },
                    new EinHtmlBlock { Id = blockCId, Order = 2 }
                ]
            });
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            var service = new EducationInNumbersContentService(context);
            var newOrder = new List<Guid> { _blockBId, blockCId, _blockAId };
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
                    new EinHtmlBlock { Id = _blockAId, Order = 0 },
                ]
            });
            await context.SaveChangesAsync();
        }

        await using (var context = InMemoryApplicationDbContext(contextId))
        {
            var service = new EducationInNumbersContentService(context);

            var newOrder = new List<Guid> { _blockAId, _blockBId }; // _blockBId does not exist in the section
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
                    new EinHtmlBlock { Id = _blockAId, Order = 0 },
                    new EinHtmlBlock { Id = _blockBId, Order = 1 },
                    new EinHtmlBlock { Id = blockCId, Order = 2 },
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
}
