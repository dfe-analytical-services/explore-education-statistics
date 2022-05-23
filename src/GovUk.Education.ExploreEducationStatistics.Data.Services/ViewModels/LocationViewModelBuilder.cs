#nullable enable
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels.Meta;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels;

public class LocationViewModelBuilder
{
    public static Dictionary<GeographicLevel, List<LocationAttributeViewModel>> BuildLocationAttributeViewModels(
        IList<Location> locations,
        Dictionary<GeographicLevel, List<string>>? hierarchies,
        Dictionary<GeographicLevel, Dictionary<string, GeoJson>>? geoJson = null)
    {
        var locationAttributes = locations.GetLocationAttributesHierarchical(hierarchies);
        return locationAttributes.ToDictionary(
            pair => pair.Key,
            pair =>
            {
                var geoJsonByCode = geoJson?.GetValueOrDefault(pair.Key);
                return BuildLocationAttributeViewModels(pair.Value, geoJsonByCode);
            });
    }

    private static List<LocationAttributeViewModel> BuildLocationAttributeViewModels(
        IEnumerable<LocationAttributeNode> locationAttributes,
        IReadOnlyDictionary<string, GeoJson>? geoJsonByCode)
    {
        return DeduplicateLocationViewModels(
            locationAttributes
                .OrderBy(OrderLocationAttributes)
                .Select(locationAttribute => BuildLocationAttributeViewModel(locationAttribute, geoJsonByCode))
                .ToList()
        );
    }

    private static LocationAttributeViewModel BuildLocationAttributeViewModel(
        LocationAttributeNode locationAttributeNode,
        IReadOnlyDictionary<string, GeoJson>? geoJsonByCode)
    {
        var locationAttribute = locationAttributeNode.Attribute;
        return locationAttributeNode.IsLeaf
            ? BuildLocationAttributeLeafViewModel(locationAttributeNode, geoJsonByCode)
            : new LocationAttributeViewModel
            {
                Label = locationAttribute.Name ?? string.Empty,
                Level = locationAttribute.GeographicLevel,
                Value = locationAttribute.GetCodeOrFallback(),
                Options = BuildLocationAttributeViewModels(locationAttributeNode.Children, geoJsonByCode)
            };
    }

    private static LocationAttributeViewModel BuildLocationAttributeLeafViewModel(
        LocationAttributeNode locationAttributeNode,
        IReadOnlyDictionary<string, GeoJson>? geoJsonByCode)
    {
        var locationAttribute = locationAttributeNode.Attribute;
        var code = locationAttribute.GetCodeOrFallback();

        var geoJson = code.IsNullOrEmpty()
            ? null
            : geoJsonByCode?.GetValueOrDefault(code)?.Deserialized;

        return new LocationAttributeViewModel
        {
            Id = locationAttributeNode.LocationId,
            GeoJson = geoJson,
            Label = locationAttribute.Name ?? string.Empty,
            Value = code
        };
    }

    private static List<LocationAttributeViewModel> DeduplicateLocationViewModels(
        List<LocationAttributeViewModel> locations)
    {
        // Find duplicates by label which also have the same value.
        // They are unique by some other attribute of location and appending the value of this attribute won't deduplicate them.
        // TODO EES-2954 enhance the deduplication to append other attributes of the location
        var notDistinctByLabelAndValue = locations
            .GroupBy(model => (model.Value, model.Label))
            .Where(grouping => grouping.Count() > 1)
            .SelectMany(grouping => grouping)
            .ToList();

        // Excluding those, find duplicates due to having the same label
        var notDistinctByLabel = locations
            .Except(notDistinctByLabelAndValue)
            .GroupBy(model => model.Label.ToLower())
            .Where(grouping => grouping.Count() > 1)
            .SelectMany(grouping => grouping)
            .ToList();

        if (!notDistinctByLabel.Any())
        {
            return locations;
        }

        return locations.Select(location =>
        {
            if (notDistinctByLabel.Contains(location))
            {
                // Append the value to make them differentiable by label
                location.Label += $" ({location.Value})";
            }

            return location;
        }).ToList();
    }

    private static string OrderLocationAttributes(LocationAttributeNode node)
    {
        var locationAttribute = node.Attribute;

        return locationAttribute switch
        {
            Region region => region.Code ?? string.Empty,
            _ => locationAttribute.Name ?? string.Empty
        };
    }
}
