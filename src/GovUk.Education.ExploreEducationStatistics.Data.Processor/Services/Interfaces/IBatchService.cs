using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Services;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Model;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces
{
    public interface IBatchService
    {
        Task<bool> UpdateStatus(string releaseId, string dataFileName, IStatus status);

        Task FailImport(string releaseId, string dataFileName, IEnumerable<ValidationError> errors);
        
        Task<IStatus> GetStatus(string releaseId, string dataFileName);

        Task CheckComplete(string releaseId, ImportMessage message, StatisticsDbContext context);

        Task UpdateStoredMessage(ImportMessage message);

        Task CreateImport(string releaseId, string dataFileName, int numberOfRows, ImportMessage message);
    }
}