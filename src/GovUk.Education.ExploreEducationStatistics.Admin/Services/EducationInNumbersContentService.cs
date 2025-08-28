#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Extensions;
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
        var page = await contentDbContext.EducationInNumbersPages
            .Include(page => page.Content)
            .ThenInclude(section => section.Content)
            .Where(page => page.Id == pageId)
            .SingleOrDefaultAsync();

        if (page == null)
        {
            return new NotFoundResult();
        }

        return new EinContentViewModel
        {
            Id = page.Id,
            Title = page.Title,
            Slug = page.Slug,
            Published = page.Published,
            Content = page.Content
                .Select(section =>
                {
                    return new EinContentSectionViewModel
                    {
                        Id = section.Id,
                        Order = section.Order,
                        Heading = section.Heading,
                        Caption = section.Caption,
                        Content = section.Content
                            .Select(block => block.ToViewModel())
                            .OrderBy(block => block.Order)
                            .ToList()
                    };
                })
                .OrderBy(section => section.Order)
                .ToList()
        };
    }

    public async Task<Either<ActionResult, EinContentSectionViewModel>> AddSection(Guid pageId,
        int order)
    {
         var newSection = new EinContentSection
         {
             Id = Guid.NewGuid(),
             EducationInNumbersPageId = pageId,
             Order = order,
             Heading = "New section",
             Caption = null,
             Content = [],
         };

         contentDbContext.EinContentSections.Add(newSection);
         await contentDbContext.SaveChangesAsync();

         return newSection.ToViewModel();
    }

    public async Task<Either<ActionResult, EinContentSectionViewModel>> UpdateSectionHeading(
        Guid pageId,
        Guid sectionId,
        string heading)
    {
        var section = contentDbContext.EinContentSections
            .Include(s => s.Content)
            .SingleOrDefault(section =>
                section.EducationInNumbersPageId == pageId
                && section.Id == sectionId);

        if (section == null)
        {
            return new NotFoundResult();
        }

        section.Heading = heading;
        contentDbContext.EinContentSections.Update(section);
        await contentDbContext.SaveChangesAsync();

        return section.ToViewModel();
    }

    public async Task<Either<ActionResult, List<EinContentSectionViewModel>>> ReorderSections(
        Guid pageId,
        List<Guid> newSectionOrder)
    {
        var page = contentDbContext.EducationInNumbersPages
            .Include(p => p.Content)
            .ThenInclude(s => s.Content)
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
                .ProvidedSectionIdsDifferFromActualSectionIds);
        }

        newSectionOrder.ForEach((sectionId, order) =>
        {
            var matchingSection = sectionList.Single(section => section.Id == sectionId);
            matchingSection.Order = order;
        });

        contentDbContext.EinContentSections.UpdateRange(sectionList);
        await contentDbContext.SaveChangesAsync();

        return sectionList
            .Select(section => section.ToViewModel())
            .OrderBy(section => section.Order)
            .ToList();
    }

    public async Task<Either<ActionResult, List<EinContentSectionViewModel>>> DeleteSection(
        Guid pageId, Guid sectionId)
    {
        var page = contentDbContext.EducationInNumbersPages
            .Include(p => p.Content)
            .ThenInclude(section => section.Content)
            .SingleOrDefault(p => p.Id == pageId);

        if (page == null)
        {
            return new NotFoundResult();
        }

        var pageSections = page.Content;

        var sectionToDelete= pageSections.SingleOrDefault(section => section.Id == sectionId);

        if (sectionToDelete == null)
        {
            return new NotFoundResult();
        }

        pageSections
            .Where(section => section.Order > sectionToDelete.Order)
            .ForEach(section => section.Order--);

        pageSections.Remove(sectionToDelete);

        await contentDbContext.SaveChangesAsync();

        return pageSections
            .Select(section => section.ToViewModel())
            .OrderBy(section => section.Order)
            .ToList();
    }

    public async Task<Either<ActionResult, EinContentBlockViewModel>> AddBlock(
        Guid pageId,
        Guid sectionId,
        EinBlockType type,
        int? order)
    {
        var newOrder = order ?? contentDbContext.EinContentBlocks
            .Count(block =>
                block.EinContentSectionId == sectionId
                && block.EinContentSection.EducationInNumbersPageId == pageId);

        EinContentBlock newBlock = type switch
        {
            EinBlockType.HtmlBlock => new EinHtmlBlock
            {
                Id = Guid.NewGuid(),
                EinContentSectionId = sectionId,
                Order = newOrder,
                Body = "",
            },
            _ => throw new Exception("There is no such block type")
        };

        contentDbContext.EinContentBlocks.Add(newBlock);
        await contentDbContext.SaveChangesAsync();

        return newBlock.ToViewModel();
    }

    public async Task<Either<ActionResult, EinContentBlockViewModel>> UpdateHtmlBlock(
        Guid pageId,
        Guid sectionId,
        Guid blockId,
        EinHtmlBlockUpdateRequest request)
    {
        var blockToUpdate = contentDbContext.EinContentBlocks
            .OfType<EinHtmlBlock>()
            .SingleOrDefault(block => block.Id == blockId
                                      && block.EinContentSectionId == sectionId
                                      && block.EinContentSection.EducationInNumbersPageId == pageId);

        if (blockToUpdate == null)
        {
            return new NotFoundResult();
        }

        blockToUpdate.Body = request.Body;
        contentDbContext.EinContentBlocks.Update(blockToUpdate);
        await contentDbContext.SaveChangesAsync();

        return blockToUpdate.ToViewModel();
    }

    public async Task<Either<ActionResult, List<EinContentBlockViewModel>>> ReorderBlocks(
        Guid pageId,
        Guid sectionId,
        List<Guid> newBlockOrder)
    {
        var section = contentDbContext.EinContentSections
            .Include(p => p.Content)
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
                .ProvidedBlockIdsDifferFromActualBlockIds);
        }

        newBlockOrder.ForEach((blockId, order) =>
        {
            var matchingBlock = blockList.Single(block => block.Id == blockId);
            matchingBlock.Order = order;
        });

        contentDbContext.EinContentBlocks.UpdateRange(blockList);
        await contentDbContext.SaveChangesAsync();

        return blockList
            .Select(block => block.ToViewModel())
            .OrderBy(block => block.Order)
            .ToList();
    }

    public async Task<Either<ActionResult, Unit>> DeleteBlock(
        Guid pageId,
        Guid sectionId,
        Guid blockId)
    {
        var section = contentDbContext.EinContentSections
            .Include(p => p.Content)
            .SingleOrDefault(s => s.Id == sectionId
                && s.EducationInNumbersPageId == pageId);

        if (section == null)
        {
            return new NotFoundResult();
        }

        var blockList = section.Content;

        var blockToDelete= blockList.SingleOrDefault(block => block.Id == blockId);

        if (blockToDelete == null)
        {
            return new NotFoundResult();
        }

        blockList
            .Where(block => block.Order > blockToDelete.Order)
            .ForEach(block => block.Order--);

        blockList.Remove(blockToDelete);

        await contentDbContext.SaveChangesAsync();

        return Unit.Instance;
    }
}
