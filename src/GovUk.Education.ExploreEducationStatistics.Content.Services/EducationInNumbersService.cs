using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static GovUk.Education.ExploreEducationStatistics.Content.ViewModels.EducationInNumbersViewModels;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services;

public class EducationInNumbersService(ContentDbContext contentDbContext) : IEducationInNumbersService
{
    public async Task<Either<ActionResult, List<EinNavItemViewModel>>> ListEinPages()
    {
        var uniqueSlugs = contentDbContext
            .EducationInNumbersPages.Where(page => page.Published != null)
            .Select(page => page.Slug)
            .Distinct()
            .ToList();

        var latestPages = new List<EinNavItemViewModel>();
        foreach (var slug in uniqueSlugs)
        {
            var latestPage = await contentDbContext
                .EducationInNumbersPages.Where(page => page.Slug == slug && page.Published != null)
                .OrderByDescending(page => page.Version)
                .Select(page => EinNavItemViewModel.FromModel(page))
                .FirstAsync();

            latestPages.Add(latestPage);
        }

        return latestPages.OrderBy(page => page.Order).ToList();
    }

    public async Task<Either<ActionResult, EinPageViewModel>> GetEinPage(string? slug)
    {
        return await contentDbContext
            .EducationInNumbersPages.Include(page => page.Content)
            .ThenInclude(section => section.Content)
            .ThenInclude(block => (block as EinTileGroupBlock)!.Tiles)
            .Where(page => page.Slug == slug && page.Published != null)
            .OrderByDescending(page => page.Version)
            .Select(page => EinPageViewModel.FromModel(page))
            .FirstOrNotFoundAsync();
    }

    public async Task<Either<ActionResult, List<EinPageSitemapItemViewModel>>> ListSitemapItems()
    {
        var uniqueSlugs = contentDbContext
            .EducationInNumbersPages.Where(page => page.Published != null)
            .Select(page => page.Slug)
            .Distinct()
            .ToList();

        var latestPages = new List<EinPageSitemapItemViewModel>();
        foreach (var slug in uniqueSlugs)
        {
            var latestPage = await contentDbContext
                .EducationInNumbersPages.Where(page => page.Slug == slug && page.Published != null)
                .OrderByDescending(page => page.Version)
                .Select(page => EinPageSitemapItemViewModel.FromModel(page))
                .FirstAsync();

            latestPages.Add(latestPage);
        }

        return latestPages;
    }
}
