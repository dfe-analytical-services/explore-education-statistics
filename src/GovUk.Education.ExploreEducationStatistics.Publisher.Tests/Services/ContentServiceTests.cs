using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.Cache;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Publications;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainers;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Tests.Services;

public class ContentServiceTests
{
    private readonly DataFixture _dataFixture = new();

    [Fact]
    public async Task InvalidatePreviousVersionsDownloadFiles_InitialPublication_DoesNothing()
    {
        var releaseVersion = _dataFixture.DefaultReleaseVersion().Generate();
        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.Add(releaseVersion);
            await contentDbContext.SaveChangesAsync();
        }

        var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);
        var frontDoorCacheService = new Mock<IFrontDoorCacheService>(MockBehavior.Strict);

        await using var testContentDbContext = InMemoryContentDbContext(contentDbContextId);
        var service = BuildService(testContentDbContext, blobStorageService.Object, frontDoorCacheService.Object);

        await service.InvalidatePreviousVersionsDownloadFiles([releaseVersion.Id]);

        blobStorageService.VerifyNoOtherCalls();
        frontDoorCacheService.VerifyNoOtherCalls();
    }

    [Fact]
    public async Task InvalidatePreviousVersionsDownloadFiles_Amendments_DeletesAndPurgesPreviousVersions()
    {
        var previousReleaseVersion1 = _dataFixture.DefaultReleaseVersion().Generate();
        var previousReleaseVersion2 = _dataFixture.DefaultReleaseVersion().Generate();
        var amendedReleaseVersion1 = _dataFixture
            .DefaultReleaseVersion()
            .WithPreviousVersion(previousReleaseVersion1)
            .Generate();
        var amendedReleaseVersion2 = _dataFixture
            .DefaultReleaseVersion()
            .WithPreviousVersion(previousReleaseVersion2)
            .Generate();
        var contentDbContextId = Guid.NewGuid().ToString();

        await using (var contentDbContext = InMemoryContentDbContext(contentDbContextId))
        {
            contentDbContext.ReleaseVersions.AddRange(amendedReleaseVersion1, amendedReleaseVersion2);
            await contentDbContext.SaveChangesAsync();
        }

        var blobStorageService = new Mock<IBlobStorageService>(MockBehavior.Strict);
        blobStorageService
            .Setup(service => service.DeleteBlobs(PublicReleaseFiles, $"{previousReleaseVersion1.Id}/", null))
            .Returns(Task.CompletedTask);
        blobStorageService
            .Setup(service => service.DeleteBlobs(PublicReleaseFiles, $"{previousReleaseVersion2.Id}/", null))
            .Returns(Task.CompletedTask);

        var expectedPreviousVersionIds = new HashSet<Guid> { previousReleaseVersion1.Id, previousReleaseVersion2.Id };
        var frontDoorCacheService = new Mock<IFrontDoorCacheService>(MockBehavior.Strict);
        frontDoorCacheService
            .Setup(service =>
                service.PurgeAllFilesZipCache(
                    It.Is<IReadOnlySet<Guid>>(ids => ids.SetEquals(expectedPreviousVersionIds)),
                    It.IsAny<CancellationToken>()
                )
            )
            .Returns(Task.CompletedTask);

        await using var testContentDbContext = InMemoryContentDbContext(contentDbContextId);
        var service = BuildService(testContentDbContext, blobStorageService.Object, frontDoorCacheService.Object);

        await service.InvalidatePreviousVersionsDownloadFiles([amendedReleaseVersion1.Id, amendedReleaseVersion2.Id]);

        blobStorageService.VerifyAll();
        frontDoorCacheService.VerifyAll();
    }

    private static ContentService BuildService(
        ContentDbContext contentDbContext,
        IBlobStorageService blobStorageService,
        IFrontDoorCacheService frontDoorCacheService
    ) =>
        new(
            contentDbContext,
            Mock.Of<IBlobCacheService>(),
            Mock.Of<IBlobCacheService>(),
            blobStorageService,
            Mock.Of<IReleaseService>(),
            Mock.Of<IMethodologyCacheService>(),
            Mock.Of<IPublicationsTreeService>(),
            frontDoorCacheService
        );
}
