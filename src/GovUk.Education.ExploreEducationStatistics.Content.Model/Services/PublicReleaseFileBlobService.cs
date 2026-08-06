#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainers;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Services;

public class PublicReleaseFileBlobService(IPublicBlobStorageService publicBlobStorageService)
    : IPublicReleaseFileBlobService
{
    public async Task<Either<ActionResult, string>> GetDownloadRedirectPath(
        ReleaseFile releaseFile,
        CancellationToken cancellationToken = default
    )
    {
        var path = releaseFile.PublicPath();
        var blob = await publicBlobStorageService.FindBlob(PublicReleaseFiles, path);
        if (blob is null)
        {
            return new NotFoundResult();
        }

        var contentDisposition = HttpResponseExtensions.ContentDispositionAttachmentHeader(releaseFile.File.Filename);

        if (blob.ContentDisposition != contentDisposition || blob.ContentType != releaseFile.File.ContentType)
        {
            await publicBlobStorageService.UpdateBlobProperties(
                containerName: PublicReleaseFiles,
                path: path,
                contentType: releaseFile.File.ContentType,
                contentDisposition: contentDisposition,
                cancellationToken: cancellationToken
            );
        }

        return $"/downloads/{path}";
    }

    public Task<Either<ActionResult, Stream>> GetDownloadStream(
        ReleaseFile releaseFile,
        CancellationToken cancellationToken = default
    )
    {
        return publicBlobStorageService.GetDownloadStream(
            containerName: PublicReleaseFiles,
            path: releaseFile.PublicPath(),
            cancellationToken: cancellationToken
        );
    }
}
