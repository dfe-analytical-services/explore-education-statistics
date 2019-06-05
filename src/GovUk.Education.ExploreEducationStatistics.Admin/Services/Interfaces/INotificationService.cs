using GovUk.Education.ExploreEducationStatistics.Admin.Models;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces
{
    public interface INotificationService
    {
        bool SendNotification(PublicationViewModel publication);

    }
}