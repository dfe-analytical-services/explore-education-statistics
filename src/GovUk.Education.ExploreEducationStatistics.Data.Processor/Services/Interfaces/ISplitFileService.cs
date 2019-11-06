using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Models;
using Microsoft.Azure.WebJobs;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces
{
    public interface ISplitFileService
    {
        Task SplitDataFile(
            ICollector<ImportMessage> collector,
            ImportMessage message,
            SubjectData subjectData,
            BatchSettings batchSettings);
    }
}