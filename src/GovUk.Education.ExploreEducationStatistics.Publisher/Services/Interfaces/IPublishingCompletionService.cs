using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Publisher.Model;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;

public interface IPublishingCompletionService
{
    Task CompletePublishingIfAllPriorStagesComplete(IEnumerable<(Guid ReleaseId, Guid ReleaseStatusId)> releaseAndReleaseStatusIds, DateTime publishedDate);
    
    Task CompletePublishingIfAllPriorStagesComplete(ReleasePublishingStatus[] releaseStatuses, DateTime publishedDate);
}