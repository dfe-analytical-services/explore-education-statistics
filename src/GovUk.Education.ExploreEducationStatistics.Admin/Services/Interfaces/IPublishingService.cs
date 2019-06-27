using System;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces
{
    public interface IPublishingService
    {
        void PublishReleaseData(Guid releaseId);
    }
}