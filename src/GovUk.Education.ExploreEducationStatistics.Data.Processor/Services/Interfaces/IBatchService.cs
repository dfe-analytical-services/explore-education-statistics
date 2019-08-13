using GovUk.Education.ExploreEducationStatistics.Data.Processor.Model;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces
{
    public interface IBatchService
    {
        void UpdateBatchCount(ImportMessage importMessage, string subjectId);

        bool IsBatchComplete(ImportMessage importMessage, string subjectId);

        void UpdateCurrentBatchNumber(ImportMessage importMessage, string subjectId);
    }
}