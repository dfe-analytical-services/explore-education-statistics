using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Requests;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Publications;
using GovUk.Education.ExploreEducationStatistics.Content.ViewModels;
using Moq;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.MockBuilders;

public class PublicationsSearchServiceMockBuilder
{
    private readonly Mock<IPublicationsSearchService> _mock = new(MockBehavior.Strict);

    private PaginatedListViewModel<PublicationSearchResultViewModel>? _publications;

    public IPublicationsSearchService Build()
    {
        _mock.Setup(m => m.GetPublications(
                It.IsAny<ReleaseType?>(),
                It.IsAny<Guid?>(),
                It.IsAny<string?>(),
                It.IsAny<PublicationsSortBy?>(),
                It.IsAny<SortDirection?>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<IEnumerable<Guid>?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(_publications ?? new PaginatedListViewModel<PublicationSearchResultViewModel>([], 0, 1, 10));

        return _mock.Object;
    }

    public PublicationsSearchServiceMockBuilder WhereHasPublications(
        PaginatedListViewModel<PublicationSearchResultViewModel> publications)
    {
        _publications = publications;
        return this;
    }

    public Asserter Assert => new(_mock);

    public class Asserter(Mock<IPublicationsSearchService> mock)
    {
        public void GetSearchItemsWasCalledForRequest(PublicationsListGetRequest request) =>
            mock.Verify(m => m.GetPublications(
                    It.Is<ReleaseType?>(actual => actual == request.ReleaseType),
                    It.Is<Guid?>(actual => actual == request.ThemeId),
                    It.Is<string?>(actual => actual == request.Search),
                    It.Is<PublicationsSortBy?>(actual => actual == request.Sort),
                    It.Is<SortDirection?>(actual => actual == request.SortDirection),
                    It.Is<int>(actual => actual == request.Page),
                    It.Is<int>(actual => actual == request.PageSize),
                    null,
                    It.IsAny<CancellationToken>()),
                Times.Once);

        public void GetSearchItemsWasCalledForRequest(PublicationsListPostRequest request) =>
            mock.Verify(m => m.GetPublications(
                    It.Is<ReleaseType?>(actual => actual == request.ReleaseType),
                    It.Is<Guid?>(actual => actual == request.ThemeId),
                    It.Is<string?>(actual => actual == request.Search),
                    It.Is<PublicationsSortBy?>(actual => actual == request.Sort),
                    It.Is<SortDirection?>(actual => actual == request.SortDirection),
                    It.Is<int>(actual => actual == request.Page),
                    It.Is<int>(actual => actual == request.PageSize),
                    It.Is<IEnumerable<Guid>?>(actual => actual == request.PublicationIds),
                    It.IsAny<CancellationToken>()),
                Times.Once);

        public void GetSearchItemsWasNotCalledForRequest() =>
            mock.Verify(m => m.GetPublications(
                    It.IsAny<ReleaseType?>(),
                    It.IsAny<Guid?>(),
                    It.IsAny<string?>(),
                    It.IsAny<PublicationsSortBy?>(),
                    It.IsAny<SortDirection?>(),
                    It.IsAny<int>(),
                    It.IsAny<int>(),
                    It.IsAny<IEnumerable<Guid>?>(),
                    It.IsAny<CancellationToken>()),
                Times.Never);
    }
}
