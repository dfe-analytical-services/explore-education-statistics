#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Data.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Data.ViewModels.Meta;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services;
public class LocationService(
    StatisticsDbContext context,
    IBoundaryDataRepository boundaryDataRepository
    ) : ILocationService
{
    public async Task<Dictionary<string, List<LocationAttributeViewModel>>> GetLocationViewModels(
            List<Location> locations,
            Dictionary<GeographicLevel, List<string>>? hierarchies,
            long? boundaryLevelId = null)
    {
        var boundaryData = await GetBoundaryData(locations, boundaryLevelId);

        return LocationViewModelBuilder.BuildLocationAttributeViewModels(locations, hierarchies, boundaryData)
            .ToDictionary(
                pair => pair.Key.ToString().CamelCase(),
                pair => pair.Value);
    }

    private async Task<Dictionary<GeographicLevel, Dictionary<string, BoundaryData>>> GetBoundaryData(
        List<Location> locations,
        long? boundaryLevelId)
    {
        if (!boundaryLevelId.HasValue)
        {
            return [];
        }

        var boundaryLevel = await context.BoundaryLevel.FindAsync(boundaryLevelId)
                            ?? throw new ArgumentOutOfRangeException(nameof(boundaryLevelId));

        var locationsMatchingLevel =
            locations.Where(location => location.GeographicLevel == boundaryLevel.Level);

        var codes = locationsMatchingLevel
            .Select(location => location.ToLocationAttribute().GetCodeOrFallback())
            .ToList();

        var boundaryData = boundaryDataRepository.FindByBoundaryLevelAndCodes(boundaryLevelId.Value, codes);

        return new Dictionary<GeographicLevel, Dictionary<string, BoundaryData>>
        {
            {
                boundaryLevel.Level,
                boundaryData
            }
        };
    }
}
