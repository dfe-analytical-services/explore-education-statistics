#nullable enable
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces
{
    public interface IStorageQueueService
    {
        void AddMessage(string queueName, object value);

        Task AddMessageAsync(string queueName, object value);

        Task AddMessages<T>(string queueName, List<T> values);

        Task Clear(string queueName);

        Task<int?> GetApproximateMessageCount(string queueName);
    }
}
