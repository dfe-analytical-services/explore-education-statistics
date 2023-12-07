using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;

namespace GovUk.Education.ExploreEducationStatistics.Public.Data.Model;

public class LocationMeta
{
    public required GeographicLevel Level { get; set; }

    public required List<LocationOptionMeta> Options { get; set; }
}
