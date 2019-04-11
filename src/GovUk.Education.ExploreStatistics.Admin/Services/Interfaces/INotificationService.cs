using GovUk.Education.ExploreStatistics.Admin.Models;

namespace GovUk.Education.ExploreStatistics.Admin.Services.Interfaces
{
    public interface INotificationService
    {
        bool SendNotification(PublicationViewModel publication);

    }
}