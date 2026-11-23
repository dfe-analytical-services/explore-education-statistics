#nullable enable
using GovUk.Education.ExploreEducationStatistics.Data.Storage.Interfaces;

namespace GovUk.Education.ExploreEducationStatistics.Data.Processor.Services.Interfaces;

public interface IDataSetMappingService
{
    Task CreateInitialDataSetMappingIfReplacement(Guid replacementFileId, IStorageDataSet replacementDataSet);
}
