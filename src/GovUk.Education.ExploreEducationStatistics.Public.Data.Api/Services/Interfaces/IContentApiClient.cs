using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Interfaces;

public interface IContentApiClient
{
    Task<PaginatedListViewModel<PublicationSearchResultViewModel>> ListPublications(int page, int pageSize, string? search = null, IEnumerable<Guid>? publicationIds = null);
}
