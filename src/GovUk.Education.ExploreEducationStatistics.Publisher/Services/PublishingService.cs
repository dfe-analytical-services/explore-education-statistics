using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Options;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainers;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.FileType;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.FileStoragePathUtils;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services;

public class PublishingService(
    IPrivateBlobStorageService privateBlobStorageService,
    IPublicBlobStorageService publicBlobStorageService,
    IMethodologyService methodologyService,
    IReleaseService releaseService,
    IOptions<AppOptions> appOptions,
    ILogger<PublishingService> logger
) : IPublishingService
{
    public async Task PublishStagedReleaseContent()
    {
        logger.LogInformation("Moving staged release content");
        await publicBlobStorageService.MoveDirectory(
            sourceContainerName: PublicContent,
            sourceDirectoryPath: PublicContentStagingPath(),
            destinationContainerName: PublicContent,
            destinationDirectoryPath: string.Empty
        );
    }

    public async Task PublishMethodologyFiles(Guid methodologyId)
    {
        var methodology = await methodologyService.Get(methodologyId);
        await PublishMethodologyFiles(methodology);
    }

    public async Task PublishMethodologyFilesIfApplicableForRelease(Guid releaseVersionId)
    {
        var releaseVersion = await releaseService.Get(releaseVersionId);
        var methodologyVersions = await methodologyService.GetLatestVersionByRelease(releaseVersion);

        if (!methodologyVersions.Any())
        {
            return;
        }

        foreach (var methodologyVersion in methodologyVersions)
        {
            if (await methodologyService.IsBeingPublishedAlongsideRelease(methodologyVersion, releaseVersion))
            {
                await PublishMethodologyFiles(methodologyVersion);
            }
        }
    }

    public async Task PublishReleaseFiles(Guid releaseVersionId, CancellationToken cancellationToken = default)
    {
        var releaseVersion = await releaseService.Get(releaseVersionId);

        var files = await releaseService.GetFiles(releaseVersionId, Ancillary, Chart, FileType.Data, Image);

        var destinationDirectoryPath = $"{releaseVersion.Id}/";

        // Delete any existing blobs in public storage
        await publicBlobStorageService.DeleteBlobs(
            containerName: PublicReleaseFiles,
            directoryPath: destinationDirectoryPath
        );

        var blobNames = files.Select(f => $"{f.RootPath}/{f.Type}/{f.Filename}").ToList();

        await privateBlobStorageService.CopyBlobs(
            sourceContainerName: PrivateReleaseFiles,
            destinationContainerName: PublicReleaseFiles,
            blobNames,
            cancellationToken
        );
    }

    private async Task PublishMethodologyFiles(MethodologyVersion methodologyVersion)
    {
        var directoryPath = $"{methodologyVersion.Id}/";

        // Delete any existing blobs in public storage
        await publicBlobStorageService.DeleteBlobs(containerName: PublicMethodologyFiles, directoryPath: directoryPath);

        // Copy the blobs from private to public storage
        await privateBlobStorageService.CopyDirectory(
            sourceContainerName: PrivateMethodologyFiles,
            sourceDirectoryPath: directoryPath,
            destinationContainerName: PublicMethodologyFiles,
            destinationDirectoryPath: directoryPath
        );
    }
}
