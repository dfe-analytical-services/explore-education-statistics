using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Releases.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Releases;

public interface IReleasesService
{
    Task<Either<ActionResult, PaginatedListViewModel<IReleaseEntryDto>>> GetPaginatedReleaseEntriesForPublication(
        string publicationSlug,
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default);
}
