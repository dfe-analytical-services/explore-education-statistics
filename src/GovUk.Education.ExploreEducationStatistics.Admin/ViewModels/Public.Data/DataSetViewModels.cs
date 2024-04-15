#nullable enable
using System;
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

    public required DataSetVersionViewModel? DraftVersion { get; init; }

    public required DataSetLiveVersionViewModel? LatestLiveVersion { get; init; }

    public Guid? SupersedingDataSetId { get; init; }
}

public record DataSetVersionViewModel
{
    public required Guid Id { get; init; }

    public required string Version { get; init; }

    [JsonConverter(typeof(StringEnumConverter))]
    public required DataSetVersionStatus Status { get; init; }

    [JsonConverter(typeof(StringEnumConverter))]
    public required DataSetVersionType Type { get; init; }
}

public record DataSetLiveVersionViewModel : DataSetVersionViewModel
{
    public required DateTimeOffset Published { get; init; }
}

public record DataSetVersionSummaryViewModel
{
    public required string Title { get; init; }

    public required Release Release { get; init; }

    public required string Version { get; init; }

    [JsonConverter(typeof(StringEnumConverter))]
    public required DataSetVersionType Type { get; init; }

    [JsonConverter(typeof(StringEnumConverter))]
    public required DataSetVersionStatus Status { get; init; }

    public required Guid DataSetFileId { get; init; }
}

public record Release
{
    public required Guid Id { get; init; }

    public required string Title { get; init; }
}
