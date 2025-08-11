#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Extensions;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.Validators;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces.Security;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

public class EducationInNumbersService(
    ContentDbContext contentDbContext,
    IUserService userService) : IEducationInNumbersService
{
    public async Task<Either<ActionResult, EducationInNumbersPageViewModel>> GetPage(Guid id) // @MarkFix tests?
    {
        return await contentDbContext.EducationInNumbersPages
            .Where(page => page.Id == id)
            .OrderByDescending(page => page.Version) // @MarkFix descending correct?
            .FirstOrNotFoundAsync()
            .OnSuccess(page => page.ToViewModel());
    }

    public async Task<Either<ActionResult, List<EducationInNumbersPageViewModel>>> ListLatestPages() // @MarkFix tests?
    {
        var uniqueSlugs = await contentDbContext.EducationInNumbersPages
            .Select(p => p.Slug)
            .Distinct()
            .ToListAsync();

        var viewModels = new List<EducationInNumbersPageViewModel>();

        foreach (var slug in uniqueSlugs)
        {
            var latestTwoPages = await contentDbContext.EducationInNumbersPages
                .AsNoTracking()
                .Where(page => page.Slug == slug)
                .OrderByDescending(page => page.Version)
                .Take(2)
                .ToListAsync();

            var latestPage = latestTwoPages[0];
            var previousVersionId = latestTwoPages.Count > 1
                ? (Guid?)latestTwoPages[1].Id
                : null;

            var viewModel = latestPage.ToViewModel(previousVersionId);
            viewModels.Add(viewModel);
        }

        return viewModels;
    }

    public async Task<Either<ActionResult, EducationInNumbersPageViewModel>> CreatePage( // @MarkFix tests?
        CreateEducationInNumbersPageRequest request)
    {
        var slug = NamingUtils.SlugFromTitle(request.Title);

        var pageWithSlugAlreadyExists = contentDbContext.EducationInNumbersPages
            .Any(page => page.Slug == slug);

        if (pageWithSlugAlreadyExists)
        {
            throw new ArgumentException($"Page with slug {slug} already exists"); // @MarkFix should be error
        }

        var currentMaxOrder = contentDbContext.EducationInNumbersPages
            .Select(page => page.Order)
            .Max();

        var newPage = new EducationInNumbersPage
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Slug = slug,
            Description = request.Description,
            Version = 0,
            Order = currentMaxOrder + 1,
            Published = null,
            Created = DateTime.UtcNow,
            CreatedById = userService.GetUserId(),
            Updated = null,
            UpdatedById = null,
        };

        contentDbContext.EducationInNumbersPages.Add(newPage);
        await contentDbContext.SaveChangesAsync();

        return newPage.ToViewModel();
    }

    public async Task<Either<ActionResult, EducationInNumbersPageViewModel>> CreateAmendment( // @MarkFix tests?
        Guid id)
    {
        return await contentDbContext.EducationInNumbersPages
            .FirstOrNotFoundAsync(page => page.Id == id)
            .OnSuccess(async page =>
            {
                if (page.Published == null)
                {
                    throw new ArgumentException("Can only create amendment of a published page");
                }

                var amendmentAlreadyExists = contentDbContext.EducationInNumbersPages
                    .Any(amendment =>
                        amendment.Slug == page.Slug
                        && amendment.Version == page.Version + 1);
                if (amendmentAlreadyExists)
                {
                    throw new ArgumentException($"Amendment already exists for page {page.Id}"); // @MarkFix should be error?
                }

                var amendment = new EducationInNumbersPage
                {
                    Id = Guid.NewGuid(),
                    Title = page.Title,
                    Slug = page.Slug,
                    Description = page.Description,
                    Version = page.Version + 1,
                    Order = page.Order,
                    Published = null,
                    Created = DateTime.UtcNow,
                    CreatedById = userService.GetUserId(),
                    Updated = null,
                    UpdatedById = null,
                };

                contentDbContext.EducationInNumbersPages.Add(amendment);
                await contentDbContext.SaveChangesAsync();

                return new Either<ActionResult, EducationInNumbersPageViewModel>(amendment.ToViewModel());
            });
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

                page.Updated = DateTime.UtcNow;
                page.UpdatedById = userService.GetUserId();

                contentDbContext.EducationInNumbersPages.Update(page);
                await contentDbContext.SaveChangesAsync();

                // @MarkFix refresh cache here?

                return page.ToViewModel();
            });
    }

    public async Task<Either<ActionResult, List<EducationInNumbersPageViewModel>>> Reorder( // @MarkFix tests?
        List<Guid> newOrder)
    {
        var pageList = await contentDbContext.EducationInNumbersPages
            .AsNoTracking()
            .GroupBy(page => page.Slug)
            .Select(group => group
                .OrderByDescending(p => p.Version)
                .First())
            .ToListAsync();

        if (!ComparerUtils.SequencesAreEqualIgnoringOrder(
                newOrder, pageList.Select(page => page.Id)))
        {
            return new Either<ActionResult, List<EducationInNumbersPageViewModel>>(
                ValidationUtils.ValidationActionResult(
                    ValidationErrorMessages.ProvidedPageIdsDifferFromActualPageIds));
        }

        var updatingUserId = userService.GetUserId();

        newOrder.ForEach((pageId, order) =>
        {
            var matchingPage = pageList.Single(page => page.Id == pageId);
            matchingPage.Order = order;
            matchingPage.Updated = DateTime.UtcNow;
            matchingPage.UpdatedById = updatingUserId;

            if (matchingPage is { Version: > 0, Published: null })
            {
                // It is an unpublished amendment, so we must also update the original in case the amendment is cancelled
                var originalPage = contentDbContext.EducationInNumbersPages
                    .Single(page => page.Slug == matchingPage.Slug
                                    && page.Version + 1 == matchingPage.Version);
                originalPage.Order = order;
                originalPage.Updated = DateTime.UtcNow;
                originalPage.UpdatedById = updatingUserId;
            }
        });

        await contentDbContext.SaveChangesAsync();

        // @MarkFix refresh cache here

        return pageList
            .Select(page => page.ToViewModel())
            .ToList();
    }

    public async Task<Either<ActionResult, Unit>> Delete(Guid id) // @MarkFix tests?
    {
        return await contentDbContext.EducationInNumbersPages
            .SingleOrNotFoundAsync(page => page.Id == id)
            .OnSuccessVoid(async page =>
            {
                if (page.Published != null)
                {
                    // we currently only allow the cancellation of unpublished amendments
                    throw new ArgumentException("Cannot delete published page");
                }

                contentDbContext.EducationInNumbersPages.Remove(page);
                await contentDbContext.SaveChangesAsync();

                // @MarkFix refresh cache?
            });
    }
}
