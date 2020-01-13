using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
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

        public async Task PublishReleaseContentAsync(ReleaseStatus releaseStatus)
        {
            await _fileStorageService.MoveStagedContentAsync(releaseStatus);
            await _releaseService.SetPublishedDateAsync(releaseStatus.ReleaseId);
        }

        public async Task PublishReleaseFilesAsync(PublishReleaseFilesMessage message)
        {
            var copyReleaseCommand = new CopyReleaseCommand
            {
                ReleaseId = message.ReleaseId,
                PublicationSlug = message.PublicationSlug,
                PublishScheduled = message.ReleasePublished,
                ReleaseSlug = message.ReleaseSlug
            };
            await _fileStorageService.CopyReleaseToPublicContainer(copyReleaseCommand);
        }
    }
}