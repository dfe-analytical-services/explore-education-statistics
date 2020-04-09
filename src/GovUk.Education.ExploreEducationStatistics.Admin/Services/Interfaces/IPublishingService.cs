using System;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces
{
    public interface IPublishingService
    {
        Task QueuePublishReleaseContentImmediateMessageAsync(Guid releaseId);
        Task QueueValidateReleaseAsync(Guid releaseId, bool immediate = false);
    }
}