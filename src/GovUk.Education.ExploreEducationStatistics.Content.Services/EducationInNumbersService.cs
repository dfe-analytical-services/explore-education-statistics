using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services;

public class EducationInNumbersService(ContentDbContext contentDbContext) : IEducationInNumbersService
{
    public async Task<Either<ActionResult, List<EducationInNumbersViewModels.EinNavItemViewModel>>> ListEinPages()
    {
        var uniqueSlugs = contentDbContext.EducationInNumbersPages
            .Where(page => page.Published != null)
            .Select(page => page.Slug)
            .Distinct()
            .ToList();

        var latestPages = new List<EducationInNumbersViewModels.EinNavItemViewModel>();
        foreach (var slug in uniqueSlugs)
        {
            var latestPage = await contentDbContext.EducationInNumbersPages
                .Where(page => page.Slug == slug && page.Published != null)
                .OrderByDescending(page => page.Version)
                .Select(page => page.ToNavItemViewModel())
                .FirstAsync();

            latestPages.Add(latestPage);
        }

        return latestPages;
    }

    public async Task<Either<ActionResult, EducationInNumbersViewModels.EinPageViewModel>> GetEinPage(string? slug)
    {
        return await contentDbContext.EducationInNumbersPages
            .Include(page => page.Content)
            .ThenInclude(section => section.Content)
            .Where(page => page.Slug == slug && page.Published != null)
            .OrderByDescending(page => page.Order)
            .Select(page => page.ToPageViewModel())
            .FirstOrNotFoundAsync();
    }
}
