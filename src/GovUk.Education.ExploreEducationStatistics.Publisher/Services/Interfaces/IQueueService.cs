using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;

public interface IQueueService
{
    Task QueueGenerateStagedReleaseContentMessage(IReadOnlyList<ReleasePublishingKey> releasePublishingKeys);

    Task QueuePublishReleaseContentMessage(ReleasePublishingKey releasePublishingKey);

    Task QueuePublishReleaseFilesMessage(IReadOnlyList<ReleasePublishingKey> releasePublishingKeys);
}
