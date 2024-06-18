using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Services.Interfaces;

public interface IDataSetMetaService
{
    Task<(
        List<(FilterMeta, List<FilterOptionMeta>)> filters,
        List<(LocationMeta, List<LocationOptionMetaRow>)> locations,
        DataSetVersionMetaSummary metaSummary
        )> ReadDataSetVersionMetaForMappings(
        Guid dataSetVersionId,
        CancellationToken cancellationToken = default);

    Task CreateDataSetVersionMeta(
        Guid dataSetVersionId,
        CancellationToken cancellationToken = default);
}
