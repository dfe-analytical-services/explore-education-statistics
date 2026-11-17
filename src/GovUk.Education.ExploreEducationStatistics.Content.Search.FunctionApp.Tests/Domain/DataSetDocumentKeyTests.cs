using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Domain;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Domain;

public class DataSetDocumentKeyTests
{
    [Fact]
    public void BlobName_IsPublicationIdThenReleaseIdThenFileId()
    {
        var key = new DataSetDocumentKey
        {
            PublicationId = Guid.Parse("e20a2dd9-98d3-47bc-af5e-4818f3e0e692"),
            ReleaseId = Guid.Parse("0ad33459-9eab-447d-a43e-23ffb680e9e5"),
            FileId = Guid.Parse("be210a75-81fc-4be5-bfd7-dfeb73ef7297"),
        };

        Assert.Equal(
            "e20a2dd9-98d3-47bc-af5e-4818f3e0e692/0ad33459-9eab-447d-a43e-23ffb680e9e5/be210a75-81fc-4be5-bfd7-dfeb73ef7297.json",
            key.BlobName
        );
    }

    [Fact]
    public void BlobNamePrefix_ForPublication_IsPublicationId()
    {
        var prefix = DataSetDocumentKey.BlobNamePrefix(Guid.Parse("e20a2dd9-98d3-47bc-af5e-4818f3e0e692"));

        Assert.Equal("e20a2dd9-98d3-47bc-af5e-4818f3e0e692/", prefix);
    }

    [Fact]
    public void BlobNamePrefix_ForRelease_IsPublicationIdThenReleaseId()
    {
        var prefix = DataSetDocumentKey.BlobNamePrefix(
            Guid.Parse("e20a2dd9-98d3-47bc-af5e-4818f3e0e692"),
            Guid.Parse("0ad33459-9eab-447d-a43e-23ffb680e9e5")
        );

        Assert.Equal("e20a2dd9-98d3-47bc-af5e-4818f3e0e692/0ad33459-9eab-447d-a43e-23ffb680e9e5/", prefix);
    }
}
