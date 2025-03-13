#nullable enable
using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using GovUk.Education.ExploreEducationStatistics.Common.Converters;

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

public record MappingStatusViewModel
{
    public required bool LocationsComplete { get; init; }
    
    public required bool FiltersComplete { get; init; }
}

public record DataSetVersionChangesViewModel
{
    public required IdTitleViewModel DataSet { get; init; }

    public required DataSetVersionViewModel2 DataSetVersion { get; init; }

    public required DataSetVersionChangesViewModel2 Changes { get; init; }
}

public record DataSetVersionViewModel2
{
    public required Guid Id { get; init; }

    public required string Version { get; init; }

    [JsonConverter(typeof(StringEnumConverter))]
    public required DataSetVersionStatus Status { get; init; }

    [JsonConverter(typeof(StringEnumConverter))]
    public required DataSetVersionType Type { get; init; }

    public required string Notes { get; init; }
}

public record DataSetVersionChangesViewModel2
{
    public required ChangeSetViewModel MajorChanges { get; init; }

    public required ChangeSetViewModel MinorChanges { get; init; }
}

public record ChangeSetViewModel
{
    public IReadOnlyList<FilterChangeViewModel>? Filters { get; init; }

    public IReadOnlyList<FilterOptionChangesViewModel>? FilterOptions { get; init; }

    public IReadOnlyList<GeographicLevelChangeViewModel>? GeographicLevels { get; init; }

    public IReadOnlyList<IndicatorChangeViewModel>? Indicators { get; init; }

    public IReadOnlyList<LocationGroupChangeViewModel>? LocationGroups { get; init; }

    public IReadOnlyList<LocationOptionChangesViewModel>? LocationOptions { get; init; }

    public IReadOnlyList<TimePeriodOptionChangeViewModel>? TimePeriods { get; init; }
}

public record FilterOptionChangesViewModel
{
    public required FilterViewModel Filter { get; init; }

    public required IReadOnlyList<FilterOptionChangeViewModel> Options { get; init; }
}

public record LocationOptionChangesViewModel
{
    public required GeographicLevelViewModel Level { get; init; }

    public required IReadOnlyList<LocationOptionChangeViewModel> Options { get; init; }
}

public abstract record ChangeViewModel<TChange>
{
    public TChange? CurrentState { get; init; }

    public TChange? PreviousState { get; init; }
}

public record FilterChangeViewModel : ChangeViewModel<FilterViewModel>;

public record FilterOptionChangeViewModel : ChangeViewModel<FilterOptionViewModel>;

public record GeographicLevelChangeViewModel : ChangeViewModel<GeographicLevelViewModel>;

public record IndicatorChangeViewModel : ChangeViewModel<IndicatorViewModel>;

public record LocationGroupChangeViewModel : ChangeViewModel<LocationGroupViewModel>;

public record LocationOptionChangeViewModel : ChangeViewModel<LocationOptionViewModel>;

public record TimePeriodOptionChangeViewModel : ChangeViewModel<TimePeriodOptionViewModel>;

public record FilterViewModel
{
    public required string Id { get; init; }

    public required string Column { get; init; }

    public required string Label { get; init; }

    public string Hint { get; init; } = string.Empty;
}

public record FilterOptionViewModel
{
    public required string Id { get; init; }

    public required string Label { get; init; }
}

public record GeographicLevelViewModel
{
    [JsonConverter(typeof(EnumToEnumValueJsonConverter<GeographicLevel>))]
    public required GeographicLevel Code { get; init; }

    public required string Label { get; init; }
}

public record IndicatorViewModel
{
    public required string Id { get; init; }

    public required string Column { get; init; }

    public required string Label { get; init; }

    [JsonConverter(typeof(EnumToEnumLabelJsonConverter<IndicatorUnit>))]
    public required IndicatorUnit? Unit { get; init; }

    public int? DecimalPlaces { get; init; }
}

public record LocationGroupViewModel
{
    public required GeographicLevelViewModel Level { get; init; }
}

public abstract record LocationOptionViewModel
{
    public required string Id { get; init; }

    public required string Label { get; init; }

    public string? Code { get; init; }

    public string? OldCode { get; init; }

    public string? Ukprn { get; init; }

    public string? Urn { get; init; }

    public string? LaEstab { get; init; }
}

public record TimePeriodOptionViewModel : TimePeriodViewModel
{
    public required string Label { get; init; }
}

public record TimePeriodViewModel
{
    [JsonConverter(typeof(EnumToEnumValueJsonConverter<TimeIdentifier>))]
    public required TimeIdentifier Code { get; init; }

    public required string Period { get; init; }
}
