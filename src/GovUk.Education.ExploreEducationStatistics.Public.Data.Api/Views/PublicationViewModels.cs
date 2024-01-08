namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Api.Views;

public record PublicationListViewModel(Guid Id, string Title, string Slug);

public record PaginatedPublicationListViewModel(
    List<PublicationListViewModel> Results, 
    int TotalResults, 
    int Page, 
    int PageSize) : PaginatedListViewModel<PublicationListViewModel>(
        Results, 
        TotalResults, 
        Page, 
        PageSize);
