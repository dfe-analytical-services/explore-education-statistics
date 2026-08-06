using System.Net.Mime;
using GovUk.Education.ExploreEducationStatistics.Analytics.Common;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Content.Api.Controllers;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Fixtures;
using GovUk.Education.ExploreEducationStatistics.Content.Services;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Utils.ContentDbUtils;

namespace GovUk.Education.ExploreEducationStatistics.Content.Api.Tests.Controllers;

public class ReleaseFileControllerDirectBlobDownloadTests
{
    private static readonly DataFixture DataFixture = new();

    [Fact]
    public async Task Stream_Enabled_RedirectsIndividualPublishedFile()
    {
        var releaseVersionId = Guid.NewGuid();
        var fileId = Guid.NewGuid();
        var releaseFileService = new Mock<IReleaseFileService>(MockBehavior.Strict);
        releaseFileService
            .Setup(service => service.GetFileDownloadRedirectPath(releaseVersionId, fileId, CancellationToken.None))
            .ReturnsAsync("/downloads/release/data/file.csv");
        await using var contentDbContext = InMemoryContentDbContext(Guid.NewGuid().ToString());
        var controller = CreateController(contentDbContext, releaseFileService.Object);

        var result = await controller.Stream(releaseVersionId.ToString(), fileId.ToString());

        var redirect = Assert.IsType<RedirectResult>(result);
        Assert.Equal("/downloads/release/data/file.csv", redirect.Url);
    }

    [Fact]
    public async Task Stream_Enabled_UnpublishedFileFallsBackToExistingStreamingPath()
    {
        ReleaseVersion releaseVersion = DataFixture.DefaultReleaseVersion().WithPublished(null);
        var fileId = Guid.NewGuid();
        await using var contentDbContext = InMemoryContentDbContext(Guid.NewGuid().ToString());
        contentDbContext.ReleaseVersions.Add(releaseVersion);
        await contentDbContext.SaveChangesAsync();
        var releaseFileService = new Mock<IReleaseFileService>(MockBehavior.Strict);
        releaseFileService
            .Setup(service => service.GetFileDownloadRedirectPath(releaseVersion.Id, fileId, CancellationToken.None))
            .ReturnsAsync((string?)null);
        releaseFileService
            .Setup(service => service.StreamFile(releaseVersion.Id, fileId))
            .ReturnsAsync(
                new FileStreamResult(new MemoryStream("draft"u8.ToArray()), MediaTypeNames.Text.Plain)
                {
                    FileDownloadName = "draft.txt",
                }
            );
        var controller = CreateController(contentDbContext, releaseFileService.Object);

        var result = await controller.Stream(releaseVersion.Id.ToString(), fileId.ToString());

        Assert.IsType<FileStreamResult>(result);
        releaseFileService.VerifyAll();
    }

    [Fact]
    public async Task StreamFilesToZip_Enabled_RedirectsCachedAllFilesZip()
    {
        var releaseVersion = CreatePublishedReleaseVersion();
        await using var contentDbContext = InMemoryContentDbContext(Guid.NewGuid().ToString());
        contentDbContext.ReleaseVersions.Add(releaseVersion);
        await contentDbContext.SaveChangesAsync();
        var releaseFileService = new Mock<IReleaseFileService>(MockBehavior.Strict);
        releaseFileService
            .Setup(service =>
                service.GetAllFilesZipDownloadRedirectPath(
                    It.Is<ReleaseVersion>(version => version.Id == releaseVersion.Id),
                    AnalyticsFromPage.ReleaseDownloads,
                    CancellationToken.None
                )
            )
            .ReturnsAsync("/downloads/release/zip/all-files.zip");
        var controller = CreateController(contentDbContext, releaseFileService.Object);

        var result = await controller.StreamFilesToZip(releaseVersion.Id, AnalyticsFromPage.ReleaseDownloads);

        var redirect = Assert.IsType<RedirectResult>(result);
        Assert.Equal("/downloads/release/zip/all-files.zip", redirect.Url);
    }

    [Fact]
    public async Task StreamFilesToZip_Enabled_ColdCacheStreamsAndGeneratesNormally()
    {
        var releaseVersion = CreatePublishedReleaseVersion();
        await using var contentDbContext = InMemoryContentDbContext(Guid.NewGuid().ToString());
        contentDbContext.ReleaseVersions.Add(releaseVersion);
        await contentDbContext.SaveChangesAsync();
        var releaseFileService = new Mock<IReleaseFileService>(MockBehavior.Strict);
        releaseFileService
            .Setup(service =>
                service.GetAllFilesZipDownloadRedirectPath(
                    It.Is<ReleaseVersion>(version => version.Id == releaseVersion.Id),
                    AnalyticsFromPage.ReleaseDownloads,
                    CancellationToken.None
                )
            )
            .ReturnsAsync((string?)null);
        releaseFileService
            .Setup(service =>
                service.ZipFilesToStream(
                    releaseVersion.Id,
                    It.IsAny<Stream>(),
                    AnalyticsFromPage.ReleaseDownloads,
                    null,
                    CancellationToken.None
                )
            )
            .ReturnsAsync(Unit.Instance);
        var controller = CreateController(contentDbContext, releaseFileService.Object);

        var result = await controller.StreamFilesToZip(releaseVersion.Id, AnalyticsFromPage.ReleaseDownloads);

        Assert.IsType<EmptyResult>(result);
        releaseFileService.VerifyAll();
    }

    [Fact]
    public async Task StreamFilesToZip_Enabled_SelectedFileNeverRedirects()
    {
        var releaseVersion = CreatePublishedReleaseVersion();
        var fileId = Guid.NewGuid();
        await using var contentDbContext = InMemoryContentDbContext(Guid.NewGuid().ToString());
        contentDbContext.ReleaseVersions.Add(releaseVersion);
        await contentDbContext.SaveChangesAsync();
        var releaseFileService = new Mock<IReleaseFileService>(MockBehavior.Strict);
        releaseFileService
            .Setup(service =>
                service.ZipFilesToStream(
                    releaseVersion.Id,
                    It.IsAny<Stream>(),
                    AnalyticsFromPage.ReleaseUsefulInfo,
                    It.Is<IEnumerable<Guid>>(ids => ids.SequenceEqual(new[] { fileId })),
                    CancellationToken.None
                )
            )
            .ReturnsAsync(Unit.Instance);
        var controller = CreateController(contentDbContext, releaseFileService.Object);

        var result = await controller.StreamFilesToZip(
            releaseVersion.Id,
            AnalyticsFromPage.ReleaseUsefulInfo,
            new[] { fileId }
        );

        Assert.IsType<EmptyResult>(result);
        releaseFileService.VerifyAll();
    }

    private static ReleaseFileController CreateController(
        ContentDbContext contentDbContext,
        IReleaseFileService releaseFileService
    )
    {
        return new ReleaseFileController(
            new PersistenceHelper<ContentDbContext>(contentDbContext),
            releaseFileService,
            Options.Create(new DirectBlobDownloadsOptions { Enabled = true })
        )
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() },
        };
    }

    private static ReleaseVersion CreatePublishedReleaseVersion()
    {
        return DataFixture
            .DefaultReleaseVersion()
            .WithPublished(DateTimeOffset.UtcNow.AddHours(-1))
            .WithRelease(
                DataFixture
                    .DefaultRelease()
                    .WithPublication(DataFixture.DefaultPublication().WithSlug("publication-slug"))
            );
    }
}
