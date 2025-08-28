#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;

public interface IEducationInNumbersService
{
    Task<Either<ActionResult, EducationInNumbersSummaryViewModel>> GetPage(Guid id);

    Task<Either<ActionResult, List<EducationInNumbersSummaryWithPrevVersionViewModel>>> ListLatestPages();

    Task<Either<ActionResult, EducationInNumbersSummaryViewModel>> CreatePage(
        CreateEducationInNumbersPageRequest request);

    Task<Either<ActionResult, EducationInNumbersSummaryViewModel>> CreateAmendment(
        Guid id);

    Task<Either<ActionResult, EducationInNumbersSummaryViewModel>> UpdatePage(
        Guid id,
        UpdateEducationInNumbersPageRequest request);

    Task<Either<ActionResult, EducationInNumbersSummaryViewModel>> PublishPage(
        Guid id);

    Task<Either<ActionResult, List<EducationInNumbersSummaryViewModel>>> Reorder(
        List<Guid> newOrder);

    Task<Either<ActionResult, Unit>> Delete(Guid id);
}
