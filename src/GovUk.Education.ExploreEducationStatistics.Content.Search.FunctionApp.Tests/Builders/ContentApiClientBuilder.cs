using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Clients.ContentApi;
using Moq;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Builders;

internal class ContentApiClientBuilder
{
    private readonly Mock<IContentApiClient> _mock = new Mock<IContentApiClient>(MockBehavior.Strict);
    private readonly ReleaseSearchViewModelBuilder _releaseSearchViewModelBuilder = new();
    private ReleaseSearchViewModelDto? _releaseSearchViewModel;

    public ContentApiClientBuilder()
    {
        Assert = new Asserter(_mock);
        
        _mock
            .Setup(m => m.GetPublicationLatestReleaseSearchViewModelAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(_releaseSearchViewModel ?? _releaseSearchViewModelBuilder.Build());
    }

    public IContentApiClient Build()
    {
        return _mock.Object;
    }

    public ContentApiClientBuilder WhereReleaseSearchViewModelIs(ReleaseSearchViewModelDto releaseSearchViewModel)
    {
        _releaseSearchViewModel = releaseSearchViewModel;
        return this;
    }
    
    public ContentApiClientBuilder WhereReleaseSearchViewModelIs(Func<ReleaseSearchViewModelBuilder, ReleaseSearchViewModelBuilder> modifyReleaseSearchViewModel)
    {
        modifyReleaseSearchViewModel(_releaseSearchViewModelBuilder);
        return this;
    }

    public Asserter Assert { get; }
    public class Asserter(Mock<IContentApiClient> mock)
    {
        public void ContentWasLoadedFor(string publicationSlug)
        {
            mock.Verify(m => m.GetPublicationLatestReleaseSearchViewModelAsync(publicationSlug, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
