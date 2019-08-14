namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces
{
    public interface IBatchService
    {
        void UpdateBatchCount(string releaseId, string subjectId, int batchSize, int batchNo);

        bool IsBatchComplete(string releaseId, string subjectId, int batchSize);

        void UpdateCurrentBatchNumber(string releaseId, string subjectId, int batchSize, int batchNo);
    }
}