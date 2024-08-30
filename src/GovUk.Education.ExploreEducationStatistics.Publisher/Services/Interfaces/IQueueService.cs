using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;

public interface IQueueService
{
    Task QueueStageReleaseContentMessages(IReadOnlyList<ReleasePublishingKeyOld> releasePublishingKeys);

    Task QueuePublishReleaseContentMessage(ReleasePublishingKeyOld releasePublishingKeyOld);

    Task QueuePublishReleaseFilesMessages(IReadOnlyList<ReleasePublishingKeyOld> releasePublishingKeys);
}
