using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Domain;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.ResetSearchableDocuments;
using Moq;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Builders;

public class FullSearchableDocumentResetterMockBuilder
{
    private readonly Mock<IFullSearchableDocumentResetter> _mock = new(MockBehavior.Strict);
    private PublicationInfo[]? _publicationInfos;

    public IFullSearchableDocumentResetter Build()
    {
        _mock
            .Setup(m => m.PerformReset(It.IsAny<CancellationToken>()))
            .ReturnsAsync(() => 
                new PerformResetResponse
                {
                    AllPublications = _publicationInfos ?? []
                });

        return _mock.Object;
    }

    public FullSearchableDocumentResetterMockBuilder WherePublicationsReturnedAre(PublicationInfo[] publications)
    {
        _publicationInfos = publications;
        return this;
    }

    public Asserter Assert => new(_mock);
    public class Asserter(Mock<IFullSearchableDocumentResetter> mock)
    {
        public void PerformResetWasCalled()
        {
            mock.Verify(m => m.PerformReset(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
