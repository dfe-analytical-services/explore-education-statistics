using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces
{
    public interface IBatchService
    {
        Task UpdateBatchCount(string releaseId, string subjectId, int batchSize, int batchNo);

        Task<bool> IsBatchComplete(string releaseId, string subjectId, int batchSize);

        Task UpdateCurrentBatchNumber(string releaseId, string subjectId, int batchSize, int batchNo);
    }
}