#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;

public interface IReleaseService
{
    Task<Either<ActionResult, ReleaseVersionViewModel>> CreateRelease(ReleaseCreateRequest request);

    Task<Either<ActionResult, ReleaseViewModel>> UpdateRelease(
        Guid releaseId, 
        ReleaseUpdateRequest request,
        CancellationToken cancellationToken = default);
}
