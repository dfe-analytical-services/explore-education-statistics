using System.Collections.Generic;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces
{
    public interface IStorageQueueService
    {
        void AddMessage(string queueName, object value);

        Task AddMessageAsync(string queueName, object value);

        Task AddMessages(string queueName, IEnumerable<object> values);
    }
}