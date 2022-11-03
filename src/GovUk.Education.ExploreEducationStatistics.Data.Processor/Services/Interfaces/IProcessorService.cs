using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Model;
using Microsoft.Azure.WebJobs;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces
{
    public interface IProcessorService
    {
        Task ProcessUnpackingArchive(Guid importId);

        Task ProcessStage1(Guid importId);

        Task ProcessStage2(Guid importId);

        Task ProcessStage3(Guid importId);

        Task ProcessStage4Messages(Guid importId, ICollector<ImportObservationsMessage> collector);
    }
}
