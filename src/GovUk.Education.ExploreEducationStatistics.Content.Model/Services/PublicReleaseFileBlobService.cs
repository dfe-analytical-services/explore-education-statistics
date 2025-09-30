#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainers;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Services;

public class PublicReleaseFileBlobService(IPublicBlobStorageService publicBlobStorageService)
    : IReleaseFileBlobService
{
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
