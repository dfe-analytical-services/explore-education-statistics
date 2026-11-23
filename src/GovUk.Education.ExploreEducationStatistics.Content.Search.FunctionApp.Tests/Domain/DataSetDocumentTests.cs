using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Domain;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Builders;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Domain;

public class DataSetDocumentTests
{
    [Fact]
    public void GetKey_ReturnsPublicationIdAndReleaseIdAndFileId()
    {
        var publicationId = Guid.NewGuid();
        var releaseId = Guid.NewGuid();
        var fileId = Guid.NewGuid();
        var dataSetDocument = new DataSetDocumentBuilder()
            .WithPublicationId(publicationId)
            .WithReleaseId(releaseId)
            .WithFileId(fileId)
            .Build();

        var key = dataSetDocument.GetKey();

        Assert.Equal(
            new DataSetDocumentKey
            {
                PublicationId = publicationId,
                ReleaseId = releaseId,
                FileId = fileId,
            },
            key
        );
    }

    [Fact]
    public void DocumentsWithEqualValues_AreEqual()
    {
        Assert.Equal(new DataSetDocumentBuilder().Build(), new DataSetDocumentBuilder().Build());
    }
}
