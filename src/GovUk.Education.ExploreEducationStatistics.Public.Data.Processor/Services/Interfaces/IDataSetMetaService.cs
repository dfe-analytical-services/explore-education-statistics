using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Services.Interfaces;

public interface IDataSetMetaService
{
    Task CreateDataSetVersionMeta(
        DataSetVersion dataSetVersion,
        CancellationToken cancellationToken = default);
}
