#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;

public interface IEducationInNumbersService
{
    Task<Either<ActionResult, EinSummaryViewModel>> GetPage(Guid id);

    Task<Either<ActionResult, List<EinSummaryWithPrevVersionViewModel>>> ListLatestPages();

    Task<Either<ActionResult, EinSummaryViewModel>> CreatePage(
        CreateEducationInNumbersPageRequest request);

    Task<Either<ActionResult, EinSummaryViewModel>> CreateAmendment(
        Guid id);

    Task<Either<ActionResult, EinSummaryViewModel>> UpdatePage(
        Guid id,
        UpdateEducationInNumbersPageRequest request);

    Task<Either<ActionResult, EinSummaryViewModel>> PublishPage(
        Guid id);

    Task<Either<ActionResult, List<EinSummaryViewModel>>> Reorder(
        List<Guid> newOrder);

    Task<Either<ActionResult, Unit>> Delete(Guid id);
}
