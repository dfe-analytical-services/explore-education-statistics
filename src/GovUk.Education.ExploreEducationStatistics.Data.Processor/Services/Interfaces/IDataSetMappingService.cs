#nullable enable

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces;

public interface IDataSetMappingService
{
    Task CreateInitialDataSetMappingIfReplacement(Guid replacementFileId);
}
