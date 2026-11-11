using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.RemoveDataSetDocuments;
using Moq;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Builders;

public class DataSetDocumentRemoverMockBuilder
{
    private readonly Mock<IDataSetDocumentRemover> _mock = new(MockBehavior.Strict);
    private RemoveDataSetDocumentsResponse? _response;

    public IDataSetDocumentRemover Build()
    {
        _mock
            .Setup(m =>
                m.RemoveDataSetDocuments(It.IsAny<RemoveDataSetDocumentsRequest>(), It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(() => _response ?? new RemoveDataSetDocumentsResponse([]));

        return _mock.Object;
    }

    public DataSetDocumentRemoverMockBuilder WhereResponseIs(RemoveDataSetDocumentsResponse response)
    {
        _response = response;
        return this;
    }

    public Asserter Assert => new(_mock);

    public class Asserter(Mock<IDataSetDocumentRemover> mock)
    {
        public void RemoveDataSetDocumentsCalledFor(Func<RemoveDataSetDocumentsRequest, bool> whereRequest)
        {
            mock.Verify(
                m =>
                    m.RemoveDataSetDocuments(
                        It.Is<RemoveDataSetDocumentsRequest>(request => whereRequest(request)),
                        It.IsAny<CancellationToken>()
                    ),
                Times.Once
            );
        }

        public void RemoveDataSetDocumentsNotCalled()
        {
            mock.Verify(
                m => m.RemoveDataSetDocuments(It.IsAny<RemoveDataSetDocumentsRequest>(), It.IsAny<CancellationToken>()),
                Times.Never
            );
        }
    }
}
