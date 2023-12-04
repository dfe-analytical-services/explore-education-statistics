using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model;

public class LocationMeta
{
    public GeographicLevel Level { get; set; }

    public List<LocationOptionMeta> Options { get; set; } = new();
}
