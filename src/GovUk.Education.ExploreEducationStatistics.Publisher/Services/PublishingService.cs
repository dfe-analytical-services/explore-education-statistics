using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services
{
    public class PublishingService : IPublishingService
    {
        private readonly IFileStorageService _fileStorageService;

        public PublishingService(IFileStorageService fileStorageService)
        {
            _fileStorageService = fileStorageService;
        }

        public async Task PublishStagedContentAsync(ReleaseStatus releaseStatus)
        {
            await _fileStorageService.MoveStagedContentAsync(releaseStatus);
        }

        public async Task PublishReleaseFilesAsync(PublishReleaseFilesMessage message)
        {
            await _fileStorageService.CopyReleaseToPublicContainer(message);
        }
    }
}