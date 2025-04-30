using System;
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Requests;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.ViewModels;
using Moq;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Tests.Builders;

public class PublicationServiceMockBuilder
{
    private readonly Mock<IPublicationService> _mock = new(MockBehavior.Strict);
    private List<PublicationSearchResultViewModel>? _listPublications;
    private List<PublicationCacheViewModel> _publications = new();
    public Asserter Assert => new(this);

    public IPublicationService Build()
    {
        _mock
            .Setup(m => m.ListPublications(
                It.IsAny<ReleaseType?>(),
                It.IsAny<Guid?>(),
                It.IsAny<string?>(),
                It.IsAny<PublicationsSortBy?>(),
                It.IsAny<SortDirection?>(),
                It.IsAny<int>(),
                It.IsAny<int>(),
                It.IsAny<IEnumerable<Guid>?>()))
            .ReturnsAsync(new PaginatedListViewModel<PublicationSearchResultViewModel>(
                _listPublications ?? new List<PublicationSearchResultViewModel>(),
                10,
                1,
                10));

        _mock
            .Setup(m => m.Get(It.IsAny<string>()))
            .ReturnsAsync((string slug) => 
                _publications.FirstOrDefault(p => p.Slug == slug)
                ?? new PublicationCacheViewModelBuilder().WithSlug(slug).Build());
        
        return _mock.Object;
    }

    /// <summary>
    /// Set up the search results returned, regardless of the search query
    /// </summary>
    public PublicationServiceMockBuilder WhereListPublicationsReturns(IEnumerable<PublicationSearchResultViewModel> publications)
    {
        _listPublications = publications.ToList();
        return this;
    }

    /// <summary>
    /// Search will return no results, regardless of the search query
    /// </summary>
    /// <returns></returns>
    public PublicationServiceMockBuilder WhereListPublicationsReturnsNoResults()
    {
        _listPublications = new List<PublicationSearchResultViewModel>();
        return this;
    }

    /// <summary>
    /// Setup publications 
    /// </summary>
    /// <param name="publications"></param>
    /// <returns></returns>
    public PublicationServiceMockBuilder WhereGetPublicationReturns(PublicationCacheViewModel publication)
    {
        _publications.Add(publication);
        return this;
    }

    public class Asserter(PublicationServiceMockBuilder builder)
    {
        public void SearchWasFor(Guid? themeId)
        {
            builder._mock
                .Verify(
                    m => m.ListPublications(
                        It.IsAny<ReleaseType?>(),
                        It.Is<Guid?>(t => themeId == null || themeId.Equals(t)),
                        It.IsAny<string?>(),
                        It.IsAny<PublicationsSortBy?>(),
                        It.IsAny<SortDirection?>(),
                        It.IsAny<int>(),
                        It.IsAny<int>(),
                        It.IsAny<IEnumerable<Guid>?>()),
                    Times.Once);
        }
    }
}
