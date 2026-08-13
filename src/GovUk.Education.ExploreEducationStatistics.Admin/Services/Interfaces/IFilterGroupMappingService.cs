#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;

public interface IFilterGroupMappingService
{
    (FilterGroupMapping? FilterGroupMapping, ErrorViewModel? Error) UpdateFilterGroupMapping(
        DataSetMapping dataSetMapping,
        Dictionary<Guid, (FilterMapping FilterMap, FilterGroupMapping GroupMap)> originalGroupIdToGroupMap,
        Guid originalId,
        Guid? newReplacementId = null
    );

    (
        Dictionary<Guid, FilterGroupMapping> FilterGroupMappings,
        List<UnmappedFilterGroup> UnmappedReplacementGroups
    ) AutoMapFilterGroupMappings(
        List<FilterGroupMapping> groupMappings,
        List<UnmappedFilterGroup>? unmappedReplacementGroups
    );
}
