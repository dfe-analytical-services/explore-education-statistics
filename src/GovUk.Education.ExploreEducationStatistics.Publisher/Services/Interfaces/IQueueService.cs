using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces
{
    public interface IQueueService
    {
        Task QueueGenerateStagedReleaseContentMessageAsync(IEnumerable<(Guid ReleaseId, Guid ReleaseStatusId)> releases);

        Task QueuePublishReleaseContentMessageAsync(Guid releaseId, Guid releaseStatusId);
        
        Task QueuePublishReleaseFilesMessageAsync(Guid releaseId, Guid releaseStatusId);

        Task QueuePublishReleaseFilesMessageAsync(IEnumerable<(Guid ReleaseId, Guid ReleaseStatusId)> releases);
    }
}