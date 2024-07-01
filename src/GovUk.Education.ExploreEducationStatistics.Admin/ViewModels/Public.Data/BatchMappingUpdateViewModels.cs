#nullable enable
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Public.Data;

// fluent validation
public record BatchLocationMappingUpdatesRequest
{
    public List<LocationMappingUpdateRequest> Updates { get; init; } = [];
}

public record LocationMappingUpdateRequest
{
    public GeographicLevel Level { get; init; }

    public string SourceKey { get; init; } = string.Empty;

    // hierarchy of types to allow only valid combos to exist?
    public string? CandidateKey { get; init; }

    public MappingType Type { get; init; } = MappingType.None;
}

public record LocationMappingUpdateResponse
{
    public GeographicLevel Level { get; init; }

    public string SourceKey { get; init; } = string.Empty;

    public LocationOptionMapping Mapping { get; init; } = null!;
}

public record BatchLocationMappingUpdatesResponseViewModel
{
    public List<LocationMappingUpdateResponse> Updates { get; init; } = [];
}
