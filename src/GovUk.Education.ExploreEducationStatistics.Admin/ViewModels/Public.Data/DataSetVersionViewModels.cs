#nullable enable
using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Public.Data;

public abstract record DataSetVersionViewModel
{
    public required Guid Id { get; init; }

    public required string Version { get; init; }

    [JsonConverter(typeof(StringEnumConverter))]
    public required DataSetVersionStatus Status { get; init; }

    [JsonConverter(typeof(StringEnumConverter))]
    public required DataSetVersionType Type { get; init; }

    public required IdTitleViewModel File { get; init; }

    public Guid? OriginalFileId { get; set; }
    
    public required IdTitleViewModel ReleaseVersion { get; init; }

    public long TotalResults { get; init; }

    public required string Notes { get; init; }

    public TimePeriodRangeViewModel? TimePeriods { get; init; }

    public IReadOnlyList<string>? GeographicLevels { get; init; }

    public IReadOnlyList<string>? Filters { get; init; }

    public IReadOnlyList<string>? Indicators { get; init; }
}

public record DataSetDraftVersionViewModel : DataSetVersionViewModel
{
    public MappingStatusViewModel? MappingStatus { get; init; }
}

public record DataSetLiveVersionViewModel : DataSetVersionViewModel
{
    public required DateTimeOffset Published { get; init; }

    public new required long TotalResults { get; init; }

    public new required TimePeriodRangeViewModel TimePeriods { get; init; }

    public new required IReadOnlyList<string> GeographicLevels { get; init; } = [];

    public new required IReadOnlyList<string> Filters { get; init; } = [];

    public new required IReadOnlyList<string> Indicators { get; init; } = [];
}

public record DataSetVersionSummaryViewModel
{
    public required Guid Id { get; init; }

    public required string Version { get; init; }

    [JsonConverter(typeof(StringEnumConverter))]
    public required DataSetVersionStatus Status { get; init; }

    [JsonConverter(typeof(StringEnumConverter))]
    public required DataSetVersionType Type { get; init; }

    public required IdTitleViewModel ReleaseVersion { get; init; }

    public required IdTitleViewModel File { get; init; }
}

public record DataSetLiveVersionSummaryViewModel : DataSetVersionSummaryViewModel
{
    public required DateTimeOffset Published { get; init; }
}

public record MajorChangesStatus
{
    public required bool HasDeletionChanges { get; init; } 

    public required bool LocationsHaveMajorChange { get; init; }

    public required bool FiltersHaveMajorChange { get; init; }
    
    public bool IsMajorVersionUpdate => HasDeletionChanges || LocationsHaveMajorChange || FiltersHaveMajorChange;
}

public record MappingStatusViewModel : MajorChangesStatus
{
    public required bool LocationsComplete { get; init; }
    
    public required bool FiltersComplete { get; init; }
}

public record DataSetVersionInfoViewModel 
{
    public required Guid Id { get; init; }

    public required string Version { get; init; }

    [JsonConverter(typeof(StringEnumConverter))]
    public required DataSetVersionStatus Status { get; init; }

    [JsonConverter(typeof(StringEnumConverter))]
    public required DataSetVersionType Type { get; init; }

    public required string Notes { get; init; }
}
