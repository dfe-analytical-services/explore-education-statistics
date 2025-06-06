#nullable enable
using GeoJSON.Net.Feature;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Database;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Repository;

public class BoundaryLevelRepository(
    StatisticsDbContext context) : IBoundaryLevelRepository
{
    public IEnumerable<BoundaryLevel> FindByGeographicLevels(IEnumerable<GeographicLevel> geographicLevels)
    {
        return context.BoundaryLevel
            .Where(level => geographicLevels.Contains(level.Level))
            .OrderByDescending(level => level.Published);
    }

    public async Task<BoundaryLevel> CreateBoundaryLevel(
        GeographicLevel level,
        string label,
        DateTime published,
        FeatureCollection featureCollection,
        CancellationToken cancellationToken = default)
    {
        return await context.RequireTransaction(async () =>
        {
            var entry = await context.BoundaryLevel.AddAsync(new()
            {
                Level = level,
                Label = label,
                Published = published,
            });

            var boundaryLevel = entry.Entity;
            var boundaryData = MapBoundaryDataFromCollection(featureCollection, boundaryLevel);

            await context.BoundaryData.AddRangeAsync(boundaryData, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);

            return boundaryLevel;
        });
    }

    private static List<BoundaryData> MapBoundaryDataFromCollection(
        FeatureCollection featureCollection,
        BoundaryLevel boundaryLevel)
    {
        return featureCollection.Features
            .Select(feature => new BoundaryData
            {
                BoundaryLevel = boundaryLevel,
                Code = BoundaryDataUtils.GetCode(feature.Properties),
                Name = BoundaryDataUtils.GetName(feature.Properties),
                GeoJson = feature
            }).ToList();
    }
}
