using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Processor.Models;
using Microsoft.Azure.WebJobs;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces
{
    public interface IValidatorService
    {
        Task<Either<IEnumerable<ValidationError>, ProcessorStatistics>> Validate(
            Guid releaseId,
            SubjectData subjectData,
            ExecutionContext executionContext,
            ImportMessage message);
    }
}