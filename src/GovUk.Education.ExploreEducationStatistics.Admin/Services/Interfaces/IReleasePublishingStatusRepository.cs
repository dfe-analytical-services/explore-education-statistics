#nullable enable
using System.Collections.Generic;
using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;

public interface IReleasePublishingStatusRepository
{
    Task<IReadOnlyList<ReleasePublishingStatus>> GetAllByOverallStage(Guid releaseVersionId,
        params ReleasePublishingStatusOverallStage[] overallStages);

    Task RemovePublisherReleaseStatuses(IReadOnlyList<Guid> releaseVersionIds);
}
