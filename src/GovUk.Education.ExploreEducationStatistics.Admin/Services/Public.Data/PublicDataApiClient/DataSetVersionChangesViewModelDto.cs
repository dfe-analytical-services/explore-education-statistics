#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Converters.SystemJson;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Model.Data;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Public.Data.PublicDataApiClient;

public record DataSetVersionChangesViewModelDto
{
    public required ChangeSetViewModelDto MajorChanges { get; init; }

    public required ChangeSetViewModelDto MinorChanges { get; init; }
}

public record ChangeSetViewModelDto
{
    public IReadOnlyList<FilterChangeViewModelDto>? Filters { get; init; }

    public IReadOnlyList<FilterOptionChangesViewModelDto>? FilterOptions { get; init; }

    public IReadOnlyList<GeographicLevelChangeViewModelDto>? GeographicLevels { get; init; }

    public IReadOnlyList<IndicatorChangeViewModelDto>? Indicators { get; init; }

    public IReadOnlyList<LocationGroupChangeViewModelDto>? LocationGroups { get; init; }
    
    public IReadOnlyList<LocationOptionChangesViewModelDto>? LocationOptions { get; init; }

    public IReadOnlyList<TimePeriodOptionChangeViewModelDto>? TimePeriods { get; init; }
}

public record FilterOptionChangesViewModelDto
{
    public required FilterViewModelDto Filter { get; init; }

    public required IReadOnlyList<FilterOptionChangeViewModelDto> Options { get; init; }
}

public record LocationOptionChangesViewModelDto
{
    public required GeographicLevelViewModelDto Level { get; init; }

    public required IReadOnlyList<LocationOptionChangeViewModelDto> Options { get; init; }
}

public abstract record ChangeViewModelDto<TChange>
{
    public TChange? CurrentState { get; init; }

    public TChange? PreviousState { get; init; }
}

public record FilterChangeViewModelDto : ChangeViewModelDto<FilterViewModelDto>;

public record FilterOptionChangeViewModelDto : ChangeViewModelDto<FilterOptionViewModelDto>;

public record GeographicLevelChangeViewModelDto : ChangeViewModelDto<GeographicLevelViewModelDto>;

public record IndicatorChangeViewModelDto : ChangeViewModelDto<IndicatorViewModelDto>;

public record LocationGroupChangeViewModelDto : ChangeViewModelDto<LocationGroupViewModelDto>;

public record LocationOptionChangeViewModelDto : ChangeViewModelDto<LocationOptionViewModelDto>;

public record TimePeriodOptionChangeViewModelDto : ChangeViewModelDto<TimePeriodOptionViewModelDto>;

public record FilterViewModelDto
{
    public required string Id { get; init; }

    public required string Column { get; init; }

    public required string Label { get; init; }

    public string Hint { get; init; } = string.Empty;
}

public record FilterOptionViewModelDto
{
    public required string Id { get; init; }

    public required string Label { get; init; }
}

public record GeographicLevelViewModelDto
{
    [JsonConverter(typeof(EnumToEnumValueJsonConverter<GeographicLevel>))]
    public required GeographicLevel Code { get; init; }

    public required string Label { get; init; }
}

public record IndicatorViewModelDto
{
    public required string Id { get; init; }

    public required string Column { get; init; }

    public required string Label { get; init; }

    [JsonConverter(typeof(EnumToEnumLabelJsonConverter<IndicatorUnit>))]
    public required IndicatorUnit? Unit { get; init; }

    public int? DecimalPlaces { get; init; }
}

public record LocationGroupViewModelDto
{
    public required GeographicLevelViewModelDto Level { get; init; }
}

public abstract record LocationOptionViewModelDto
{
    public required string Id { get; init; }

    public required string Label { get; init; }

    public string? Code { get; init; }

    public string? OldCode { get; init; }

    public string? Ukprn { get; init; }

    public string? Urn { get; init; }

    public string? LaEstab { get; init; }
}

public record TimePeriodOptionViewModelDto : TimePeriodViewModelDto
{
    public required string Label { get; init; }
}

public record TimePeriodViewModelDto
{
    [JsonConverter(typeof(EnumToEnumValueJsonConverter<TimeIdentifier>))]
    public required TimeIdentifier Code { get; init; }

    public required string Period { get; init; }
}
