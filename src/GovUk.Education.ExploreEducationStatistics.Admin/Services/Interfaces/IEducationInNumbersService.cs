#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;

public interface IEducationInNumbersService
{
    Task<Either<ActionResult, EinPageVersionSummaryViewModel>> GetPageVersion(Guid pageVersionId);

    Task<Either<ActionResult, List<EinPageVersionSummaryWithPrevVersionViewModel>>> ListLatestPages();

    Task<Either<ActionResult, EinPageVersionSummaryViewModel>> CreatePage(CreateEducationInNumbersPageRequest request);

    Task<Either<ActionResult, EinPageVersionSummaryViewModel>> CreateAmendment(Guid pageVersionId);

    Task<Either<ActionResult, EinPageVersionSummaryViewModel>> UpdatePage(
        Guid pageVersionId,
        UpdateEducationInNumbersPageRequest request
    );

    Task<Either<ActionResult, EinPageVersionSummaryViewModel>> PublishPage(Guid pageVersionId);

    Task<Either<ActionResult, List<EinPageVersionSummaryViewModel>>> Reorder(List<Guid> newOrder);

    Task<Either<ActionResult, Unit>> Delete(Guid id);

    Task<Either<ActionResult, Unit>> FullDelete(string slug);
}
