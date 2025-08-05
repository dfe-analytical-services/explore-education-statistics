#nullable enable
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;

public interface IEducationInNumbersService
{
    Task<Either<ActionResult, EducationInNumbersPageViewModel>> GetPage(
        string? slug,
        bool? published = null);
}
