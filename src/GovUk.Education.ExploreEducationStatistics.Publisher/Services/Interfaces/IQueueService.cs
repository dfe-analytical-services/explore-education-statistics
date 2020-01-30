using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces
{
    public interface IQueueService
    {
        Task QueueGenerateReleaseContentMessageAsync(IEnumerable<(Guid ReleaseId, Guid ReleaseStatusId)> releases);

        Task QueuePublishReleaseDataMessagesAsync(IEnumerable<(Guid ReleaseId, Guid ReleaseStatusId)> releases);

        Task QueuePublishReleaseFilesMessageAsync(IEnumerable<(Guid ReleaseId, Guid ReleaseStatusId)> releases);
    }
}