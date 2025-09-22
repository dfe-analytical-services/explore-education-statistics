#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Validators;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

public class EducationInNumbersContentService(
    ContentDbContext contentDbContext): IEducationInNumbersContentService
{
    public async Task<Either<ActionResult, EinContentViewModel>> GetPageContent(Guid pageId)
    {
        return await contentDbContext.EducationInNumbersPages
            .Include(page => page.Content)
            .ThenInclude(section => section.Content)
            .ThenInclude(block => (block as EinTileGroupBlock)!.Tiles)
            .Where(page => page.Id == pageId)
            .FirstOrNotFoundAsync()
            .OnSuccess(EinContentViewModel.FromModel);
    }

    public async Task<Either<ActionResult, EinContentSectionViewModel>> AddSection(Guid pageId,
        int order)
    {
        var sectionList = contentDbContext.EinContentSections
            .Where(section => section.EducationInNumbersPageId == pageId)
            .ToList();

        var newSection = new EinContentSection
        {
            Id = Guid.NewGuid(),
            EducationInNumbersPageId = pageId,
            Order = order,
            Heading = "New section",
            Caption = null,
            Content = [],
        };

        sectionList.Where(section => section.Order >= order)
            .ForEach(section => section.Order++);

        contentDbContext.EinContentSections.Add(newSection);
        contentDbContext.EinContentSections.UpdateRange(sectionList);
        await contentDbContext.SaveChangesAsync();

        return EinContentSectionViewModel.FromModel(newSection);
    }

    public async Task<Either<ActionResult, EinContentSectionViewModel>> UpdateSectionHeading(
        Guid pageId,
        Guid sectionId,
        string heading)
    {
        return await contentDbContext.EinContentSections
            .Include(s => s.Content)
            .FirstOrNotFoundAsync(section =>
                section.EducationInNumbersPageId == pageId
                && section.Id == sectionId)
            .OnSuccess(async section =>
            {
                section.Heading = heading;
                contentDbContext.EinContentSections.Update(section);
                await contentDbContext.SaveChangesAsync();

                return EinContentSectionViewModel.FromModel(section);
            });
    }

    public async Task<Either<ActionResult, List<EinContentSectionViewModel>>> ReorderSections(
        Guid pageId,
        List<Guid> newSectionOrder)
    {
        var page = contentDbContext.EducationInNumbersPages
            .Include(p => p.Content)
            .ThenInclude(s => s.Content)
            .ThenInclude(b => (b as EinTileGroupBlock)!.Tiles)
            .SingleOrDefault(p => p.Id == pageId);

        if (page == null)
        {
            return new NotFoundResult();
        }

        var sectionList = page.Content;

        if (!ComparerUtils.SequencesAreEqualIgnoringOrder(
                sectionList.Select(section => section.Id), newSectionOrder))
        {
            return ValidationUtils.ValidationActionResult(ValidationErrorMessages
                .EinProvidedSectionIdsDifferFromActualSectionIds);
        }

        newSectionOrder.ForEach((sectionId, order) =>
        {
            var matchingSection = sectionList.Single(section => section.Id == sectionId);
            matchingSection.Order = order;
        });

        contentDbContext.EinContentSections.UpdateRange(sectionList);
        await contentDbContext.SaveChangesAsync();

        return sectionList
            .Select(EinContentSectionViewModel.FromModel)
            .OrderBy(section => section.Order)
            .ToList();
    }

    public async Task<Either<ActionResult, List<EinContentSectionViewModel>>> DeleteSection(
        Guid pageId, Guid sectionId)
    {
        var page = contentDbContext.EducationInNumbersPages
            .Include(p => p.Content)
            .ThenInclude(section => section.Content)
            .ThenInclude(block => (block as EinTileGroupBlock)!.Tiles)
            .SingleOrDefault(p => p.Id == pageId);

        if (page == null)
        {
            return new NotFoundResult();
        }

        var pageSections = page.Content;

        var sectionToDelete = pageSections.SingleOrDefault(section => section.Id == sectionId);

        if (sectionToDelete == null)
        {
            return new NotFoundResult();
        }

        pageSections.Remove(sectionToDelete);

        pageSections // fix order of remaining sections
            .Where(section => section.Order > sectionToDelete.Order)
            .ForEach(section => section.Order--);

        contentDbContext.EinContentSections.UpdateRange(pageSections);
        await contentDbContext.SaveChangesAsync();

        return pageSections
            .Select(EinContentSectionViewModel.FromModel)
            .OrderBy(section => section.Order)
            .ToList();
    }

    public async Task<Either<ActionResult, EinContentBlockViewModel>> AddBlock(
        Guid pageId,
        Guid sectionId,
        EinBlockType type,
        int? order)
    {
        var blockList = contentDbContext.EinContentBlocks
            .Where(block => block.EinContentSectionId == sectionId)
            .ToList();

        EinContentBlock newBlock = type switch
        {
            EinBlockType.HtmlBlock => new EinHtmlBlock
            {
                Id = Guid.NewGuid(),
                EinContentSectionId = sectionId,
                Order = order ?? blockList.Count,
                Body = "",
            },
            EinBlockType.TileGroupBlock => new EinTileGroupBlock
            {
                Id = Guid.NewGuid(),
                Title = null,
                EinContentSectionId = sectionId,
                Order = order ?? blockList.Count,
                Tiles = [],
            },
            _ => throw new Exception($"{nameof(EinContentBlock)} type {type} not found")
        };

        blockList // fix order of preexisting blocks
            .Where(block => block.Order >= newBlock.Order)
            .ForEach(block => block.Order++);

        contentDbContext.EinContentBlocks.UpdateRange(blockList);
        contentDbContext.EinContentBlocks.Add(newBlock);
        await contentDbContext.SaveChangesAsync();

        return EinContentBlockViewModel.FromModel(newBlock);
    }

    public async Task<Either<ActionResult, EinContentBlockViewModel>> UpdateHtmlBlock(
        Guid pageId,
        Guid sectionId,
        Guid htmlBlockId,
        EinHtmlBlockUpdateRequest request)
    {
        var htmlBlockToUpdate = contentDbContext.EinContentBlocks
            .OfType<EinHtmlBlock>()
            .SingleOrDefault(htmlBlock => htmlBlock.Id == htmlBlockId
                                      && htmlBlock.EinContentSectionId == sectionId
                                      && htmlBlock.EinContentSection.EducationInNumbersPageId == pageId);

        if (htmlBlockToUpdate == null)
        {
            return new NotFoundResult();
        }

        htmlBlockToUpdate.Body = request.Body;
        contentDbContext.EinContentBlocks.Update(htmlBlockToUpdate);
        await contentDbContext.SaveChangesAsync();

        return EinContentBlockViewModel.FromModel(htmlBlockToUpdate);
    }

    public async Task<Either<ActionResult, EinContentBlockViewModel>> UpdateTileGroupBlock(
        Guid pageId,
        Guid sectionId,
        Guid tileGroupBlockId,
        EinTileGroupBlockUpdateRequest request)
    {
        var tileGroupBlockToUpdate = contentDbContext.EinContentBlocks
            .OfType<EinTileGroupBlock>()
            .Include(groupBlock => groupBlock.Tiles)
            .SingleOrDefault(tileGroupBlock => tileGroupBlock.Id == tileGroupBlockId
                                               && tileGroupBlock.EinContentSectionId == sectionId
                                               && tileGroupBlock.EinContentSection.EducationInNumbersPageId == pageId);

        if (tileGroupBlockToUpdate == null)
        {
            return new NotFoundResult();
        }

        tileGroupBlockToUpdate.Title = request.Title;
        contentDbContext.EinContentBlocks.Update(tileGroupBlockToUpdate);
        await contentDbContext.SaveChangesAsync();

        return EinContentBlockViewModel.FromModel(tileGroupBlockToUpdate);
    }

    public async Task<Either<ActionResult, List<EinContentBlockViewModel>>> ReorderBlocks(
        Guid pageId,
        Guid sectionId,
        List<Guid> newBlockOrder)
    {
        var section = contentDbContext.EinContentSections
            .Include(p => p.Content)
            .ThenInclude(block => (block as EinTileGroupBlock)!.Tiles)
            .SingleOrDefault(s => s.Id == sectionId
                && s.EducationInNumbersPageId == pageId);

        if (section == null)
        {
            return new NotFoundResult();
        }

        var blockList = section.Content;

        if (!ComparerUtils.SequencesAreEqualIgnoringOrder(
                blockList.Select(block => block.Id), newBlockOrder))
        {
            return ValidationUtils.ValidationActionResult(ValidationErrorMessages
                .EinProvidedBlockIdsDifferFromActualBlockIds);
        }

        newBlockOrder.ForEach((blockId, order) =>
        {
            var matchingBlock = blockList.Single(block => block.Id == blockId);
            matchingBlock.Order = order;
        });

        contentDbContext.EinContentBlocks.UpdateRange(blockList);
        await contentDbContext.SaveChangesAsync();

        return blockList
            .Select(EinContentBlockViewModel.FromModel)
            .OrderBy(block => block.Order)
            .ToList();
    }

    public async Task<Either<ActionResult, Unit>> DeleteBlock(
        Guid pageId,
        Guid sectionId,
        Guid blockId)
    {
        var section = contentDbContext.EinContentSections
            .Include(section => section.Content)
            .ThenInclude(block => (block as EinTileGroupBlock)!.Tiles)
            .SingleOrDefault(s => s.Id == sectionId
                && s.EducationInNumbersPageId == pageId);

        if (section == null)
        {
            return new NotFoundResult();
        }

        var blockList = section.Content;

        var blockToDelete = blockList.SingleOrDefault(block => block.Id == blockId);

        if (blockToDelete == null)
        {
            return new NotFoundResult();
        }

        blockList.Remove(blockToDelete);

        blockList // fix order of remaining blocks
            .Where(block => block.Order > blockToDelete.Order)
            .ForEach(block => block.Order--);

        await contentDbContext.SaveChangesAsync();

        return Unit.Instance;
    }

    public async Task<Either<ActionResult, EinTileViewModel>> AddTile(
        Guid pageId,
        Guid parentBlockId,
        EinTileType type,
        int? order)
    {
        var tileList = contentDbContext.EinTiles
            .Where(tile => tile.EinParentBlockId == parentBlockId
                           && tile.EinParentBlock.EinContentSection.EducationInNumbersPageId == pageId)
            .ToList();

        EinTile newTile = type switch
        {
            EinTileType.FreeTextStatTile => new EinFreeTextStatTile
            {
                Id = Guid.NewGuid(),
                EinParentBlockId = parentBlockId,
                Order = order ?? tileList.Count,
                Title = "",
                Statistic = "",
                Trend = "",
                LinkUrl = null,
                LinkText = null,
            },
            _ => throw new Exception($"{nameof(EinTile)} type {type} not found")
        };

        tileList // fix order of preexisting tiles
            .Where(tile => tile.Order >= newTile.Order)
            .ForEach(tile => tile.Order++);

        contentDbContext.EinTiles.UpdateRange(tileList);
        contentDbContext.EinTiles.Add(newTile);
        await contentDbContext.SaveChangesAsync();

        return EinTileViewModel.FromModel(newTile);
    }

    public async Task<Either<ActionResult, EinTileViewModel>> UpdateFreeTextStatTile(
        Guid pageId,
        Guid tileId,
        EinFreeTextStatTileUpdateRequest request)
    {
        var tileToUpdate = contentDbContext.EinTiles
            .OfType<EinFreeTextStatTile>()
            .SingleOrDefault(tile => tile.Id == tileId
                                     && tile.EinParentBlock.EinContentSection.EducationInNumbersPageId == pageId);

        if (tileToUpdate == null)
        {
            return new NotFoundResult();
        }

        tileToUpdate.Title = request.Title;
        tileToUpdate.Statistic = request.Statistic;
        tileToUpdate.Trend = request.Trend;
        tileToUpdate.LinkUrl = request.LinkUrl;
        tileToUpdate.LinkText = request.LinkText;

        contentDbContext.EinTiles.Update(tileToUpdate);
        await contentDbContext.SaveChangesAsync();

        return EinTileViewModel.FromModel(tileToUpdate);
    }

    public async Task<Either<ActionResult, List<EinTileViewModel>>> ReorderTiles(
        Guid pageId,
        Guid parentBlockId,
        List<Guid> newTileOrder)
    {
        var parentBlock = contentDbContext.EinContentBlocks
            .OfType<EinTileGroupBlock>()
            .Include(parentBlock => parentBlock.Tiles)
            .SingleOrDefault(parentBlock => parentBlock.Id == parentBlockId
                                            && parentBlock.EinContentSection.EducationInNumbersPageId == pageId);

        if (parentBlock == null)
        {
            return new NotFoundResult();
        }

        var tileList = parentBlock.Tiles;

        if (!ComparerUtils.SequencesAreEqualIgnoringOrder(
                tileList.Select(tile => tile.Id), newTileOrder))
        {
            return ValidationUtils.ValidationActionResult(ValidationErrorMessages
                .EinProvidedTileIdsDifferFromActualTileIds);
        }

        newTileOrder.ForEach((tileId, order) =>
        {
            var matching = tileList.Single(tile => tile.Id == tileId);
            matching.Order = order;
        });

        contentDbContext.EinTiles.UpdateRange(tileList);
        await contentDbContext.SaveChangesAsync();

        return tileList
            .Select(EinTileViewModel.FromModel)
            .OrderBy(tile => tile.Order)
            .ToList();
    }

    public async Task<Either<ActionResult, Unit>> DeleteTile(
        Guid pageId,
        Guid blockId,
        Guid tileId)
    {
        var block = contentDbContext.EinContentBlocks
            .OfType<EinTileGroupBlock>()
            .Include(block => block.Tiles)
            .SingleOrDefault(block => block.Id == blockId
                                      && block.EinContentSection.EducationInNumbersPageId == pageId);

        if (block == null)
        {
            return new NotFoundResult();
        }

        var tileList = block.Tiles;

        var tileToDelete = tileList.SingleOrDefault(tile => tile.Id == tileId);

        if (tileToDelete == null)
        {
            return new NotFoundResult();
        }

        tileList.Remove(tileToDelete);

        tileList // fix order of remaining tiles
            .Where(tile => tile.Order > tileToDelete.Order)
            .ForEach(tile => tile.Order--);

        await contentDbContext.SaveChangesAsync();

        return Unit.Instance;
    }
}
