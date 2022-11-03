using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Microsoft.Azure.WebJobs;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces
{
    public interface IValidatorService
    {
        Task<Either<List<DataImportError>, ProcessorStatistics>> Validate(Guid importId);
    }
}