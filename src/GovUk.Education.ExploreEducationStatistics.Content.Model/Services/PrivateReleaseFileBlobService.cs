#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainers;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Services;

public class PrivateReleaseFileBlobService(IPrivateBlobStorageService privateBlobStorageService)
    : IReleaseFileBlobService
{
    public Task<Either<ActionResult, Stream>> GetDownloadStream(
        ReleaseFile releaseFile,
        CancellationToken cancellationToken = default
    )
    {
        return privateBlobStorageService.GetDownloadStream(
            containerName: PrivateReleaseFiles,
            path: releaseFile.Path(),
            cancellationToken: cancellationToken
        );
    }
}
