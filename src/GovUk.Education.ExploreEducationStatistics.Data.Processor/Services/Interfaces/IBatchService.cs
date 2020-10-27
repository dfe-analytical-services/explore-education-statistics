using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Model;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces
{
    public interface IBatchService
    {
        Task FailImport(Guid releaseId, string dataFileName, IEnumerable<ValidationError> errors);
        Task UpdateStoredMessage(ImportMessage message);
    }
}