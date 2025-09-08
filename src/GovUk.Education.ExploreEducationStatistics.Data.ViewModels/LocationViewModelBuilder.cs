using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.ViewModels.Meta;

namespace GovUk.Education.ExploreEducationStatistics.Data.ViewModels;

public static class LocationViewModelBuilder
{
    public static Dictionary<GeographicLevel, List<LocationAttributeViewModel>> BuildLocationAttributeViewModels(
        IList<Location> locations,
        Dictionary<GeographicLevel, List<string>>? hierarchies,
        Dictionary<GeographicLevel, Dictionary<string, BoundaryData>>? boundaryData = null
    )
    {
        var locationAttributes = locations.GetLocationAttributesHierarchical(hierarchies);
        return locationAttributes.ToDictionary(
            levelAndLocationAttributes => levelAndLocationAttributes.Key,
            levelAndLocationAttributes =>
            {
                var boundaryDataByCode = boundaryData?.GetValueOrDefault(levelAndLocationAttributes.Key);
                return BuildLocationAttributeViewModels(levelAndLocationAttributes.Value, boundaryDataByCode);
            }
        );
    }

    private static List<LocationAttributeViewModel> BuildLocationAttributeViewModels(
        IEnumerable<LocationAttributeNode> locationAttributes,
        IReadOnlyDictionary<string, BoundaryData>? boundaryDataByCode
    )
    {
        return DeduplicateLocationViewModels(
            locationAttributes
                .OrderBy(OrderingKey)
                .Select(locationAttribute => BuildLocationAttributeViewModel(locationAttribute, boundaryDataByCode))
                .ToList()
        );
    }

    private static LocationAttributeViewModel BuildLocationAttributeViewModel(
        LocationAttributeNode locationAttributeNode,
        IReadOnlyDictionary<string, BoundaryData>? boundaryDataByCode
    )
    {
        var locationAttribute = locationAttributeNode.Attribute;

        // If we are including GeoJson data, it's only included in leaf nodes
        if (!locationAttributeNode.IsLeaf)
        {
            return new LocationAttributeViewModel
            {
                Label = locationAttribute.Name ?? string.Empty,
                Level = locationAttribute.GeographicLevel,
                Value = locationAttribute.GetCodeOrFallback(),
                Options = BuildLocationAttributeViewModels(locationAttributeNode.Children, boundaryDataByCode),
            };
        }

        var code = locationAttribute.GetCodeOrFallback();

        var feature = code.IsNullOrEmpty() ? null : boundaryDataByCode?.GetValueOrDefault(code)?.GeoJson;

        if (feature != null)
        {
            feature.Properties.TryAdd("Code", BoundaryDataUtils.GetCode(feature.Properties));
            feature.Properties.TryAdd("Name", BoundaryDataUtils.GetName(feature.Properties));
        }

        return new LocationAttributeViewModel
        {
            Id = locationAttributeNode.LocationId,
            GeoJson = feature,
            Label = locationAttribute.Name ?? string.Empty,
            Value = code,
        };
    }

    private static List<LocationAttributeViewModel> DeduplicateLocationViewModels(
        List<LocationAttributeViewModel> locations
    )
    {
        // Find duplicates by label which also have the same value.
        // They are unique by some other attribute of location and appending the value of this attribute won't deduplicate them.
        // TODO EES-2954 enhance the deduplication to append other attributes of the location
        var nonDistinctByLabelAndValue = locations
            .GroupBy(model => (model.Value, model.Label))
            .Where(grouping => grouping.Count() > 1)
            .SelectMany(grouping => grouping)
            .ToList();

        // Excluding those, find duplicates due to having the same label
        var nonDistinctByLabel = locations
            .Except(nonDistinctByLabelAndValue)
            .GroupBy(model => model.Label.ToLower())
            .Where(grouping => grouping.Count() > 1)
            .SelectMany(grouping => grouping)
            .ToList();

        nonDistinctByLabel.ForEach(location => location.Label += $" ({location.Value})");

        return locations;
    }

    private static string OrderingKey(LocationAttributeNode node)
    {
        var locationAttribute = node.Attribute;

        return locationAttribute switch
        {
            Region region => region.Code ?? string.Empty,
            _ => locationAttribute.Name ?? string.Empty,
        };
    }
}
