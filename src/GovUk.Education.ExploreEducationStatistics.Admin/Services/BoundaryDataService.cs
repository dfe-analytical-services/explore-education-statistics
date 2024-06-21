using GeoJSON.Net.Feature;
using GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Data.Model;
using GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services;

public class BoundaryDataService : IBoundaryDataService
{
    private readonly IBoundaryLevelRepository _boundaryLevelRepository;
    private readonly IBoundaryDataRepository _boundaryDataRepository;

    public BoundaryDataService(
        IBoundaryLevelRepository boundaryLevelRepository,
        IBoundaryDataRepository boundaryDataRepository)
    {
        _boundaryLevelRepository = boundaryLevelRepository;
        _boundaryDataRepository = boundaryDataRepository;
    }

    public async Task ProcessGeoJson(
        GeographicLevel level,
        string label,
        DateTime published,
        FeatureCollection featureCollection)
    {
        var boundaryLevel = await _boundaryLevelRepository.Create(level, label, published);
        var boundaryData = new List<BoundaryData>();

        foreach (var feature in featureCollection.Features)
        {
            var (name, code) = BoundaryDataUtils.GetFeatureDetails(feature.Properties);

            boundaryData.Add(new()
            {
                BoundaryLevel = boundaryLevel,
                Name = name,
                Code = code,
                GeoJson = JsonConvert.SerializeObject(feature)
            });
        }

        await _boundaryDataRepository.AddRange(boundaryData);
    }
}
