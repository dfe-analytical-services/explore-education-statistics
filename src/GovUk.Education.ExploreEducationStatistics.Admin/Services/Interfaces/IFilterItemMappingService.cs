#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;

public interface IFilterItemMappingService
{
    (FilterItemMapping? FilterItemMapping, ErrorViewModel? Error) UpdateFilterItemMapping(
        DataSetMapping dataSetMapping,
        Dictionary<Guid, (FilterGroupMapping FilterGroup, FilterItemMapping FilterItem)> originalItemIdToItemMap,
        Guid originalId,
        Guid? newReplacementId = null
    );

    (
        Dictionary<Guid, FilterItemMapping> FilterItemMappings,
        List<UnmappedFilterItem> UnmappedReplacementItems
    ) AutoMapFilterItemMappings(
        List<FilterItemMapping> itemMappings,
        List<UnmappedFilterItem>? unmappedReplacementItems
    );
}
