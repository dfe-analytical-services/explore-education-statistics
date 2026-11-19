using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.CreateDataSetDocuments;
using Moq;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Builders;

public class DataSetDocumentCreatorMockBuilder
{
    private readonly Mock<IDataSetDocumentCreator> _mock = new(MockBehavior.Strict);
    private CreateDataSetDocumentsResponse? _response;
    private Exception? _exception;

    public IDataSetDocumentCreator Build()
    {
        _mock
            .Setup(m =>
                m.CreateReleaseDataSetDocuments(
                    It.IsAny<CreateReleaseDataSetDocumentsRequest>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .Returns(GetResponse);

        _mock
            .Setup(m =>
                m.CreatePublicationDataSetDocuments(
                    It.IsAny<CreatePublicationDataSetDocumentsRequest>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .Returns(GetResponse);

        return _mock.Object;
    }

    private Task<CreateDataSetDocumentsResponse> GetResponse() =>
        _exception is null
            ? Task.FromResult(_response ?? new CreateDataSetDocumentsResponse([]))
            : Task.FromException<CreateDataSetDocumentsResponse>(_exception);

    public DataSetDocumentCreatorMockBuilder WhereResponseIs(CreateDataSetDocumentsResponse response)
    {
        _response = response;
        return this;
    }

    public DataSetDocumentCreatorMockBuilder WhereThrows(Exception exception)
    {
        _exception = exception;
        return this;
    }

    public Asserter Assert => new(_mock);

    public class Asserter(Mock<IDataSetDocumentCreator> mock)
    {
        public void CreateReleaseDataSetDocumentsCalledFor(CreateReleaseDataSetDocumentsRequest expectedRequest)
        {
            mock.Verify(
                m => m.CreateReleaseDataSetDocuments(expectedRequest, It.IsAny<CancellationToken>()),
                Times.Once
            );
        }

        public void CreateReleaseDataSetDocumentsNotCalled()
        {
            mock.Verify(
                m =>
                    m.CreateReleaseDataSetDocuments(
                        It.IsAny<CreateReleaseDataSetDocumentsRequest>(),
                        It.IsAny<CancellationToken>()
                    ),
                Times.Never
            );
        }

        public void CreatePublicationDataSetDocumentsCalledFor(CreatePublicationDataSetDocumentsRequest expectedRequest)
        {
            mock.Verify(
                m => m.CreatePublicationDataSetDocuments(expectedRequest, It.IsAny<CancellationToken>()),
                Times.Once
            );
        }

        public void CreatePublicationDataSetDocumentsNotCalled()
        {
            mock.Verify(
                m =>
                    m.CreatePublicationDataSetDocuments(
                        It.IsAny<CreatePublicationDataSetDocumentsRequest>(),
                        It.IsAny<CancellationToken>()
                    ),
                Times.Never
            );
        }
    }
}
