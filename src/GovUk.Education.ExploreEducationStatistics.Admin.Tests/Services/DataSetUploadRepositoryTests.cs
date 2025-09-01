#nullable enable
using AutoMapper;
using GovUk.Education.ExploreEducationStatistics.Admin.Services;
using GovUk.Education.ExploreEducationStatistics.Admin.Tests.MockBuilders;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
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

        var builder = new DataSetUploadMockBuilder();
        var upload1 = builder.WithReleaseVersionId(releaseVersionId).BuildEntity();
        var upload2 = builder.WithReleaseVersionId(releaseVersionId).BuildEntity();
        var upload3 = builder.WithReleaseVersionId(releaseVersionId).BuildEntity();

        var contextId = Guid.NewGuid().ToString();
        await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
        {
            contentDbContext.DataSetUploads.AddRange(upload1, upload2, upload3);
            await contentDbContext.SaveChangesAsync();
        }

        await using (var contentDbContext = InMemoryApplicationDbContext(contextId))
        {
            var service = BuildService(contentDbContext);

            // Act
            var result = await service.ListAll(releaseVersionId, default);

            // Assert
            var uploads = result.AssertRight();

            Assert.Equal(3, uploads.Count);
        }
    }

    [Fact]
    public async Task Delete_Success_ObjectRemovedFromDb()
    {
        // Arrange
        var releaseVersionId = Guid.NewGuid();

        var builder = new DataSetUploadMockBuilder();
        var upload1 = builder.WithReleaseVersionId(releaseVersionId).BuildEntity();
        var upload2 = builder.WithReleaseVersionId(releaseVersionId).BuildEntity();
        var upload3 = builder.WithReleaseVersionId(releaseVersionId).BuildEntity();

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
                .Setup(mock => mock.DeleteBlob(
                    PrivateReleaseTempFiles,
                    It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            var service = BuildService(
                contentDbContext,
                privateBlobStorageService.Object);

            // Act
            var result = await service.Delete(releaseVersionId, upload2.Id, default);

            // Assert
            privateBlobStorageService
                .Verify(mock => mock.DeleteBlob(
                    PrivateReleaseTempFiles,
                    It.IsAny<string>()),
                Times.Exactly(2));

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
        var upload1 = builder.WithReleaseVersionId(releaseVersionId).BuildEntity();
        var upload2 = builder.WithReleaseVersionId(releaseVersionId).BuildEntity();
        var upload3 = builder.WithReleaseVersionId(releaseVersionId).BuildEntity();

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
                .Setup(mock => mock.DeleteBlob(
                    PrivateReleaseTempFiles,
                    It.IsAny<string>()))
                .Returns(Task.CompletedTask);

            var service = BuildService(
                contentDbContext,
                privateBlobStorageService.Object);

            // Act
            var result = await service.DeleteAll(releaseVersionId, default);

            // Assert
            privateBlobStorageService
                .Verify(mock => mock.DeleteBlob(
                    PrivateReleaseTempFiles,
                    It.IsAny<string>()),
                Times.Exactly(6));

            result.AssertRight();
            Assert.Empty(contentDbContext.DataSetUploads);
        }
    }

    private static DataSetUploadRepository BuildService(
        ContentDbContext context,
        IPrivateBlobStorageService? privateBlobStorageService = null,
        IMapper? mapper = null)
    {
        return new DataSetUploadRepository(
            context,
            privateBlobStorageService ?? Mock.Of<IPrivateBlobStorageService>(),
            mapper ?? Mock.Of<IMapper>());
    }
}
