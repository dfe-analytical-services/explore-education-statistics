using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces
{
    public interface IBatchService
    {
        Task UpdateBatchCount(string releaseId, string dataFileName, int batchNo);

        Task UpdateStatus(string releaseId, string dataFileName, ImportStatus status);

        Task FailImport(string releaseId, string dataFileName, List<string> errors);

        Task LogErrors(string releaseId, string dataFileName, List<string> errors);

        Task CreateImport(string releaseId, string dataFileName, int numBatches);

        Task<DatafileImport> GetImport(string releaseId, string dataFileName);

        Task<bool> IsBatchProcessed(string releaseId, string dataFileName, int batchNo);
    }
}