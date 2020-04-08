using System;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces
{
    public interface IPublishingService
    {
        Task QueuePublishReleaseContentImmediateMessageAsync(Guid releaseId, Guid releaseStatusId);
        Task QueueValidateReleaseAsync(Guid releaseId, bool immediate = false);
    }
}