using System;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces
{
    public interface INotificationsService
    {
        bool NotifySubscribers(Guid publicationId);
    }
}