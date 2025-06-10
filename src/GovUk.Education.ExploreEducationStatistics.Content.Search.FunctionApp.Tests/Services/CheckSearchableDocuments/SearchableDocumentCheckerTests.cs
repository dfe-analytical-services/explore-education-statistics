using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Domain;
using GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Services.CheckSearchableDocuments;

namespace GovUk.Education.ExploreEducationStatistics.Content.Search.FunctionApp.Tests.Services.CheckSearchableDocuments;

public class SearchableDocumentCheckerTests
{
    private readonly ReleaseSummaryRetrieverMockBuilder _releaseSummaryRetrieverMockBuilder = new();
    private readonly BlobNameListerMockBuilder _blobNameListerMockBuilder = new();

    private SearchableDocumentChecker GetSut() =>
        new(_blobNameListerMockBuilder.Build(), _releaseSummaryRetrieverMockBuilder.Build());
    
    [Fact]
    public void Can_instantiate_Sut() => Assert.NotNull(GetSut());

    [Fact]
    public async Task GivenCorrectBlobsForReleases_WhenChecked_ThenReportIsEmpty()
    {
        // Arrange
        string[] releaseIds = ["release-1", "release-2", "release-3"];
        var releaseSummaries =
            releaseIds.Select(releaseId => new ReleaseSummary
                {
                    Id = $"id for release {releaseId}",
                    ReleaseId = releaseId,
                    Title = $"title for release {releaseId}",
                    Slug = $"slug for release {releaseId}",
                        
                })
                .ToArray();
        _blobNameListerMockBuilder.WhereBlobNamesReturnedAre(releaseIds);
        _releaseSummaryRetrieverMockBuilder.WhereReleaseSummariesReturnedAre(releaseSummaries);
        
        var sut = GetSut();
        
        // Act
        var report = await sut.RunCheck();

        // Assert
        Assert.NotNull(report);
        Assert.Empty(report.MissingBlobs);
        Assert.Empty(report.ExtraneousBlobs);
        Assert.Equal(releaseIds.Length, report.OkBlobCount);
        Assert.Equal(releaseIds.Length, report.TotalBlobCount);
        Assert.Equal(releaseIds.Length, report.TotalPublicationCount);
        Assert.Empty(report.MissingBlobSummaries);
    }
    
    [Fact]
    public async Task GivenMissingBlobs_WhenChecked_ThenMissingBlobsReported()
    {
        // Arrange
        string[] blobNames = ["release-1", "release-3"];
        string[] releaseIds = ["release-1", "release-2", "release-3"];
        var releaseSummaries =
            releaseIds.Select(releaseId => new ReleaseSummary
                {
                    Id = $"id for release {releaseId}",
                    ReleaseId = releaseId,
                    Title = $"title for release {releaseId}",
                    Slug = $"slug for release {releaseId}",
                        
                })
                .ToArray();
        _blobNameListerMockBuilder.WhereBlobNamesReturnedAre(blobNames);
        _releaseSummaryRetrieverMockBuilder.WhereReleaseSummariesReturnedAre(releaseSummaries);
        
        var sut = GetSut();
        
        // Act
        var report = await sut.RunCheck();

        // Assert
        Assert.NotNull(report);
        Assert.Equal(["release-2"], report.MissingBlobs);
        Assert.Empty(report.ExtraneousBlobs);
        Assert.Equal(2, report.OkBlobCount);
        Assert.Equal(2, report.TotalBlobCount);
        Assert.Equal(3, report.TotalPublicationCount);
        var actualMissingBlobSummary = Assert.Single(report.MissingBlobSummaries);
        var expectedMissingBlobSummary = ReleaseSummaryViewModel.FromModel(releaseSummaries[1]);
        Assert.Equal(expectedMissingBlobSummary, actualMissingBlobSummary);
    }
    
    [Fact]
    public async Task GivenExtraneousBlobs_WhenChecked_ThenExtraBlobsReported()
    {
        // Arrange
        string[] blobNames = ["release-1", "release-2", "release-3", "release-4"];
        string[] releaseIds = ["release-1", "release-2", "release-3"];
        var releaseSummaries =
            releaseIds.Select(releaseId => new ReleaseSummary
                {
                    Id = $"id for release {releaseId}",
                    ReleaseId = releaseId,
                    Title = $"title for release {releaseId}",
                    Slug = $"slug for release {releaseId}",
                        
                })
                .ToArray();
        _blobNameListerMockBuilder.WhereBlobNamesReturnedAre(blobNames);
        _releaseSummaryRetrieverMockBuilder.WhereReleaseSummariesReturnedAre(releaseSummaries);
        
        var sut = GetSut();
        
        // Act
        var report = await sut.RunCheck();

        // Assert
        Assert.NotNull(report);
        Assert.Empty(report.MissingBlobs);
        Assert.Equal(["release-4"], report.ExtraneousBlobs);
        Assert.Equal(3, report.OkBlobCount);
        Assert.Equal(4, report.TotalBlobCount);
        Assert.Equal(3, report.TotalPublicationCount);
        Assert.Empty(report.MissingBlobSummaries);
    }
    
    [Fact]
    public async Task GivenOnlyExtraneousBlobs_WhenChecked_ThenExtraBlobsReported()
    {
        // Arrange
        string[] blobNames = ["release-A", "release-B", "release-C", "release-D"];
        string[] releaseIds = ["release-1", "release-2", "release-3"];
        var releaseSummaries =
            releaseIds.Select(releaseId => new ReleaseSummary
                {
                    Id = $"id for release {releaseId}",
                    ReleaseId = releaseId,
                    Title = $"title for release {releaseId}",
                    Slug = $"slug for release {releaseId}",
                        
                })
                .ToArray();
        _blobNameListerMockBuilder.WhereBlobNamesReturnedAre(blobNames);
        _releaseSummaryRetrieverMockBuilder.WhereReleaseSummariesReturnedAre(releaseSummaries);
        
        var sut = GetSut();
        
        // Act
        var report = await sut.RunCheck();

        // Assert
        Assert.NotNull(report);
        Assert.Equal(releaseIds, report.MissingBlobs);
        Assert.Equal(blobNames, report.ExtraneousBlobs);
        Assert.Equal(0, report.OkBlobCount);
        Assert.Equal(4, report.TotalBlobCount);
        Assert.Equal(3, report.TotalPublicationCount);
        Assert.Equal(3, report.MissingBlobSummaries.Length);
    }
    
    [Fact]
    public async Task GivenBothMissingAndExtraneousBlobs_WhenChecked_ThenAllDiffsReported()
    {
        // Arrange
        string[] blobNames = ["release-1", "release-2", "release-3", "release-4", "release-5"];
        string[] releaseIds = ["release-4", "release-5", "release-6", "release-7", "release-8", "release-9"];
        var releaseSummaries =
            releaseIds.Select(releaseId => new ReleaseSummary
                {
                    Id = $"id for release {releaseId}",
                    ReleaseId = releaseId,
                    Title = $"title for release {releaseId}",
                    Slug = $"slug for release {releaseId}",
                        
                })
                .ToArray();
        _blobNameListerMockBuilder.WhereBlobNamesReturnedAre(blobNames);
        _releaseSummaryRetrieverMockBuilder.WhereReleaseSummariesReturnedAre(releaseSummaries);
        
        var sut = GetSut();
        
        // Act
        var report = await sut.RunCheck();

        // Assert
        Assert.NotNull(report);
        Assert.Equal(["release-6", "release-7", "release-8", "release-9"], report.MissingBlobs);
        Assert.Equal(["release-1", "release-2", "release-3"], report.ExtraneousBlobs);
        Assert.Equal(2, report.OkBlobCount);
        Assert.Equal(5, report.TotalBlobCount);
        Assert.Equal(6, report.TotalPublicationCount);
        Assert.Equal(4, report.MissingBlobSummaries.Length);
    }
}
