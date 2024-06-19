using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Services.Interfaces;

public interface IDataSetMetaService
{
    Task<DataSetVersionMappingMeta> ReadDataSetVersionMetaForMappings(
        Guid dataSetVersionId,
        CancellationToken cancellationToken = default);

    Task CreateDataSetVersionMeta(
        Guid dataSetVersionId,
        CancellationToken cancellationToken = default);
}

public record DataSetVersionMappingMeta(
    List<(FilterMeta, List<FilterOptionMeta>)> Filters,
    List<(LocationMeta, List<LocationOptionMetaRow>)> Locations,
    DataSetVersionMetaSummary MetaSummary);
