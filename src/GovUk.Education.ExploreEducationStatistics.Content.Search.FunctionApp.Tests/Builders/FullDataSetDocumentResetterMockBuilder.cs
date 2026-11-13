using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Domain;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.ResetSearchableDocuments;
using Moq;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Builders;

public class FullDataSetDocumentResetterMockBuilder
{
    private readonly Mock<IFullDataSetDocumentResetter> _mock = new(MockBehavior.Strict);
    private DataSetDocumentPublication[]? _publications;
    private int _removedBlobCount;

    public IFullDataSetDocumentResetter Build()
    {
        _mock
            .Setup(m => m.PerformReset(It.IsAny<CancellationToken>()))
            .ReturnsAsync(() =>
                new FullDataSetDocumentResetResponse
                {
                    Publications = _publications ?? [],
                    RemovedBlobCount = _removedBlobCount,
                }
            );

        return _mock.Object;
    }

    public FullDataSetDocumentResetterMockBuilder WherePublicationsReturnedAre(
        params DataSetDocumentPublication[] publications
    )
    {
        _publications = publications;
        return this;
    }

    public FullDataSetDocumentResetterMockBuilder WhereRemovedBlobCountIs(int removedBlobCount)
    {
        _removedBlobCount = removedBlobCount;
        return this;
    }

    public Asserter Assert => new(_mock);

    public class Asserter(Mock<IFullDataSetDocumentResetter> mock)
    {
        public void PerformResetWasCalled()
        {
            mock.Verify(m => m.PerformReset(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
