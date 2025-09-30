using System.Linq.Expressions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.Builders.Releases;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Releases;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Releases.Dtos;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.MockBuilders;

public class ReleaseSearchableDocumentsServiceMockBuilder
{
    private readonly Mock<IReleaseSearchableDocumentsService> _mock = new(MockBehavior.Strict);

    private ReleaseSearchableDocumentDto? _searchableDocument;

    private static readonly Expression<
        Func<
            IReleaseSearchableDocumentsService,
            Task<Either<ActionResult, ReleaseSearchableDocumentDto>>
        >
    > GetLatestReleaseAsSearchableDocument = m =>
        m.GetLatestReleaseAsSearchableDocument(It.IsAny<string>(), It.IsAny<CancellationToken>());

    public ReleaseSearchableDocumentsServiceMockBuilder()
    {
        _mock
            .Setup(GetLatestReleaseAsSearchableDocument)
            .ReturnsAsync(() =>
                _searchableDocument ?? new ReleaseSearchableDocumentDtoBuilder().Build()
            );
    }

    public IReleaseSearchableDocumentsService Build() => _mock.Object;

    public ReleaseSearchableDocumentsServiceMockBuilder WhereHasSearchableDocument(
        ReleaseSearchableDocumentDto searchableDocument
    )
    {
        _searchableDocument = searchableDocument;
        return this;
    }

    public ReleaseSearchableDocumentsServiceMockBuilder WhereGetLatestReleaseAsSearchableDocumentReturnsNotFound()
    {
        _mock
            .Setup(m =>
                m.GetLatestReleaseAsSearchableDocument(
                    It.IsAny<string>(),
                    It.IsAny<CancellationToken>()
                )
            )
            .ReturnsAsync(new NotFoundResult());

        return this;
    }

    public Asserter Assert => new(_mock);

    public class Asserter(Mock<IReleaseSearchableDocumentsService> mock)
    {
        public void GetLatestReleaseAsSearchableDocumentWasCalled(string? publicationSlug = null)
        {
            mock.Verify(
                m =>
                    m.GetLatestReleaseAsSearchableDocument(
                        It.Is<string>(actual =>
                            publicationSlug == null || actual == publicationSlug
                        ),
                        It.IsAny<CancellationToken>()
                    ),
                Times.Once
            );
        }
    }
}
