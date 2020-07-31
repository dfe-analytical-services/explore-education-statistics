using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces
{
    public interface IStorageQueueService
    {
        void AddMessages(string queueName, params object[] values);
        
        Task AddMessagesAsync(string queueName, params object[] values);
    }
}