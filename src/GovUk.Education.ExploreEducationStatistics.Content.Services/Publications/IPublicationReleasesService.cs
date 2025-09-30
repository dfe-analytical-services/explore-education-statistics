using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Publications.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Publications;

public interface IPublicationReleasesService
{
    Task<Either<ActionResult, PaginatedListViewModel<IPublicationReleaseEntryDto>>> GetPublicationReleases(
        string publicationSlug,
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default);
}
