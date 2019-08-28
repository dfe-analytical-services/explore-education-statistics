using System.Collections.Generic;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces
{
    public interface IBatchService
    {
        Task UpdateBatchCount(string releaseId, int batchSize, int batchNo, string dataFileName);

        Task<bool> IsBatchComplete(string releaseId, int batchSize, string dataFileName);

        Task UpdateStatus(string releaseId, int batchSize, ImportStatus status, string dataFileName);
        Task UpdateStatus(string releaseId, ImportStatus status, string dataFileName);
        Task FailBatch(string releaseId, List<string> errors, string dataFileName);
        Task LogErrors(string releaseId, List<string> errors, int batchNo, string dataFileName);
    }
}