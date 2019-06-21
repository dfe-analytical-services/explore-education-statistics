using GovUk.Education.ExploreEducationStatistics.Data.Processor.Model;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces
{
    public interface IImportService
    {
        void Import(ImportMessage importMessage);
    }
}