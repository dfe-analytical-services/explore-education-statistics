using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Search;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Interfaces.Search;

public interface ISearchService
{
    Task<Either<ActionResult, PaginatedListViewModel<PublicationSearchResult>>> SearchPublications(
        int page,
        int pageSize,
        IEnumerable<Guid> publicationIds,
        string? searchText = null,
        CancellationToken cancellationToken = default
    );
}
