using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Requests;
using GovUk.Education.ExploreEducationStatistics.Content.ViewModels;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Publications;

public interface IPublicationsSearchService
{
    Task<PaginatedListViewModel<PublicationSearchResultViewModel>> GetPublications(
        ReleaseType? releaseType = null,
        Guid? themeId = null,
        string? search = null,
        PublicationsSortBy? sort = null,
        SortDirection? sortDirection = null,
        int page = 1,
        int pageSize = 10,
        IEnumerable<Guid>? publicationIds = null,
        CancellationToken cancellationToken = default);
}
