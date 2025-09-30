using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;

public interface IEducationInNumbersService
{
    Task<
        Either<ActionResult, List<EducationInNumbersViewModels.EinNavItemViewModel>>
    > ListEinPages();

    Task<Either<ActionResult, EducationInNumbersViewModels.EinPageViewModel>> GetEinPage(
        string? slug
    );
}
