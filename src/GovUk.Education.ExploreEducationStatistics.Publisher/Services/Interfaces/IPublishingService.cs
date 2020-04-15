using System;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces
{
    public interface IPublishingService
    {
        Task PublishStagedReleaseContentAsync(Guid releaseId);

        Task PublishReleaseFilesAsync(Guid releaseId);
    }
}