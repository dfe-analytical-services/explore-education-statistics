using GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Models;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Services.Interfaces;

public interface IDataSetMetaService
{
    Task<DataSetVersionMappingMeta> ReadDataSetVersionMappingMeta(
        Guid dataSetVersionId,
        CancellationToken cancellationToken = default
    );

    Task CreateDataSetVersionMeta(Guid dataSetVersionId, CancellationToken cancellationToken = default);
}
