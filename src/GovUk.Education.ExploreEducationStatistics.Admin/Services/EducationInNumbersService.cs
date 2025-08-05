#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Extensions;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
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

public class EducationInNumbersService(
    ContentDbContext contentDbContext,
    IUserService userService) : IEducationInNumbersService
{
    public async Task<Either<ActionResult, EducationInNumbersPageViewModel>> GetPage( // @MarkFix tests?
        string? slug,
        bool? published = null)
    {
        return await contentDbContext.EducationInNumbersPages
            .Where(page =>
                page.Slug == slug
                && (published == null || page.Published.HasValue == published ))
            .OrderByDescending(page => page.Version) // @MarkFix descending correct?
            .FirstOrNotFoundAsync()
            .OnSuccess(page => page.ToViewModel());
    }

    public async Task<Either<ActionResult, List<EducationInNumbersPageViewModel>>> ListLatestPages() // @MarkFix tests?
    {
        return await contentDbContext.EducationInNumbersPages
            .AsNoTracking()
            .GroupBy(page => page.Slug)
            .Select(group => group
                .OrderByDescending(p => p.Version)
                .First())
            .Select(page => page.ToViewModel()) // @MarkFix might want to be a summary view model
            .ToListAsync();
    }

    public async Task<Either<ActionResult, EducationInNumbersPageViewModel>> CreatePage( // @MarkFix tests?
        CreateEducationInNumbersPageRequest request)
    {
        var pageWithSlugAlreadyExists = contentDbContext.EducationInNumbersPages
            .Any(page => page.Slug == request.Slug);

        if (pageWithSlugAlreadyExists)
        {
            // @MarkFix return error

        }

        var currentMaxOrder = contentDbContext.EducationInNumbersPages
            .Select(page => page.Order)
            .Max();

        var newPage = new EducationInNumbersPage
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Slug = request.Slug,
            Description = request.Description,
            Version = 0,
            Order = currentMaxOrder + 1,
            Published = null,
            Created = DateTime.UtcNow(),
            CreatedById = userService.GetUserId(),
            Updated = null,
            UpdatedById = null,
        };

        contentDbContext.EducationInNumbersPages.Add(newPage);
        await contentDbContext.SaveChangesAsync();

        return newPage.ToViewModel();
    }

    public async Task<Either<ActionResult, EducationInNumbersPageViewModel>> UpdatePage( // @MarkFix tests?
        Guid id,
        UpdateEducationInNumbersPageRequest request)
    {
        return await contentDbContext.EducationInNumbersPages
            .FirstOrNotFoundAsync(page => page.Id == id)
            .OnSuccess(async page =>
            {
                if (page.Published != null)
                {
                    throw new ArgumentException("Cannot update already published EiN page"); // @MarkFix exception fine?
                }

                page.Title = request.Title ?? page.Title;
                page.Slug = request.Slug ?? page.Slug;
                page.Description = request.Description ?? page.Description;

                if (request.Publish == true)
                {
                    page.Published = DateTime.UtcNow;
                }

                await contentDbContext.SaveChangesAsync();

                // @MarkFix refresh cache here?

                return page.ToViewModel();
            });
    }
}
