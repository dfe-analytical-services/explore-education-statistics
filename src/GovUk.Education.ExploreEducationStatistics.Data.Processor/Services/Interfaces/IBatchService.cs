using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Model;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces
{
    public interface IBatchService
    {
        Task UpdateBatchCount(string releaseId, string dataFileName, int batchNo);

        Task<bool> UpdateStatus(string releaseId, string dataFileName, IStatus status);

        Task FailImport(string releaseId, string dataFileName, List<string> errors);

        Task LogErrors(string releaseId, string dataFileName, List<string> errors);

        Task CreateImport(string releaseId, string dataFileName, int numberOfRows, int numBatches, ImportMessage message);

        Task<bool> IsBatchProcessed(string releaseId, string dataFileName, int batchNo);
    }
}