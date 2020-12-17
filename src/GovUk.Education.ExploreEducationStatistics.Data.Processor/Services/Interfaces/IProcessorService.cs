using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Models;
using Microsoft.Azure.WebJobs;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces
{
    public interface IProcessorService
    {
        Task ProcessUnpackingArchive(ImportMessage message);

        Task ProcessStage1(ImportMessage message, ExecutionContext executionContext);

        Task ProcessStage2(ImportMessage message);

        Task ProcessStage3(ImportMessage message);

        Task ProcessStage4Messages(ImportMessage message, ICollector<ImportObservationsMessage> collector);
    }
}