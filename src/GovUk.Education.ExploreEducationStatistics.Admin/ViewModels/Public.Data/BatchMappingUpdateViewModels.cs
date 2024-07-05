#nullable enable
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Public.Data;

public record BatchLocationMappingUpdatesResponseViewModel
{
    public List<LocationMappingUpdateResponseViewModel> Updates { get; init; } = [];
}

public record LocationMappingUpdateResponseViewModel
{
    public GeographicLevel Level { get; init; }

    public string SourceKey { get; init; } = string.Empty;

    public LocationOptionMapping Mapping { get; init; } = null!;
}
