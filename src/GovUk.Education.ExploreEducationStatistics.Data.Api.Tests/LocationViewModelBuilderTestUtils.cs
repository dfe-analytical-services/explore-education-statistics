#nullable enable
using System.Collections.Generic;
using System.Linq;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Data.Services.ViewModels.Meta;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Tests;

// TODO EES-3755 Remove after Permalink snapshot migration work is complete
internal static class LocationViewModelBuilderTestUtils
{
    public static Dictionary<GeographicLevel, List<LocationAttributeViewModel>>
        BuildLocationAttributeViewModelsWithoutLocationIds(
            IList<Location> locations,
            Dictionary<GeographicLevel, List<string>>? hierarchies,
            Dictionary<GeographicLevel, Dictionary<string, GeoJson>>? geoJson = null)
    {
        var viewModels = LocationViewModelBuilder.BuildLocationAttributeViewModels(locations, hierarchies, geoJson);

        // Deliberately remove the location id's from options to represent permalink data prior to location id's
        // being added to subject meta data.

        return viewModels.ToDictionary(
            pair => pair.Key,
            pair => StripLocationIds(pair.Value)
        );
    }

    private static List<LocationAttributeViewModel> StripLocationIds(List<LocationAttributeViewModel> attributes)
    {
        return attributes.Select(attribute => attribute with
        {
            Id = null,
            Options = attribute.Options == null ? null : StripLocationIds(attribute.Options)
        }).ToList();
    }
}
