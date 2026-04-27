#nullable enable
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.MockBuilders;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Screener;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Screener;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;
using Microsoft.Extensions.Time.Testing;
using Moq;
using static GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services.DbUtils;
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainers;
using static Moq.MockBehavior;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Tests.Services;

public class DataSetUploadRepositoryTests
{
    [Fact]
    public async Task ListAll_Success_ReturnsListOfVms()
    {
        // Arrange
        var releaseVersionId = Guid.NewGuid();
        var timeProvider = new FakeTimeProvider(DateTimeOffset.UtcNow);

        var builder = new DataSetUploadMockBuilder(timeProvider: timeProvider);
        var upload1 = builder.WithReleaseVersionId(releaseVersionId).BuildInitialEntity();
        var upload2 = builder.WithReleaseVersionId(releaseVersionId).WithFailingTests().BuildScreenedEntity();
        var upload3 = builder.WithReleaseVersionId(releaseVersionId).WithWarningTests().BuildScreenedEntity();
        var unrelatedUpload = builder.WithReleaseVersionId(Guid.NewGuid()).BuildScreenedEntity();

        var contextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
        {
            contentDbContext.DataSetUploads.AddRange(upload1, upload2, upload3, unrelatedUpload);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
        {
            var service = BuildService(contentDbContext);

            // Act
            var result = await service.ListAll(releaseVersionId, CancellationToken.None);

            // Assert
            var uploadViewModels = result.AssertRight();

            Assert.Equal(3, uploadViewModels.Count);

            // Assert the basic details are mapped correctly.
            AssertBasicViewModelDetailsCorrect(uploadViewModels[0], upload1);
            AssertBasicViewModelDetailsCorrect(uploadViewModels[1], upload2);
            AssertBasicViewModelDetailsCorrect(uploadViewModels[2], upload3);

            // Assert that a DataSetUpload with no screening is mapped correctly.
            // TODO EES-7003 - we're currently asserting that status is SCREENER_ERROR at the moment without
            // a screening result present. This needs to be updated so that initially after upload, it's OK for
            // there to be no screener result.
            Assert.Equal(nameof(DataSetUploadStatus.SCREENER_ERROR), uploadViewModels[0].Status);

            // Assert it has no screener results or progress yet.
            Assert.False(uploadViewModels[0].PublicApiCompatible);
            Assert.Null(uploadViewModels[0].ScreenerResult);
            Assert.Null(uploadViewModels[0].ScreenerProgress);

            // Assert that a DataSetUpload with a failed screening result is mapped correctly.
            Assert.Equal(nameof(DataSetUploadStatus.FAILED_SCREENING), uploadViewModels[1].Status);

            // Assert its screening progress is mapped correctly.
            Assert.Equal(
                upload2.ScreenerProgress!.PercentageComplete,
                uploadViewModels[1].ScreenerProgress!.PercentageComplete
            );
            Assert.Equal(upload2.ScreenerProgress!.Stage, uploadViewModels[1].ScreenerProgress!.Stage);

            // Assert that a DataSetUpload with a warning screening result is mapped correctly.
            Assert.Equal(nameof(DataSetUploadStatus.PENDING_REVIEW), uploadViewModels[2].Status);
            Assert.Equal(upload3.ScreenerResult!.PublicApiCompatible, uploadViewModels[2].PublicApiCompatible);
            Assert.Equal(upload3.ScreenerResult!.OverallResult, uploadViewModels[2].ScreenerResult!.OverallResult);
            AssertScreenerTestsCorrect(
                upload3.ScreenerResult!.TestResults,
                uploadViewModels[2].ScreenerResult!.TestResults
            );

            // Assert its screening progress is mapped correctly.
            Assert.Equal(
                upload3.ScreenerProgress!.PercentageComplete,
                uploadViewModels[2].ScreenerProgress!.PercentageComplete
            );
            Assert.Equal(upload3.ScreenerProgress!.Stage, uploadViewModels[2].ScreenerProgress!.Stage);
        }
    }

    [Fact]
    public async Task Delete_Success_ObjectRemovedFromDb()
    {
        // Arrange
        var releaseVersionId = Guid.NewGuid();

        var builder = new DataSetUploadMockBuilder();
        var upload1 = builder.WithReleaseVersionId(releaseVersionId).BuildScreenedEntity();
        var upload2 = builder.WithReleaseVersionId(releaseVersionId).BuildScreenedEntity();
        var upload3 = builder.WithReleaseVersionId(releaseVersionId).BuildScreenedEntity();

        var contextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
        {
            contentDbContext.DataSetUploads.AddRange(upload1, upload2, upload3);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
        {
            var privateBlobStorageService = new Mock<IPrivateBlobStorageService>(Strict);

            privateBlobStorageService
                .Setup(mock => mock.DeleteBlob(PrivateReleaseTempFiles, It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            var service = BuildService(contentDbContext, privateBlobStorageService.Object);

            // Act
            var result = await service.Delete(releaseVersionId, upload2.Id, CancellationToken.None);

            // Assert
            privateBlobStorageService.Verify(
                mock => mock.DeleteBlob(PrivateReleaseTempFiles, It.IsAny<string>()),
                Times.Exactly(2)
            );

            result.AssertRight();
            Assert.Equal(2, contentDbContext.DataSetUploads.Count());
            Assert.Null(await contentDbContext.DataSetUploads.FindAsync(upload2.Id));
        }
    }

    [Fact]
    public async Task DeleteAll_Success_ObjectsRemovedFromDb()
    {
        // Arrange
        var releaseVersionId = Guid.NewGuid();

        var builder = new DataSetUploadMockBuilder();
        var upload1 = builder.WithReleaseVersionId(releaseVersionId).BuildScreenedEntity();
        var upload2 = builder.WithReleaseVersionId(releaseVersionId).BuildScreenedEntity();
        var upload3 = builder.WithReleaseVersionId(releaseVersionId).BuildScreenedEntity();

        var contextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
        {
            contentDbContext.DataSetUploads.AddRange(upload1, upload2, upload3);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
        {
            var privateBlobStorageService = new Mock<IPrivateBlobStorageService>(Strict);

            privateBlobStorageService
                .Setup(mock => mock.DeleteBlob(PrivateReleaseTempFiles, It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            var service = BuildService(contentDbContext, privateBlobStorageService.Object);

            // Act
            var result = await service.DeleteAll(releaseVersionId, CancellationToken.None);

            // Assert
            privateBlobStorageService.Verify(
                mock => mock.DeleteBlob(PrivateReleaseTempFiles, It.IsAny<string>()),
                Times.Exactly(6)
            );

            result.AssertRight();
            Assert.Empty(contentDbContext.DataSetUploads);
        }
    }

    private void AssertBasicViewModelDetailsCorrect(DataSetUploadViewModel uploadViewModel, DataSetUpload upload)
    {
        Assert.Equal(upload.Id, uploadViewModel.Id);
        Assert.Equal(upload.Created, uploadViewModel.Created);
        Assert.Equal(upload.DataFileName, uploadViewModel.DataFileName);
        Assert.Equal(upload.DataFileSizeInBytes.DisplaySize(), uploadViewModel.DataFileSize);
        Assert.Equal(upload.DataSetTitle, uploadViewModel.DataSetTitle);
        Assert.Equal(upload.MetaFileName, uploadViewModel.MetaFileName);
        Assert.Equal(upload.MetaFileSizeInBytes.DisplaySize(), uploadViewModel.MetaFileSize);
        Assert.Equal(upload.ReplacingFileId, uploadViewModel.ReplacingFileId);
        Assert.Equal(upload.UploadedBy, uploadViewModel.UploadedBy);
    }

    private void AssertScreenerTestsCorrect(
        List<DataScreenerTestResult> testResults,
        List<ScreenerTestResultViewModel> viewModels
    )
    {
        Assert.Equal(testResults.Count, viewModels.Count);

        testResults.ForEach(
            (testResult, index) =>
            {
                var viewModel = viewModels[index];
                Assert.Equal(testResult.Stage, viewModel.Stage);
                Assert.Equal(testResult.Notes, viewModel.Notes);
                Assert.Equal(testResult.Result.ToString(), viewModel.Result);
                Assert.Equal(testResult.TestFunctionName, viewModel.TestFunctionName);
            }
        );
    }

    private static DataSetUploadRepository BuildService(
        ContentDbContext context,
        IPrivateBlobStorageService? privateBlobStorageService = null
    )
    {
        return new DataSetUploadRepository(
            context,
            privateBlobStorageService ?? Mock.Of<IPrivateBlobStorageService>(),
            mapper: MapperUtils.AdminMapper()
        );
    }
}
