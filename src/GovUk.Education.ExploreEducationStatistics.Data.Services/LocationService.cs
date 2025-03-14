#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Data.ViewModels.Meta;
using System.Collections.Generic;
using System.Linq;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services;
public class LocationService : ILocationService
{
    public Dictionary<string, List<LocationAttributeViewModel>> GetLocationViewModels(
            List<Location> locations,
            Dictionary<GeographicLevel, List<string>>? hierarchies)
    {
        return LocationViewModelBuilder.BuildLocationAttributeViewModels(locations, hierarchies)
            .ToDictionary(
                pair => pair.Key.ToString().CamelCase(),
                pair => pair.Value);
    }
}
