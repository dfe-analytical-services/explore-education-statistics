using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces
{
    public interface IQueueService
    {
        Task QueueGenerateStagedReleaseContentMessage(IEnumerable<(Guid ReleaseId, Guid ReleaseStatusId)> releases);

        Task QueuePublishReleaseContentMessage(Guid releaseId, Guid releaseStatusId);
        
        Task QueuePublishReleaseFilesMessage(Guid releaseId, Guid releaseStatusId);

        Task QueuePublishReleaseFilesMessage(IEnumerable<(Guid ReleaseId, Guid ReleaseStatusId)> releases);
    }
}