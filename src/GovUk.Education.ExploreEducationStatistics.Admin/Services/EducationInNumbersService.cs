#nullable enable
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Extensions;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

public class EducationInNumbersService(
    ContentDbContext contentDbContext) : IEducationInNumbersService
{
    public async Task<Either<ActionResult, EducationInNumbersPageViewModel>> GetPage(
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
}
