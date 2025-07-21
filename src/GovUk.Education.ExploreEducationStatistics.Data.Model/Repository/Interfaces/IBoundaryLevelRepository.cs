#nullable enable
using GeoJSON.Net.Feature;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model.Repository.Interfaces;

public interface IBoundaryLevelRepository
{
    Task<BoundaryLevel> CreateBoundaryLevel(
        GeographicLevel level,
        string label,
        DateTime published,
        FeatureCollection featureCollection,
        CancellationToken cancellationToken = default);

    IEnumerable<BoundaryLevel> FindByGeographicLevels(IEnumerable<GeographicLevel> geographicLevels);
}
