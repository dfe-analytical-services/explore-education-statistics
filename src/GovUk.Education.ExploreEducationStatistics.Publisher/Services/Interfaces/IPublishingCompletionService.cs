using System;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces;

public interface IPublishingCompletionService
{
    Task CompletePublishingIfAllStagesComplete(Guid releaseId, Guid releaseStatusId);
}