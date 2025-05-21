using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Services.Public.Data;

public record FilterMappingTypes
{
    public required string FilterRaw { get; init; }

    public required string FilterOptionRaw { get; init; }

    public MappingType Filter => EnumUtil.GetFromEnumValue<MappingType>(FilterRaw);

    public MappingType FilterOption => EnumUtil.GetFromEnumValue<MappingType>(FilterOptionRaw);
}

public record LocationMappingTypes
{
    public required string LocationLevelRaw { get; init; }

    public required string LocationOptionRaw { get; init; }

    public MappingType LocationLevel => EnumUtil.GetFromEnumValue<MappingType>(LocationLevelRaw);

    public MappingType LocationOption => EnumUtil.GetFromEnumValue<MappingType>(LocationOptionRaw);
}
