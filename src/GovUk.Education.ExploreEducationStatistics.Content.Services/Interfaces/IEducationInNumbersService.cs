using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;
using static GovUk.Education.ExploreEducationStatistics.Content.ViewModels.EducationInNumbersViewModels;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;

public interface IEducationInNumbersService
{
    Task<Either<ActionResult, List<EinNavItemViewModel>>> ListEinPages();

    Task<Either<ActionResult, EinPageViewModel>> GetEinPage(string? slug);

    Task<Either<ActionResult, List<EinPageSitemapItemViewModel>>> ListSitemapItems();
}
