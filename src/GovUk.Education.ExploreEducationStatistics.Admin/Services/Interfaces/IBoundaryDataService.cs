using GeoJSON.Net.Feature;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using System;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Interfaces;

public interface IBoundaryDataService
{
    Task ProcessGeoJson(
        GeographicLevel level,
        string label,
        DateTime published,
        FeatureCollection featureCollection);
}
