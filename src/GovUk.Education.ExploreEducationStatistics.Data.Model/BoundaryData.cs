#nullable enable
using GeoJSON.Net.Feature;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model;

public class BoundaryData
{
    public int Id { get; set; }

    public required string Code { get; set; }

    public required string Name { get; set; }

    public Feature GeoJson { get; set; } = null!;

    public BoundaryLevel BoundaryLevel { get; set; } = null!;
}
