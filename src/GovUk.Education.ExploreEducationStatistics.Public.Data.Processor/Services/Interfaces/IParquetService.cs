using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Services.Interfaces;

public interface IParquetService
{
    Task WriteData(
        DataSetVersion dataSetVersion,
        CancellationToken cancellationToken = default);
}
