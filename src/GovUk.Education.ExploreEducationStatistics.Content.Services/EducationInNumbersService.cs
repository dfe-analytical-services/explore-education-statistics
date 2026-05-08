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
        return await contentDbContext
            .EinPages.Include(page => page.LatestPublishedVersion!.EinPage)
            .Where(page => page.LatestPublishedVersionId != null)
            .OrderBy(page => page.Order)
            .Select(page => EinNavItemViewModel.FromModel(page.LatestPublishedVersion!))
            .ToListAsync(cancellationToken);
    }

    public async Task<Either<ActionResult, EinPageVersionViewModel>> GetEinPage(
        string? slug,
        CancellationToken cancellationToken
    )
    {
        return await contentDbContext
            .EinPages.Include(page => page.LatestPublishedVersion!.EinPage)
            .Include(page => page.LatestPublishedVersion!.Content)
                .ThenInclude(section => section.Content)
                    .ThenInclude(block => (block as EinTileGroupBlock)!.Tiles)
                        .ThenInclude(tile => (tile as EinApiQueryStatTile)!.Release!.Publication)
            .Where(page => page.Slug == slug && page.LatestPublishedVersionId != null)
            .Select(page => EinPageVersionViewModel.FromModel(page.LatestPublishedVersion!))
            .SingleOrNotFoundAsync(cancellationToken: cancellationToken);
    }

    public async Task<Either<ActionResult, List<EinPageSitemapItemViewModel>>> ListSitemapItems(
        CancellationToken cancellationToken
    )
    {
        return await contentDbContext
            .EinPages.Include(page => page.LatestPublishedVersion!.EinPage)
            .Where(page => page.LatestPublishedVersionId != null)
            .OrderBy(page => page.Order)
            .Select(page => EinPageSitemapItemViewModel.FromModel(page.LatestPublishedVersion!))
            .ToListAsync(cancellationToken);
    }
}
