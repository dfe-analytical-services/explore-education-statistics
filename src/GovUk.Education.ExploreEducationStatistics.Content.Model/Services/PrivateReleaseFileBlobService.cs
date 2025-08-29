#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Services.Interfaces;
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainers;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model.Services;

public class PrivateReleaseFileBlobService(
    IPrivateBlobStorageService privateBlobStorageService) : IReleaseFileBlobService
{
    public Task<Stream> StreamBlob(
        ReleaseFile releaseFile,
        CancellationToken cancellationToken = default)
    {
        return privateBlobStorageService.StreamBlob(
            containerName: PrivateReleaseFiles,
            path: releaseFile.Path(),
            cancellationToken: cancellationToken
        );
    }
}
