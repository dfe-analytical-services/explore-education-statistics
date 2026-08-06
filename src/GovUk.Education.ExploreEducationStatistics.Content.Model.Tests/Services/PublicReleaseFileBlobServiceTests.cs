using System.Net.Mime;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Tests.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Services;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainers;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Tests.Services;

public class PublicReleaseFileBlobServiceTests
{
    [Fact]
    public async Task GetDownloadRedirectPath_ReturnsNotFound_WhenPublishedBlobDoesNotExist()
    {
        var releaseFile = CreateReleaseFile();
        var blobStorageService = new Mock<IPublicBlobStorageService>(MockBehavior.Strict);
        blobStorageService
            .Setup(service => service.FindBlob(PublicReleaseFiles, releaseFile.PublicPath()))
            .ReturnsAsync((BlobInfo?)null);

        var result = await new PublicReleaseFileBlobService(blobStorageService.Object).GetDownloadRedirectPath(
            releaseFile
        );

        result.AssertNotFound();
    }

    [Fact]
    public async Task GetDownloadRedirectPath_ReturnsRelativePath_WithoutUpdatingCorrectHeaders()
    {
        var releaseFile = CreateReleaseFile();
        var path = releaseFile.PublicPath();
        var blobStorageService = new Mock<IPublicBlobStorageService>(MockBehavior.Strict);
        blobStorageService
            .Setup(service => service.FindBlob(PublicReleaseFiles, path))
            .ReturnsAsync(
                new BlobInfo(
                    path: path,
                    contentType: MediaTypeNames.Application.Pdf,
                    contentLength: 100,
                    contentDisposition: "attachment; filename=\"supporting-file.pdf\""
                )
            );

        var result = await new PublicReleaseFileBlobService(blobStorageService.Object).GetDownloadRedirectPath(
            releaseFile
        );

        Assert.Equal($"/downloads/{path}", result.AssertRight());
    }

    [Fact]
    public async Task GetDownloadRedirectPath_RepairsMissingDownloadHeaders()
    {
        var releaseFile = CreateReleaseFile();
        var path = releaseFile.PublicPath();
        var blobStorageService = new Mock<IPublicBlobStorageService>(MockBehavior.Strict);
        blobStorageService
            .Setup(service => service.FindBlob(PublicReleaseFiles, path))
            .ReturnsAsync(new BlobInfo(path: path, contentType: string.Empty, contentLength: 100));
        blobStorageService
            .Setup(service =>
                service.UpdateBlobProperties(
                    PublicReleaseFiles,
                    path,
                    MediaTypeNames.Application.Pdf,
                    "attachment; filename=\"supporting-file.pdf\"",
                    null,
                    It.IsAny<CancellationToken>()
                )
            )
            .Returns(Task.CompletedTask);

        var result = await new PublicReleaseFileBlobService(blobStorageService.Object).GetDownloadRedirectPath(
            releaseFile
        );

        Assert.Equal($"/downloads/{path}", result.AssertRight());
        blobStorageService.VerifyAll();
    }

    private static ReleaseFile CreateReleaseFile()
    {
        return new ReleaseFile
        {
            ReleaseVersionId = Guid.NewGuid(),
            File = new File
            {
                Id = Guid.NewGuid(),
                RootPath = Guid.NewGuid(),
                Filename = "supporting-file.pdf",
                ContentType = MediaTypeNames.Application.Pdf,
                Type = FileType.Ancillary,
            },
        };
    }
}
