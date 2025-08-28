#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.ManageContent;

public interface IKeyStatisticService
{
    Task<Either<ActionResult, KeyStatisticDataBlockViewModel>> CreateKeyStatisticDataBlock(
        Guid releaseVersionId,
        KeyStatisticDataBlockCreateRequest request);

    Task<Either<ActionResult, KeyStatisticTextViewModel>> CreateKeyStatisticText(
        Guid releaseVersionId,
        KeyStatisticTextCreateRequest request);

    Task<Either<ActionResult, KeyStatisticDataBlockViewModel>> UpdateKeyStatisticDataBlock(
        Guid releaseVersionId,
        Guid keyStatisticId,
        KeyStatisticDataBlockUpdateRequest request);

    Task<Either<ActionResult, KeyStatisticTextViewModel>> UpdateKeyStatisticText(
        Guid releaseVersionId,
        Guid keyStatisticId,
        KeyStatisticTextUpdateRequest request);

    Task<Either<ActionResult, Unit>> Delete(Guid releaseVersionId,
        Guid keyStatisticId);

    Task<Either<ActionResult, List<KeyStatisticViewModel>>> Reorder(
        Guid releaseVersionId,
        List<Guid> newOrder);
}
