using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Models;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;
using static GovUk.Education.ExploreEducationStatistics.Common.BlobContainerNames;
using static GovUk.Education.ExploreEducationStatistics.Common.Model.FileType;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.FileStoragePathUtils;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services
{
    public class PublishingService : IPublishingService
    {
        private readonly IFileStorageService _fileStorageService;
        private readonly IReleaseService _releaseService;

        public PublishingService(IFileStorageService fileStorageService,
            IReleaseService releaseService)
        {
            _fileStorageService = fileStorageService;
            _releaseService = releaseService;
        }

        public async Task PublishStagedReleaseContentAsync(Guid releaseId)
        {
            await _fileStorageService.MovePublicDirectory(PublicContentContainerName, PublicContentStagingPath(),
                string.Empty);
            await _releaseService.SetPublishedDatesAsync(releaseId, DateTime.UtcNow);
        }

        public async Task PublishReleaseFilesAsync(Guid releaseId)
        {
            var release = await _releaseService.GetAsync(releaseId);
            var copyReleaseCommand = new CopyReleaseFilesCommand
            {
                ReleaseId = releaseId,
                PublicationSlug = release.Publication.Slug,
                PublishScheduled = release.PublishScheduled.Value,
                ReleaseSlug = release.Slug,
                Files = await _releaseService.GetFiles(releaseId,
                    Ancillary,
                    Chart,
                    FileType.Data)
            };
            await _fileStorageService.CopyReleaseFilesToPublicContainer(copyReleaseCommand);
        }
    }
}