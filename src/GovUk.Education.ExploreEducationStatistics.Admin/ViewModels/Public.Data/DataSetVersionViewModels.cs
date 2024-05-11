#nullable enable
using System;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Public.Data;

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
    public required Guid Id { get; init; }

    public required string Version { get; init; }

    [JsonConverter(typeof(StringEnumConverter))]
    public required DataSetVersionStatus Status { get; init; }

    [JsonConverter(typeof(StringEnumConverter))]
    public required DataSetVersionType Type { get; init; }

    public required Guid DataSetFileId { get; init; }

    public required IdTitleViewModel ReleaseVersion { get; init; }
}
