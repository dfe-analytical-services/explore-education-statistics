using System.Threading.Tasks;
using Microsoft.WindowsAzure.Storage.Table;

namespace GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces
{
    public interface ITableStorageService
    {
        Task<CloudTable> GetTableAsync(string tableName, bool createIfNotExists = true);
    }
}