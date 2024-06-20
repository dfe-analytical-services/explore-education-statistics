using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Processor.Model;

public record DataSetVersionMappingMeta(
    IDictionary<FilterMeta, List<FilterOptionMeta>> Filters,
    IDictionary<LocationMeta, List<LocationOptionMetaRow>> Locations,
    DataSetVersionMetaSummary MetaSummary);
