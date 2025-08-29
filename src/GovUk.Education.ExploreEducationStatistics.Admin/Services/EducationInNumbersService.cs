#nullable enable
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
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using static GovUk.Education.ExploreEducationStatistics.Common.Validators.ValidationUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

public class EducationInNumbersService(
    ContentDbContext contentDbContext,
    IUserService userService) : IEducationInNumbersService
{
    public async Task<Either<ActionResult, EducationInNumbersSummaryViewModel>> GetPage(Guid id)
    {
        return await contentDbContext.EducationInNumbersPages
            .Where(page => page.Id == id)
            .OrderByDescending(page => page.Version)
            .FirstOrNotFoundAsync()
            .OnSuccess(page => page.ToViewModel());
    }

    public async Task<Either<ActionResult, List<EducationInNumbersSummaryWithPrevVersionViewModel>>> ListLatestPages()
    {
        var uniqueSlugs = await contentDbContext.EducationInNumbersPages
            .Select(p => p.Slug)
            .Distinct()
            .ToListAsync();

        var viewModels = new List<EducationInNumbersSummaryWithPrevVersionViewModel>();

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

        return viewModels
            .OrderBy(vm => vm.Order)
            .ToList();
    }

    public async Task<Either<ActionResult, EducationInNumbersSummaryViewModel>> CreatePage(
        CreateEducationInNumbersPageRequest request)
    {
        var pageWithTitleAlreadyExists = contentDbContext.EducationInNumbersPages
            .Any(page => page.Title == request.Title);
        if (pageWithTitleAlreadyExists)
        {
            return new Either<ActionResult, EducationInNumbersSummaryViewModel>(
                ValidationResult(ValidationErrorMessages.TitleNotUnique));
        }

        var slug = NamingUtils.SlugFromTitle(request.Title);
        var pageWithSlugAlreadyExists = contentDbContext.EducationInNumbersPages
            .Any(page => page.Slug == slug);
        if (pageWithSlugAlreadyExists)
        {
            return new Either<ActionResult, EducationInNumbersSummaryViewModel>(
                ValidationResult(ValidationErrorMessages.SlugNotUnique));
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

    public async Task<Either<ActionResult, EducationInNumbersSummaryViewModel>> CreateAmendment(
        Guid id)
    {
        return await contentDbContext.EducationInNumbersPages
            .Include(p => p.Content)
            .ThenInclude(section => section.Content)
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
                    throw new ArgumentException($"Amendment already exists for page {page.Id}");
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

                var amendmentContent = page.Content
                    .Select(section => section.Clone(amendment.Id))
                    .ToList();

                contentDbContext.EducationInNumbersPages.Add(amendment);
                contentDbContext.EinContentSections.AddRange(amendmentContent); // includes cloned EinContentBlocks

                await contentDbContext.SaveChangesAsync();

                return new Either<ActionResult, EducationInNumbersSummaryViewModel>(amendment.ToViewModel());
            });
    }

    public async Task<Either<ActionResult, EducationInNumbersSummaryViewModel>> UpdatePage(
        Guid id,
        UpdateEducationInNumbersPageRequest request)
    {
        return await contentDbContext.EducationInNumbersPages
            .FirstOrNotFoundAsync(page => page.Id == id)
            .OnSuccess(async page =>
            {
                if (page.Published != null)
                {
                    throw new ArgumentException("Cannot update details of already published page");
                }

                if (page.Version > 0)
                {
                    // To prevent slug from being changed by an amendment, as we have no redirects
                    throw new Exception(
                        "Cannot update details for a page amendment");
                }

                // If the title is being updated, we also update the slug
                string? newSlug = null;
                if (request.Title != null && request.Title != page.Title)
                {
                    var pageWithTitleAlreadyExists = contentDbContext.EducationInNumbersPages
                        .Any(p => p.Title == request.Title);
                    if (pageWithTitleAlreadyExists)
                    {
                        return new Either<ActionResult, EducationInNumbersSummaryViewModel>(
                            ValidationResult(ValidationErrorMessages.TitleNotUnique));
                    }

                    newSlug = NamingUtils.SlugFromTitle(request.Title);
                    var newSlugIsNotUnique = contentDbContext.EducationInNumbersPages
                        .Any(p =>
                            p.Slug == newSlug
                            && p.Id != id); // if the slug is for the page we're updating, we don't care
                    if (newSlugIsNotUnique)
                    {
                        return new Either<ActionResult, EducationInNumbersSummaryViewModel>(
                            ValidationResult(ValidationErrorMessages.SlugNotUnique));
                    }
                }

                page.Title = request.Title ?? page.Title;
                page.Slug = newSlug ?? page.Slug;
                page.Description = request.Description ?? page.Description;

                page.Updated = DateTime.UtcNow;
                page.UpdatedById = userService.GetUserId();

                await contentDbContext.SaveChangesAsync();

                return new Either<ActionResult, EducationInNumbersSummaryViewModel>(page.ToViewModel());
            });
    }

    public async Task<Either<ActionResult, EducationInNumbersSummaryViewModel>> PublishPage(
        Guid id)
    {
        return await contentDbContext.EducationInNumbersPages
            .FirstOrNotFoundAsync(page => page.Id == id)
            .OnSuccess(async page =>
            {
                if (page.Published != null)
                {
                    throw new ArgumentException("Cannot publish already published page");
                }

                page.Published = DateTime.UtcNow;

                page.Updated = DateTime.UtcNow;
                page.UpdatedById = userService.GetUserId();

                await contentDbContext.SaveChangesAsync();

                return page.ToViewModel();
            });
    }

    public async Task<Either<ActionResult, List<EducationInNumbersSummaryViewModel>>> Reorder(
        List<Guid> newOrder)
    {
        var pageList = await contentDbContext.EducationInNumbersPages
            .GroupBy(page => page.Slug)
            .Select(group => group
                .OrderByDescending(p => p.Version)
                .First())
            .ToListAsync();

        if (!ComparerUtils.SequencesAreEqualIgnoringOrder(
                newOrder, pageList.Select(page => page.Id)))
        {
            return new Either<ActionResult, List<EducationInNumbersSummaryViewModel>>(
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
                var previousPageVersion = contentDbContext.EducationInNumbersPages
                    .Single(page => page.Slug == matchingPage.Slug
                                    && page.Version + 1 == matchingPage.Version);
                previousPageVersion.Order = order;
                previousPageVersion.Updated = DateTime.UtcNow;
                previousPageVersion.UpdatedById = updatingUserId;
            }
        });

        await contentDbContext.SaveChangesAsync();

        return pageList
            .Select(page => page.ToViewModel())
            .ToList();
    }

    public async Task<Either<ActionResult, Unit>> Delete(Guid id)
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

                // NOTE: Sections and blocks are cascade deleted by the database, so no worries

                await contentDbContext.SaveChangesAsync();
            });
    }
}
