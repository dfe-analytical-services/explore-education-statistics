using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Releases.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Releases;

public interface IReleaseUpdatesService
{
    Task<Either<ActionResult, PaginatedListViewModel<ReleaseUpdateDto>>> GetReleaseUpdates(
        string publicationSlug,
        string releaseSlug,
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default
    );
}
