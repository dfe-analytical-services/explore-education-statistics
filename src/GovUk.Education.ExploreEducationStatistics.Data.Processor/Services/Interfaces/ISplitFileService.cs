using System;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Model;
using Microsoft.Azure.WebJobs;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces
{
    public interface ISplitFileService
    {
        Task SplitDataFileIfRequired(Guid importId);

        Task AddBatchDataFileMessages(
            Guid importId,
            ICollector<ImportObservationsMessage> collector);
    }
}