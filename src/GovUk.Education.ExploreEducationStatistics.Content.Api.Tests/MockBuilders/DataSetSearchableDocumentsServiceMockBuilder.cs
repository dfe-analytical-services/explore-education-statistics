using GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.Builders.DataSets;
using GovUk.Education.ExploreEducationStatistics.Content.Services.DataSets;
using GovUk.Education.ExploreEducationStatistics.Content.Services.DataSets.Dtos;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.MockBuilders;

public class DataSetSearchableDocumentsServiceMockBuilder
{
    private readonly Mock<IDataSetSearchableDocumentsService> _mock = new(MockBehavior.Strict);

    private DataSetSearchableDocumentDto[]? _searchableDocuments;

    public DataSetSearchableDocumentsServiceMockBuilder()
    {
        _mock
            .Setup(m =>
                m.GetReleaseDataSetSearchableDocuments(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(() => _searchableDocuments ?? [new DataSetSearchableDocumentDtoBuilder().Build()]);
    }

    public IDataSetSearchableDocumentsService Build() => _mock.Object;

    public DataSetSearchableDocumentsServiceMockBuilder WhereHasSearchableDocuments(
        params DataSetSearchableDocumentDto[] searchableDocuments
    )
    {
        _searchableDocuments = searchableDocuments;
        return this;
    }

    public DataSetSearchableDocumentsServiceMockBuilder WhereGetReleaseDataSetSearchableDocumentsReturnsNotFound()
    {
        _mock
            .Setup(m =>
                m.GetReleaseDataSetSearchableDocuments(
                    It.IsAny<string>(),
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(new NotFoundResult());

        return this;
    }

    public Asserter Assert => new(_mock);

    public class Asserter(Mock<IDataSetSearchableDocumentsService> mock)
    {
        public void GetReleaseDataSetSearchableDocumentsWasCalled(
            string? publicationSlug = null,
            string? releaseSlug = null
        )
        {
            mock.Verify(
                m =>
                    m.GetReleaseDataSetSearchableDocuments(
                        It.Is<string>(actual => publicationSlug == null || actual == publicationSlug),
                        It.Is<string>(actual => releaseSlug == null || actual == releaseSlug),
                        It.IsAny<CancellationToken>()
                    ),
                Times.Once
            );
        }
    }
}
