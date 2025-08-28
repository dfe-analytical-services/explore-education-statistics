#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Requests;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;

public interface IReleaseApprovalService
{
    Task<Either<ActionResult, Unit>> CreateReleaseStatus(Guid releaseVersionId,
        ReleaseStatusCreateRequest request);

    Task<Either<ActionResult, List<ReleaseStatusViewModel>>> ListReleaseStatuses(Guid releaseVersionId);
}
