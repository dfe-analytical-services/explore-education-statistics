using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Model;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces
{
    public interface IBatchService
    {
        Task<bool> UpdateStatus(string releaseId, string dataFileName, IStatus status);

        Task FailImport(string releaseId, string dataFileName, List<string> errors);
        
        Task<IStatus> GetStatus(string releaseId, string dataFileName);

        Task CheckComplete(string releaseId, ImportMessage message);
    }
}