#nullable enable
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;

public interface IQueueService
{
    Task QueueGenerateStagedReleaseContentMessage(IEnumerable<(Guid ReleaseVersionId, Guid ReleaseStatusId)> releases);

    Task QueuePublishReleaseContentMessage(Guid releaseVersionId, Guid releaseStatusId);

    Task QueuePublishReleaseFilesMessage(Guid releaseVersionId, Guid releaseStatusId);

    Task QueuePublishReleaseFilesMessage(IEnumerable<(Guid ReleaseVersionId, Guid ReleaseStatusId)> releases);
}
