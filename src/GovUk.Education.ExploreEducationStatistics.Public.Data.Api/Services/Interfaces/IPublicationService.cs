using GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Views;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Services.Interfaces;

public interface IPublicationService
{
    Task<PaginatedListViewModel<PublicationListViewModel>> ListPublications(int page, int pageSize, string? search = null);
}
