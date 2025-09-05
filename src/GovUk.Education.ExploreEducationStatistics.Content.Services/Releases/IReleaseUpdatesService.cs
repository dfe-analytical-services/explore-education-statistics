using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Releases.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Releases;

public interface IReleaseUpdatesService
{
    Task<Either<ActionResult, PaginatedListViewModel<ReleaseUpdateDto>>> GetPaginatedUpdatesForRelease(
        GetReleaseUpdatesRequest request,
        CancellationToken cancellationToken = default);
}
