#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Releases.Dtos;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Releases;

public interface IReleaseUpdatesService
{
    Task<Either<ActionResult, List<ReleaseUpdateDto>>> CreateReleaseUpdate(
        Guid releaseVersionId,
        DateTime? date,
        string reason,
        CancellationToken cancellationToken = default
    );

    Task<Either<ActionResult, PaginatedListViewModel<ReleaseUpdateDto>>> GetReleaseUpdates(
        Guid releaseVersionId,
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default
    );

    Task<Either<ActionResult, List<ReleaseUpdateDto>>> UpdateReleaseUpdate(
        Guid releaseVersionId,
        Guid releaseUpdateId,
        DateTime? date,
        string reason,
        CancellationToken cancellationToken = default
    );

    Task<Either<ActionResult, List<ReleaseUpdateDto>>> DeleteReleaseUpdate(
        Guid releaseVersionId,
        Guid releaseUpdateId,
        CancellationToken cancellationToken = default
    );
}
