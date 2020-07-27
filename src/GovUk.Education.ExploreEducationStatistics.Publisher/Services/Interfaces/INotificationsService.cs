using System;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Publisher.Services.Interfaces
{
    public interface INotificationsService
    {
        Task NotifySubscribers(params Guid[] releaseIds);
    }
}