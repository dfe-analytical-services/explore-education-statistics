using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces;

public interface IValidatorService
{
    Task<Either<List<DataImportError>, ProcessorStatistics>> Validate(Guid importId);
}
