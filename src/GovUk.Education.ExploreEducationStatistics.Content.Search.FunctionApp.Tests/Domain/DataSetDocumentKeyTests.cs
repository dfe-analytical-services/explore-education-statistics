using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Domain;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Domain;

public class DataSetDocumentKeyTests
{
    [Fact]
    public void BlobName_IsPublicationIdThenReleaseIdThenFileId()
    {
        var key = new DataSetDocumentKey
        {
            PublicationId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
            ReleaseId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
            FileId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
        };

        Assert.Equal(
            "11111111-1111-1111-1111-111111111111/22222222-2222-2222-2222-222222222222/33333333-3333-3333-3333-333333333333.json",
            key.BlobName
        );
    }
}
