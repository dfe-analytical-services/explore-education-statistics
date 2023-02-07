#nullable enable

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;

public interface IPublishingCompletionService
{
    Task CompletePublishingIfAllPriorStagesComplete(
        IEnumerable<(Guid ReleaseId, Guid ReleaseStatusId)> releaseAndReleaseStatusIds);
}
