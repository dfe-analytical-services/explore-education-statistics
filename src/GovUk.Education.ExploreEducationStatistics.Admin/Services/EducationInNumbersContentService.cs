#nullable enable
using System;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Extensions;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

public class EducationInNumbersContentService(
    ContentDbContext contentDbContext,
    IUserService userService) : IEducationInNumbersContentService
{
    public async Task<Either<ActionResult, EducationInNumbersContentViewModel>> GetPageContent(Guid pageId)
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

        return new EducationInNumbersContentViewModel
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
             Heading = "New section", // @MarkFix yeah?
             Caption = null, // @MarkFix yeah?
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
}
