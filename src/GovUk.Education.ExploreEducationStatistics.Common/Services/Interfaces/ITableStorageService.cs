using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;

namespace GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces
{
    public interface ITableStorageService
    {
        Task<CloudTable> GetTableAsync(string tableName, bool createIfNotExists = true);
    }
}