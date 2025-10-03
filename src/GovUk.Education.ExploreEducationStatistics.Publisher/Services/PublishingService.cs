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
using static GovUk.Education.ExploreEducationStatistics.Publisher.Utils.CopyDirectoryCallbacks;

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
    private readonly AppOptions _appOptions = appOptions.Value;

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

    public async Task PublishReleaseFiles(Guid releaseVersionId)
    {
        var releaseVersion = await releaseService.Get(releaseVersionId);

        var files = await releaseService.GetFiles(releaseVersionId, Ancillary, Chart, FileType.Data, Image);

        var destinationDirectoryPath = $"{releaseVersion.Id}/";

        // Delete any existing blobs in public storage
        await publicBlobStorageService.DeleteBlobs(
            containerName: PublicReleaseFiles,
            directoryPath: destinationDirectoryPath
        );

        // Get a list of source directory paths for all the files.
        // There will be multiple root paths if they were created on different amendment Releases
        var sourceDirectoryPaths = files.Select(f => $"{f.RootPath}/").Distinct().ToList();

        // Copy the blobs of those directories in private storage to the destination directory in public storage
        foreach (var sourceDirectoryPath in sourceDirectoryPaths)
        {
            await privateBlobStorageService.CopyDirectory(
                sourceContainerName: PrivateReleaseFiles,
                sourceDirectoryPath: sourceDirectoryPath,
                destinationContainerName: PublicReleaseFiles,
                destinationDirectoryPath: destinationDirectoryPath,
                new IBlobStorageService.CopyDirectoryOptions
                {
                    DestinationConnectionString = _appOptions.PublicStorageConnectionString,
                    ShouldTransferCallbackAsync = (source, _) =>
                        // Filter by blobs with matching file paths
                        TransferBlobIfFileExistsCallback(
                            source: source,
                            files: files,
                            sourceContainerName: PrivateReleaseFiles,
                            logger: logger
                        ),
                }
            );
        }
    }

    private async Task PublishMethodologyFiles(MethodologyVersion methodologyVersion)
    {
        var files = await methodologyService.GetFiles(methodologyVersion.Id, Image);

        var directoryPath = $"{methodologyVersion.Id}/";

        // Delete any existing blobs in public storage
        await publicBlobStorageService.DeleteBlobs(containerName: PublicMethodologyFiles, directoryPath: directoryPath);

        // Copy the blobs from private to public storage
        await privateBlobStorageService.CopyDirectory(
            sourceContainerName: PrivateMethodologyFiles,
            sourceDirectoryPath: directoryPath,
            destinationContainerName: PublicMethodologyFiles,
            destinationDirectoryPath: directoryPath,
            new IBlobStorageService.CopyDirectoryOptions
            {
                DestinationConnectionString = _appOptions.PublicStorageConnectionString,
                ShouldTransferCallbackAsync = (source, _) =>
                    // Filter by blobs with matching file paths
                    TransferBlobIfFileExistsCallback(
                        source: source,
                        files: files,
                        sourceContainerName: PrivateMethodologyFiles,
                        logger: logger
                    ),
            }
        );
    }
}
