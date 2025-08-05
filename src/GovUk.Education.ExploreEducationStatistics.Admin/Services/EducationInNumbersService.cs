#nullable enable
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Extensions;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

public class EducationInNumbersService(
    ContentDbContext contentDbContext) : IEducationInNumbersService
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
}
