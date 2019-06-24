using GovUk.Education.ExploreEducationStatistics.Publisher.Model;
using GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services
{
    public class PublishingService : IPublishingService
    {
        private readonly IFileStorageService _fileStorageService;

        private const string PrivateContainerName = "releases";
        private const string PublicContainerName = "downloads";

        public PublishingService(IFileStorageService fileStorageService)
        {
            _fileStorageService = fileStorageService;
        }

        public void PublishReleaseData(PublishReleaseDataMessage message)
        {
            _fileStorageService.CopyFilesAsync(message.PublicationSlug, message.ReleaseSlug, PrivateContainerName,
                PublicContainerName);

            // TODO DFE-874 Run the importer or copy the data from the statistics database
            // TODO DFE-874 to the publicly available statistics database
        }
    }
}