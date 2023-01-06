#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces.ManageContent;

public interface IKeyStatisticService
{
    Task<Either<ActionResult, KeyStatisticDataBlockViewModel>> CreateKeyStatisticDataBlock(
        Guid releaseId, KeyStatisticDataBlockCreateRequest request);

    Task<Either<ActionResult, KeyStatisticDataBlockViewModel>> UpdateKeyStatisticDataBlock(
        Guid releaseId,
        Guid keyStatisticId,
        KeyStatisticDataBlockUpdateRequest request);

    Task<Either<ActionResult, Unit>> Delete(Guid releaseId, Guid keyStatisticId);

    Task DeleteAnyAssociatedWithDataBlock(Guid releaseId, Guid dataBlockId);

    Task<Either<ActionResult, List<KeyStatisticViewModel>>> Reorder(
        Guid releaseId,
        Dictionary<Guid, int> newKeyStatisticOrder);
}
