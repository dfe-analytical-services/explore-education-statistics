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

        public async Task PublishReleaseData(PublishReleaseDataMessage message)
        {
            await _fileStorageService.CopyReleaseToPublicContainer(message);

            // TODO DFE-874 Run the importer or copy the data from the statistics database
            // TODO DFE-874 to the publicly available statistics database
        }
    }
}