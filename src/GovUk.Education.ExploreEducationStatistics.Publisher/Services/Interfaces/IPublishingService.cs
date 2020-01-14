using System;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces
{
    public interface IPublishingService
    {
        Task PublishReleaseContentAsync(Guid releaseId);

        Task PublishReleaseFilesAsync(Guid releaseId);
    }
}