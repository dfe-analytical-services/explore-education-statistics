using System;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces
{
    public interface IPublishingService
    {
        Task PublishStagedReleaseContent(Guid releaseId);

        Task PublishMethodologyFiles(Guid methodologyId);

        Task PublishReleaseFiles(Guid releaseId);
    }
}
