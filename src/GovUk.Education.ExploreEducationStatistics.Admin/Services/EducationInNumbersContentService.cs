#nullable enable
using System;
using System.Linq;
using System.Threading.Tasks;
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
    public async Task<Either<ActionResult, EducationInNumbersContentViewModel>> GetPageContent(Guid id)
    {
        var page = await contentDbContext.EducationInNumbersPages
            .Include(page => page.Content)
            .ThenInclude(section => section.Content)
            .Where(page => page.Id == id)
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
                            .Select(block => GenerateBlockViewModel(block))
                            .WhereNotNull()
                            .OrderBy(block => block.Order)
                            .ToList()
                    };
                })
                .OrderBy(section => section.Order)
                .ToList()
        };
    }

    private EinContentBlockViewModel? GenerateBlockViewModel(EinContentBlock block)
    {
        return block switch
        {
            EinHtmlBlock htmlBlock => new EinHtmlBlockViewModel
            {
                Id = htmlBlock.Id,
                Order = htmlBlock.Order,
                Type = EinBlockType.HtmlBlock,
                Body = htmlBlock.Body,
            },
            _ => throw new Exception("Ein block type not found")
        };
    }
}
