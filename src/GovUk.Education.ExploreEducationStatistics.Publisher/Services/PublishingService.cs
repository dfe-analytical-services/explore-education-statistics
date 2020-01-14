using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Publisher.Models;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services
{
    public class PublishingService : IPublishingService
    {
        private readonly IFileStorageService _fileStorageService;
        private readonly IReleaseService _releaseService;

        public PublishingService(IFileStorageService fileStorageService, IReleaseService releaseService)
        {
            _fileStorageService = fileStorageService;
            _releaseService = releaseService;
        }

        public async Task PublishReleaseContentAsync(Guid releaseId)
        {
            await _fileStorageService.MoveStagedContentAsync();
            await _releaseService.SetPublishedDateAsync(releaseId);
        }

        public async Task PublishReleaseFilesAsync(Guid releaseId)
        {
            var release = await _releaseService.GetAsync(releaseId);
            var copyReleaseCommand = new CopyReleaseCommand
            {
                ReleaseId = releaseId,
                PublicationSlug = release.Publication.Slug,
                PublishScheduled = release.PublishScheduled.Value,
                ReleaseSlug = release.Slug
            };
            await _fileStorageService.CopyReleaseToPublicContainer(copyReleaseCommand);
        }
    }
}