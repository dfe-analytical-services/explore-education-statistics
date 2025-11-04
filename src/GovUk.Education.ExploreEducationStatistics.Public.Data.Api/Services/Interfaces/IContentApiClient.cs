using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Interfaces;

public interface IContentApiClient
{
    Task<Either<ActionResult, PaginatedListViewModel<PublicationSearchResultViewModel>>> ListPublications(
        int page,
        int pageSize,
        string? search = null,
        IEnumerable<Guid>? publicationIds = null,
        CancellationToken cancellationToken = default
    );

    Task<Either<ActionResult, PublishedPublicationSummaryViewModel>> GetPublication(
        Guid publicationId,
        CancellationToken cancellationToken = default
    );
}
