using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Content.ViewModels.EducationInNumbersViewModels;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services;

public class EducationInNumbersService(ContentDbContext contentDbContext) : IEducationInNumbersService
{
    public async Task<Either<ActionResult, List<EinNavItemViewModel>>> ListEinPages(CancellationToken cancellationToken)
    {
        var uniqueSlugs = await contentDbContext
            .EducationInNumbersPages.Where(page => page.Published != null)
            .Select(page => page.Slug)
            .Distinct()
            .ToListAsync(cancellationToken: cancellationToken);

        return await uniqueSlugs
            .ToAsyncEnumerable()
            .SelectAwait(async slug =>
                await contentDbContext
                    .EducationInNumbersPages.Where(page => page.Slug == slug && page.Published != null)
                    .OrderByDescending(page => page.Version)
                    .Select(page => EinNavItemViewModel.FromModel(page))
                    .FirstAsync(cancellationToken: cancellationToken)
            )
            .OrderBy(navItem => navItem.Order)
            .ToListAsync(cancellationToken: cancellationToken);
    }

    public async Task<Either<ActionResult, EinPageViewModel>> GetEinPage(
        string? slug,
        CancellationToken cancellationToken
    )
    {
        return await contentDbContext
            .EducationInNumbersPages.Include(page => page.Content)
            .ThenInclude(section => section.Content)
            .ThenInclude(block => (block as EinTileGroupBlock)!.Tiles)
            .Where(page => page.Slug == slug && page.Published != null)
            .OrderByDescending(page => page.Version)
            .Select(page => EinPageViewModel.FromModel(page))
            .FirstOrNotFoundAsync(cancellationToken: cancellationToken);
    }

    public async Task<Either<ActionResult, List<EinPageSitemapItemViewModel>>> ListSitemapItems(
        CancellationToken cancellationToken
    )
    {
        var uniqueSlugs = await contentDbContext
            .EducationInNumbersPages.Where(page => page.Published != null)
            .Select(page => page.Slug)
            .Distinct()
            .ToListAsync(cancellationToken: cancellationToken);

        return await uniqueSlugs
            .ToAsyncEnumerable()
            .SelectAwait(async slug =>
                await contentDbContext
                    .EducationInNumbersPages.Where(page => page.Slug == slug && page.Published != null)
                    .OrderByDescending(page => page.Version)
                    .Select(page => EinPageSitemapItemViewModel.FromModel(page))
                    .FirstAsync(cancellationToken: cancellationToken)
            )
            .ToListAsync(cancellationToken: cancellationToken);
    }
}
