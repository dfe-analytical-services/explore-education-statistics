#nullable enable
using GeoJSON.Net.Feature;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

public class BoundaryDataService(
    IBoundaryLevelRepository boundaryLevelRepository,
    IBoundaryDataRepository boundaryDataRepository) : IBoundaryDataService
{
    public async Task ProcessGeoJson(
        GeographicLevel level,
        string label,
        DateTime published,
        FeatureCollection featureCollection)
    {
        var boundaryLevel = await boundaryLevelRepository.CreateBoundaryLevel(level, label, published);
        var boundaryData = featureCollection.Features
            .Select(feature =>
            {
                return new BoundaryData
                {
                    BoundaryLevel = boundaryLevel,
                    Code = BoundaryDataUtils.GetCode(feature.Properties),
                    Name = BoundaryDataUtils.GetName(feature.Properties),
                    GeoJson = feature
                };
            }).ToList();

        await boundaryDataRepository.AddRange(boundaryData);
    }
}
