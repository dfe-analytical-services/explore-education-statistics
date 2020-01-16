using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces
{
    public interface IQueueService
    {
        Task QueueGenerateReleaseContentMessageAsync(Guid releaseId, Guid releaseStatusId);

        Task QueuePublishReleaseDataMessagesAsync(IEnumerable<(Guid ReleaseId, Guid ReleaseStatusId)> releases);

        Task QueuePublishReleaseFilesMessagesAsync(IEnumerable<(Guid ReleaseId, Guid ReleaseStatusId)> releases);
    }
}