#nullable enable
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Public.Data;

// fluent validation
public record BatchLocationMappingUpdatesRequest
{
    public List<LocationMappingUpdate> Updates { get; init; } = [];
}

public record LocationMappingUpdate
{
    public GeographicLevel Level { get; init; }

    public string SourceKey { get; init; } = string.Empty;

    // hierarchy of types to allow only valid combos to exist?
    public string CandidateKey { get; init; } = string.Empty;

    public MappingType Type { get; init; } = MappingType.None;
}

public record BatchMappingUpdatesResponseViewModel
{
    public Dictionary<string, LocationMappingUpdateResult> Results { get; init; } = [];
}

public record LocationMappingUpdateResult
{
    public string CandidateKey { get; init; } = string.Empty;

    public MappingType Type { get; init; } = MappingType.None;
}
