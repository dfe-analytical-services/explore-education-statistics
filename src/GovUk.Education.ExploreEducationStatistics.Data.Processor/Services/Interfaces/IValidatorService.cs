using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Model;
using Microsoft.Azure.WebJobs;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces
{
    public interface IValidatorService
    {
        Task<Either<IEnumerable<ValidationError>, ProcessorStatistics>> Validate(DataTable metaTable,
            DataTable csvTable, ExecutionContext executionContext, ImportMessage message);
    }
}