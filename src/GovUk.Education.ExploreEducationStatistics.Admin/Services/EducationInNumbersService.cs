#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Extensions;
using GovUk.Education.ExploreEducationStatistics.Admin.Options;
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
using Microsoft.Extensions.Options;
using static GovUk.Education.ExploreEducationStatistics.Common.Validators.ValidationUtils;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

public class EducationInNumbersService(
    IOptions<AppOptions> appOptions,
    ContentDbContext contentDbContext,
    IUserService userService
) : IEducationInNumbersService
{
    private readonly bool _publishedPageDeletionAllowed = appOptions.Value.EnableEinPublishedPageDeletion;

    public async Task<Either<ActionResult, EinPageVersionSummaryViewModel>> GetPageVersion(Guid pageVersionId)
    {
        return await contentDbContext
            .EinPageVersions.Include(pageVersion => pageVersion.EinPage)
            .SingleOrNotFoundAsync(pageVersion => pageVersion.Id == pageVersionId)
            .OnSuccess(EinPageVersionSummaryViewModel.FromModel);
    }

    public async Task<Either<ActionResult, List<EinPageVersionSummaryWithPrevVersionViewModel>>> ListLatestPages()
    {
        var latestPageVersions = await contentDbContext
            .EinPageVersions.Include(pv => pv.EinPage)
            .Where(pv => pv.Id == pv.EinPage.LatestVersionId)
            .ToListAsync();

        var previousVersionIdLookup = await contentDbContext
            .EinPageVersions.Where(pv => pv.Version == pv.EinPage.LatestVersion!.Version - 1)
            .ToDictionaryAsync(pv => pv.EinPageId, pv => (Guid?)pv.Id);

        return latestPageVersions
            .Select(latestPageVersion =>
            {
                previousVersionIdLookup.TryGetValue(latestPageVersion.EinPageId, out var prevVersionId);
                return EinPageVersionSummaryWithPrevVersionViewModel.FromModel(latestPageVersion, prevVersionId);
            })
            .OrderBy(viewModel => viewModel.Order)
            .ToList();
    }

    public async Task<Either<ActionResult, EinPageVersionSummaryViewModel>> CreatePage(
        CreateEducationInNumbersPageRequest request
    )
    {
        var pageWithTitleAlreadyExists = contentDbContext.EinPages.Any(page => page.Title == request.Title);
        if (pageWithTitleAlreadyExists)
        {
            return new Either<ActionResult, EinPageVersionSummaryViewModel>(
                ValidationResult(ValidationErrorMessages.TitleNotUnique)
            );
        }

        var slug = NamingUtils.SlugFromTitle(request.Title);
        var pageWithSlugAlreadyExists = contentDbContext.EinPages.Any(page => page.Slug == slug);
        if (pageWithSlugAlreadyExists)
        {
            return ValidationResult(ValidationErrorMessages.SlugNotUnique);
        }

        var currentMaxOrder = contentDbContext.EinPages.Select(page => page.Order).Max();

        var createdPageVersion = await contentDbContext.RequireTransaction(async () =>
        {
            var newPage = new EinPage
            {
                Id = Guid.NewGuid(),
                Title = request.Title,
                Slug = slug,
                Description = request.Description,
                Order = currentMaxOrder + 1,
                LatestVersionId = null, // cannot set yet otherwise get a circular dependency
            };

            var newPageVersionId = Guid.NewGuid();
            var newPageVersion = new EinPageVersion
            {
                Id = newPageVersionId,
                EinPageId = newPage.Id,
                EinPage = newPage,
                Version = 0,
                Published = null,
                Created = DateTime.UtcNow,
                CreatedById = userService.GetUserId(),
                Updated = null,
                UpdatedById = null,
            };

            contentDbContext.EinPages.Add(newPage);
            contentDbContext.EinPageVersions.Add(newPageVersion);
            await contentDbContext.SaveChangesAsync();

            newPage.LatestVersionId = newPageVersionId;
            newPage.LatestVersion = newPageVersion;
            await contentDbContext.SaveChangesAsync();

            return newPageVersion;
        });

        return EinPageVersionSummaryViewModel.FromModel(createdPageVersion);
    }

    public async Task<Either<ActionResult, EinPageVersionSummaryViewModel>> CreateAmendment(Guid pageVersionId)
    {
        return await contentDbContext
            .EinPageVersions.Include(pageVersion => pageVersion.EinPage)
            .Include(pageVersion => pageVersion.Content)
                .ThenInclude(section => section.Content)
                    .ThenInclude(block => (block as EinTileGroupBlock)!.Tiles)
            .FirstOrNotFoundAsync(pageVersion => pageVersion.Id == pageVersionId)
            .OnSuccess(async pageVersion =>
            {
                if (pageVersion.Id != pageVersion.EinPage.LatestPublishedVersionId)
                {
                    throw new ArgumentException($"Can only create amendment of latest published page version");
                }

                if (pageVersion.Id != pageVersion.EinPage.LatestVersionId)
                {
                    throw new ArgumentException($"Amendment already exists for page version {pageVersion.Id}");
                }

                var amendment = new EinPageVersion
                {
                    Id = Guid.NewGuid(),
                    EinPageId = pageVersion.EinPageId,
                    EinPage = pageVersion.EinPage,
                    Version = pageVersion.Version + 1,
                    Published = null,
                    Created = DateTime.UtcNow,
                    CreatedById = userService.GetUserId(),
                    Updated = null,
                    UpdatedById = null,
                };
                pageVersion.EinPage.LatestVersionId = amendment.Id;

                var amendmentContent = pageVersion.Content.Select(section => section.Clone(amendment.Id)).ToList();

                contentDbContext.EinPageVersions.Add(amendment);
                contentDbContext.EinContentSections.AddRange(amendmentContent); // includes cloned EinContentBlocks/Tiles

                await contentDbContext.SaveChangesAsync();

                return EinPageVersionSummaryViewModel.FromModel(amendment);
            });
    }

    public async Task<Either<ActionResult, EinPageVersionSummaryViewModel>> UpdatePage(
        Guid pageVersionId,
        UpdateEducationInNumbersPageRequest request
    )
    {
        return await contentDbContext
            .EinPageVersions.Include(page => page.EinPage)
            .FirstOrNotFoundAsync(pageVersion => pageVersion.Id == pageVersionId)
            .OnSuccess(async pageVersion =>
            {
                if (pageVersion.EinPage.LatestPublishedVersionId != null)
                {
                    throw new ArgumentException("Cannot update details of already published page");
                }

                // If the title is being updated, we also update the slug
                string? newSlug = null;
                if (request.Title != null && request.Title != pageVersion.EinPage.Title)
                {
                    var pageWithTitleAlreadyExists = contentDbContext.EinPages.Any(page => page.Title == request.Title);
                    if (pageWithTitleAlreadyExists)
                    {
                        return new Either<ActionResult, EinPageVersionSummaryViewModel>(
                            ValidationResult(ValidationErrorMessages.TitleNotUnique)
                        );
                    }

                    newSlug = NamingUtils.SlugFromTitle(request.Title);
                    var newSlugIsNotUnique = contentDbContext.EinPages.Any(p =>
                        p.Slug == newSlug && p.Id != pageVersion.EinPageId
                    ); // if the slug is for the page we're updating, we don't care
                    if (newSlugIsNotUnique)
                    {
                        return ValidationResult(ValidationErrorMessages.SlugNotUnique);
                    }
                }

                pageVersion.EinPage.Title = request.Title ?? pageVersion.EinPage.Title;
                pageVersion.EinPage.Slug = newSlug ?? pageVersion.EinPage.Slug;
                pageVersion.EinPage.Description = request.Description ?? pageVersion.EinPage.Description;

                await contentDbContext.SaveChangesAsync();

                return EinPageVersionSummaryViewModel.FromModel(pageVersion);
            });
    }

    public async Task<Either<ActionResult, EinPageVersionSummaryViewModel>> PublishPage(Guid pageVersionId)
    {
        return await contentDbContext
            .EinPageVersions.Include(pageVersion => pageVersion.EinPage)
            .FirstOrNotFoundAsync(pageVersion => pageVersion.Id == pageVersionId)
            .OnSuccess(async pageVersion =>
            {
                if (pageVersion.Published != null)
                {
                    throw new ArgumentException("Cannot publish already published page version");
                }

                if (pageVersion.Id != pageVersion.EinPage.LatestVersionId)
                {
                    throw new Exception("An unpublished page should always be the latest page version");
                }

                pageVersion.EinPage.LatestPublishedVersionId = pageVersion.Id;

                pageVersion.Published = DateTime.UtcNow;

                pageVersion.Updated = DateTime.UtcNow;
                pageVersion.UpdatedById = userService.GetUserId();

                await contentDbContext.SaveChangesAsync();

                return EinPageVersionSummaryViewModel.FromModel(pageVersion);
            });
    }

    public async Task<Either<ActionResult, List<EinPageVersionSummaryViewModel>>> Reorder(List<Guid> newOrder)
    {
        var latestPageVersions = await contentDbContext
            .EinPageVersions.Include(pageVersion => pageVersion.EinPage)
            .Where(pageVersion => pageVersion.Id == pageVersion.EinPage.LatestVersionId)
            .ToListAsync();

        if (
            !ComparerUtils.SequencesAreEqualIgnoringOrder(
                newOrder,
                latestPageVersions.Select(pageVersion => pageVersion.Id)
            )
        )
        {
            return ValidationUtils.ValidationActionResult(
                ValidationErrorMessages.EinProvidedPageIdsDifferFromActualPageIds
            );
        }

        newOrder.ForEach(
            (pageVersionId, order) =>
            {
                var matchingPageVersion = latestPageVersions.Single(pageVersion => pageVersion.Id == pageVersionId);
                matchingPageVersion.EinPage.Order = order;
            }
        );
        await contentDbContext.SaveChangesAsync();

        return latestPageVersions.Select(EinPageVersionSummaryViewModel.FromModel).ToList();
    }

    public async Task<Either<ActionResult, Unit>> Delete(Guid pageVersionId)
    {
        return await contentDbContext
            .EinPageVersions.Include(pv => pv.EinPage)
            .SingleOrNotFoundAsync(pageVersion => pageVersion.Id == pageVersionId)
            .OnSuccessVoid(async pageVersion =>
            {
                if (pageVersion.Published != null)
                {
                    throw new ArgumentException("Can only delete unpublished page versions");
                }

                await contentDbContext.RequireTransaction(async () =>
                {
                    // the previous version will be the last published version
                    pageVersion.EinPage.LatestVersionId = pageVersion.EinPage.LatestPublishedVersionId;
                    await contentDbContext.SaveChangesAsync();

                    contentDbContext.EinPageVersions.Remove(pageVersion);

                    if (pageVersion.Version == 0)
                    {
                        contentDbContext.EinPages.Remove(pageVersion.EinPage);
                    }

                    // NOTE: Sections and blocks are cascade deleted by the database, so no worries

                    await contentDbContext.SaveChangesAsync();
                });
            });
    }

    public async Task<Either<ActionResult, Unit>> FullDelete(string slug)
    {
        if (!_publishedPageDeletionAllowed)
        {
            throw new Exception("Full delete not enabled");
        }

        var pagesToRemove = contentDbContext
            .EinPages.Include(page => page.PageVersions)
            .Single(page => page.Slug == slug);

        contentDbContext.EinPages.Remove(pagesToRemove);

        // NOTE: Page versions, content sections and blocks are cascade deleted by the database

        await contentDbContext.SaveChangesAsync();

        return Unit.Instance;
    }
}
