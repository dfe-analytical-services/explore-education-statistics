#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;

public interface IDataGuidanceService
{
    public Task<Either<ActionResult, DataGuidanceViewModel>> GetDataGuidance(Guid releaseVersionId,
        CancellationToken cancellationToken = default);

    public Task<Either<ActionResult, DataGuidanceViewModel>> UpdateDataGuidance(Guid releaseVersionId,
        DataGuidanceUpdateRequest request,
        CancellationToken cancellationToken = default);

    public Task<Either<ActionResult, Unit>> ValidateForReleaseChecklist(Guid releaseVersionId,
        CancellationToken cancellationToken = default);
}
