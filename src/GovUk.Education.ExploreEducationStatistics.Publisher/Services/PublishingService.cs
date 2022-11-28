#nullable enable
using System;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using Microsoft.Extensions.Logging;
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainers;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.FileType;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.FileStoragePathUtils;
using static GovUk.Education.ExploreEducationStatistics.Publisher.Utils.CopyDirectoryCallbacks;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services
{
    public class PublishingService : IPublishingService
    {
        private readonly string _publicStorageConnectionString;
        private readonly IBlobStorageService _privateBlobStorageService;
        private readonly IBlobStorageService _publicBlobStorageService;
        private readonly IMethodologyService _methodologyService;
        private readonly IPublicationRepository _publicationRepository;
        private readonly IReleaseService _releaseService;
        private readonly ILogger<PublishingService> _logger;

        public PublishingService(
            string publicStorageConnectionString,
            IBlobStorageService privateBlobStorageService,
            IBlobStorageService publicBlobStorageService,
            IMethodologyService methodologyService,
            IPublicationRepository publicationRepository,
            IReleaseService releaseService,
            ILogger<PublishingService> logger)
        {
            _publicStorageConnectionString = publicStorageConnectionString;
            _privateBlobStorageService = privateBlobStorageService;
            _publicBlobStorageService = publicBlobStorageService;
            _methodologyService = methodologyService;
            _publicationRepository = publicationRepository;
            _releaseService = releaseService;
            _logger = logger;
        }

        public async Task PublishStagedReleaseContent()
        {
            _logger.LogInformation("Moving staged release content");
            await _publicBlobStorageService.MoveDirectory(
                sourceContainerName: PublicContent,
                sourceDirectoryPath: PublicContentStagingPath(),
                destinationContainerName: PublicContent,
                destinationDirectoryPath: string.Empty
            );
        }

        public async Task PublishMethodologyFiles(Guid methodologyId)
        {
            var methodology = await _methodologyService.Get(methodologyId);
            await PublishMethodologyFiles(methodology);
        }

        public async Task PublishMethodologyFilesIfApplicableForRelease(Guid releaseId)
        {
            var methodologies = await _methodologyService.GetLatestByRelease(releaseId);

            if (methodologies.Any())
            {
                // Publish the files of the latest methodologies of this release that
                // aren't already accessible but depended on this release being published,
                // since those methodologies will be published for the first time with this release
                var release = await _releaseService.Get(releaseId);
                var firstRelease = !await _publicationRepository.IsPublished(release.PublicationId);
                foreach (var methodology in methodologies)
                {
                    if (methodology.Approved)
                    {
                        // Include methodologies scheduled immediately that will now be accessible
                        // because this Publication's first release is being published
                        var firstReleaseAndMethodologyScheduledImmediately =
                            firstRelease &&
                            methodology.ScheduledForPublishingImmediately;

                        // Include methodologies scheduled to be published with this release
                        var methodologyScheduledWithThisRelease =
                            methodology.ScheduledForPublishingWithRelease
                            && methodology.ScheduledWithReleaseId == releaseId;

                        if (firstReleaseAndMethodologyScheduledImmediately ||
                            methodologyScheduledWithThisRelease)
                        {
                            await PublishMethodologyFiles(methodology);
                        }
                    }
                }
            }
        }

        public async Task PublishReleaseFiles(Guid releaseId)
        {
            var release = await _releaseService.Get(releaseId);

            var files = await _releaseService.GetFiles(
                releaseId,
                Ancillary,
                Chart,
                FileType.Data,
                Image
            );

            var destinationDirectoryPath = $"{release.Id}/";

            // Delete any existing blobs in public storage
            await _publicBlobStorageService.DeleteBlobs(
                containerName: PublicReleaseFiles,
                directoryPath: destinationDirectoryPath);

            // Get a list of source directory paths for all the files.
            // There will be multiple root paths if they were created on different amendment Releases
            var sourceDirectoryPaths = files
                .Select(f => $"{f.RootPath}/")
                .Distinct()
                .ToList();

            // Copy the blobs of those directories in private storage to the destination directory in public storage
            foreach (var sourceDirectoryPath in sourceDirectoryPaths)
            {
                await _privateBlobStorageService.CopyDirectory(
                    sourceContainerName: PrivateReleaseFiles,
                    sourceDirectoryPath: sourceDirectoryPath,
                    destinationContainerName: PublicReleaseFiles,
                    destinationDirectoryPath: destinationDirectoryPath,
                    new IBlobStorageService.CopyDirectoryOptions
                    {
                        DestinationConnectionString = _publicStorageConnectionString,
                        ShouldTransferCallbackAsync = (source, _) =>
                            // Filter by blobs with matching file paths
                            TransferBlobIfFileExistsCallback(
                                source: source,
                                files: files,
                                sourceContainerName: PrivateReleaseFiles,
                                logger: _logger)
                    });
            }
        }

        private async Task PublishMethodologyFiles(MethodologyVersion methodologyVersion)
        {
            var files = await _methodologyService.GetFiles(methodologyVersion.Id, Image);

            var directoryPath = $"{methodologyVersion.Id}/";

            // Delete any existing blobs in public storage
            await _publicBlobStorageService.DeleteBlobs(
                containerName: PublicMethodologyFiles,
                directoryPath: directoryPath
            );

            // Copy the blobs from private to public storage
            await _privateBlobStorageService.CopyDirectory(
                sourceContainerName: PrivateMethodologyFiles,
                sourceDirectoryPath: directoryPath,
                destinationContainerName: PublicMethodologyFiles,
                destinationDirectoryPath: directoryPath,
                new IBlobStorageService.CopyDirectoryOptions
                {
                    DestinationConnectionString = _publicStorageConnectionString,
                    ShouldTransferCallbackAsync = (source, _) =>
                        // Filter by blobs with matching file paths
                        TransferBlobIfFileExistsCallback(
                            source: source,
                            files: files,
                            sourceContainerName: PrivateMethodologyFiles,
                            logger: _logger)
                });
        }
    }
}
