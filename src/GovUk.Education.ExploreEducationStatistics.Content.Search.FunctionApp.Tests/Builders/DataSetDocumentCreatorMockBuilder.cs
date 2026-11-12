using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.CreateDataSetDocuments;
using Moq;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Builders;

public class DataSetDocumentCreatorMockBuilder
{
    private readonly Mock<IDataSetDocumentCreator> _mock = new(MockBehavior.Strict);
    private RefreshDataSetDocumentsResponse? _response;

    public IDataSetDocumentCreator Build()
    {
        _mock
            .Setup(m =>
                m.RefreshReleaseDataSetDocuments(
                    It.IsAny<RefreshReleaseDataSetDocumentsRequest>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(() => _response ?? new RefreshDataSetDocumentsResponse([], []));

        _mock
            .Setup(m =>
                m.RefreshPublicationDataSetDocuments(
                    It.IsAny<RefreshPublicationDataSetDocumentsRequest>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(() => _response ?? new RefreshDataSetDocumentsResponse([], []));

        return _mock.Object;
    }

    public DataSetDocumentCreatorMockBuilder WhereResponseIs(RefreshDataSetDocumentsResponse response)
    {
        _response = response;
        return this;
    }

    public Asserter Assert => new(_mock);

    public class Asserter(Mock<IDataSetDocumentCreator> mock)
    {
        public void RefreshReleaseDataSetDocumentsCalledFor(RefreshReleaseDataSetDocumentsRequest expectedRequest)
        {
            mock.Verify(
                m => m.RefreshReleaseDataSetDocuments(expectedRequest, It.IsAny<CancellationToken>()),
                Times.Once
            );
        }

        public void RefreshReleaseDataSetDocumentsNotCalled()
        {
            mock.Verify(
                m =>
                    m.RefreshReleaseDataSetDocuments(
                        It.IsAny<RefreshReleaseDataSetDocumentsRequest>(),
                        It.IsAny<CancellationToken>()
                    ),
                Times.Never
            );
        }

        public void RefreshPublicationDataSetDocumentsCalledFor(
            RefreshPublicationDataSetDocumentsRequest expectedRequest
        )
        {
            mock.Verify(
                m => m.RefreshPublicationDataSetDocuments(expectedRequest, It.IsAny<CancellationToken>()),
                Times.Once
            );
        }

        public void RefreshPublicationDataSetDocumentsNotCalled()
        {
            mock.Verify(
                m =>
                    m.RefreshPublicationDataSetDocuments(
                        It.IsAny<RefreshPublicationDataSetDocumentsRequest>(),
                        It.IsAny<CancellationToken>()
                    ),
                Times.Never
            );
        }
    }
}
