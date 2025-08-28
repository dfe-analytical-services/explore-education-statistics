#nullable enable
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Public.Data;

public record DataSetViewModel
{
    public required Guid Id { get; init; }

    public required string Title { get; init; }

    public required string Summary { get; init; }

    [JsonConverter(typeof(StringEnumConverter))]
    public required DataSetStatus Status { get; init; }

    public Guid? SupersedingDataSetId { get; init; }

    public required DataSetDraftVersionViewModel? DraftVersion { get; init; }

    public required DataSetLiveVersionViewModel? LatestLiveVersion { get; init; }

    public required List<Guid> PreviousReleaseIds { get; init; }
}

public record DataSetSummaryViewModel
{
    public required Guid Id { get; init; }

    public required string Title { get; init; }

    public required string Summary { get; init; }

    [JsonConverter(typeof(StringEnumConverter))]
    public required DataSetStatus Status { get; init; }

    public Guid? SupersedingDataSetId { get; init; }

    public required DataSetVersionSummaryViewModel? DraftVersion { get; init; }

    public required DataSetLiveVersionSummaryViewModel? LatestLiveVersion { get; init; }

    public required List<Guid> PreviousReleaseIds { get; init; }
}
