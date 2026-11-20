using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Domain;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Options;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.CheckSearchableDocuments;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Builders;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Services.CheckSearchableDocuments;

public class DataSetDocumentCheckerTests
{
    private const string DataSetDocumentsContainerName = "dataset-documents-container";

    private readonly ContentApiClientMockBuilder _contentApiClientMockBuilder = new();
    private readonly BlobNameListerMockBuilder _blobNameListerMockBuilder = new();

    private DataSetDocumentChecker GetSut() =>
        new(
            _blobNameListerMockBuilder.Build(),
            () => _contentApiClientMockBuilder.Build(),
            Microsoft.Extensions.Options.Options.Create(
                new AppOptions { DataSetDocumentsContainerName = DataSetDocumentsContainerName }
            )
        );

    private static DataSetDocumentKey[] CreateKeys(int count) =>
        Enumerable
            .Range(0, count)
            .Select(_ => new DataSetDocumentKey
            {
                PublicationId = Guid.NewGuid(),
                ReleaseId = Guid.NewGuid(),
                FileId = Guid.NewGuid(),
            })
            .ToArray();

    private static string[] BlobNamesOf(params DataSetDocumentKey[] keys) => keys.Select(key => key.BlobName).ToArray();

    [Fact]
    public void Can_instantiate_Sut() => Assert.NotNull(GetSut());

    [Fact]
    public async Task WhenChecked_ThenBlobsAreListedForDataSetDocumentsContainer()
    {
        // Arrange
        var sut = GetSut();

        // Act
        await sut.RunCheck();

        // Assert
        _blobNameListerMockBuilder.Assert.BlobsWereListedForContainer(DataSetDocumentsContainerName);
    }

    [Fact]
    public async Task GivenNoBlobsAndNoKeys_WhenChecked_ThenReportIsEmpty()
    {
        // Arrange
        var sut = GetSut();

        // Act
        var report = await sut.RunCheck();

        // Assert
        Assert.Empty(report.MissingBlobs);
        Assert.Empty(report.ExtraneousBlobs);
        Assert.Equal(0, report.OkBlobCount);
        Assert.Equal(0, report.TotalBlobCount);
        Assert.Equal(0, report.TotalExpectedCount);
    }

    [Fact]
    public async Task GivenCorrectBlobsForKeys_WhenChecked_ThenReportIsEmpty()
    {
        // Arrange
        var keys = CreateKeys(3);
        _blobNameListerMockBuilder.WhereBlobNamesReturnedAre(BlobNamesOf(keys));
        _contentApiClientMockBuilder.WhereHasDataSetDocumentKeys(keys);

        var sut = GetSut();

        // Act
        var report = await sut.RunCheck();

        // Assert
        Assert.Empty(report.MissingBlobs);
        Assert.Empty(report.ExtraneousBlobs);
        Assert.Equal(3, report.OkBlobCount);
        Assert.Equal(3, report.TotalBlobCount);
        Assert.Equal(3, report.TotalExpectedCount);
    }

    [Fact]
    public async Task GivenMissingBlobs_WhenChecked_ThenMissingBlobsReported()
    {
        // Arrange
        var keys = CreateKeys(3);
        _blobNameListerMockBuilder.WhereBlobNamesReturnedAre(BlobNamesOf(keys[0], keys[2]));
        _contentApiClientMockBuilder.WhereHasDataSetDocumentKeys(keys);

        var sut = GetSut();

        // Act
        var report = await sut.RunCheck();

        // Assert
        Assert.Equal([keys[1].BlobName], report.MissingBlobs);
        Assert.Empty(report.ExtraneousBlobs);
        Assert.Equal(2, report.OkBlobCount);
        Assert.Equal(2, report.TotalBlobCount);
        Assert.Equal(3, report.TotalExpectedCount);
    }

    [Fact]
    public async Task GivenExtraneousBlobs_WhenChecked_ThenExtraBlobsReported()
    {
        // Arrange
        var keys = CreateKeys(3);
        var extraneousBlobName = CreateKeys(1)[0].BlobName;
        _blobNameListerMockBuilder.WhereBlobNamesReturnedAre([.. BlobNamesOf(keys), extraneousBlobName]);
        _contentApiClientMockBuilder.WhereHasDataSetDocumentKeys(keys);

        var sut = GetSut();

        // Act
        var report = await sut.RunCheck();

        // Assert
        Assert.Empty(report.MissingBlobs);
        Assert.Equal([extraneousBlobName], report.ExtraneousBlobs);
        Assert.Equal(3, report.OkBlobCount);
        Assert.Equal(4, report.TotalBlobCount);
        Assert.Equal(3, report.TotalExpectedCount);
    }

    [Fact]
    public async Task GivenOnlyExtraneousBlobs_WhenChecked_ThenAllDiffsReported()
    {
        // Arrange
        var keys = CreateKeys(3);
        var extraneousBlobNames = BlobNamesOf(CreateKeys(4));
        _blobNameListerMockBuilder.WhereBlobNamesReturnedAre(extraneousBlobNames);
        _contentApiClientMockBuilder.WhereHasDataSetDocumentKeys(keys);

        var sut = GetSut();

        // Act
        var report = await sut.RunCheck();

        // Assert
        Assert.Equal(BlobNamesOf(keys), report.MissingBlobs);
        Assert.Equal(extraneousBlobNames, report.ExtraneousBlobs);
        Assert.Equal(0, report.OkBlobCount);
        Assert.Equal(4, report.TotalBlobCount);
        Assert.Equal(3, report.TotalExpectedCount);
    }

    [Fact]
    public async Task GivenBothMissingAndExtraneousBlobs_WhenChecked_ThenAllDiffsReported()
    {
        // Arrange
        var keys = CreateKeys(4);
        var extraneousBlobNames = BlobNamesOf(CreateKeys(2));
        _blobNameListerMockBuilder.WhereBlobNamesReturnedAre([.. extraneousBlobNames, keys[0].BlobName]);
        _contentApiClientMockBuilder.WhereHasDataSetDocumentKeys(keys);

        var sut = GetSut();

        // Act
        var report = await sut.RunCheck();

        // Assert
        Assert.Equal(BlobNamesOf(keys[1], keys[2], keys[3]), report.MissingBlobs);
        Assert.Equal(extraneousBlobNames, report.ExtraneousBlobs);
        Assert.Equal(1, report.OkBlobCount);
        Assert.Equal(3, report.TotalBlobCount);
        Assert.Equal(4, report.TotalExpectedCount);
    }

    [Fact]
    public async Task GivenBlobForSameFileUnderDifferentRelease_WhenChecked_ThenReportedAsMissingAndExtraneous()
    {
        // Arrange
        var key = CreateKeys(1)[0];
        var misplacedBlobName = (key with { ReleaseId = Guid.NewGuid() }).BlobName;
        _blobNameListerMockBuilder.WhereBlobNamesReturnedAre([misplacedBlobName]);
        _contentApiClientMockBuilder.WhereHasDataSetDocumentKeys(key);

        var sut = GetSut();

        // Act
        var report = await sut.RunCheck();

        // Assert
        Assert.Equal([key.BlobName], report.MissingBlobs);
        Assert.Equal([misplacedBlobName], report.ExtraneousBlobs);
        Assert.Equal(0, report.OkBlobCount);
    }
}
