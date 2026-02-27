using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Public.Data;

/// <summary>
/// This record provides a structure into which we can read the mapping types (e.g. AutoMapped)
/// for each LocationOption, combined with that of its parent LocationLevel's mapping type.
///
/// We use jsonb queries to populate the raw string properties in this record and provide
/// convenience methods to get the actual <see cref="MappingType"/> enum values from those
/// raw strings.
/// </summary>
public record LocationMappingTypes
{
    public required string LocationLevelMappingTypeString { get; init; }

    public required string LocationOptionMappingTypeString { get; init; }

    public MappingType LocationLevelMappingType =>
        EnumUtil.GetFromEnumValue<MappingType>(LocationLevelMappingTypeString);

    public MappingType LocationOptionMappingType =>
        EnumUtil.GetFromEnumValue<MappingType>(LocationOptionMappingTypeString);
}

/// <summary>
/// This record provides a structure into which we can read the mapping types (e.g. AutoMapped)
/// for each FilterOption, combined with that of its parent Filter's mapping type.
///
/// We use jsonb queries to populate the raw string properties in this record and provide
/// convenience methods to get the actual <see cref="MappingType"/> enum values from those
/// raw strings.
/// </summary>
public record FilterMappingTypes
{
    public required string FilterMappingTypeString { get; init; }

    public required string FilterOptionMappingTypeString { get; init; }

    public MappingType FilterMappingType => EnumUtil.GetFromEnumValue<MappingType>(FilterMappingTypeString);

    public MappingType FilterOptionMappingType => EnumUtil.GetFromEnumValue<MappingType>(FilterOptionMappingTypeString);
}

/// <summary>
/// This record provides a structure into which we can read the mapping types (e.g. AutoMapped)
/// for each Indicator.
///
/// We use jsonb queries to populate the raw string properties in this record and provide
/// convenience methods to get the actual <see cref="MappingType"/> enum values from those
/// raw strings.
/// </summary>
public record IndicatorMappingTypes
{
    public required string IndicatorMappingTypeString { get; init; }

    public MappingType IndicatorMappingType => EnumUtil.GetFromEnumValue<MappingType>(IndicatorMappingTypeString);
}
