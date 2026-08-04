#nullable enable
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;

public class FiltersMappingDto
{
    public List<FilterMappingDto> Filters { get; set; } = null!;
    public List<FilterGroupMappingDto> FilterGroups { get; set; } = null!;
    public List<FilterItemMappingDto> FilterItems { get; set; } = null!;
}

public class FilterMappingDto
{
    public Guid OriginalId { get; set; }
    public string OriginalLabel { get; set; } = "";
    public string OriginalColumnName { get; set; } = "";

    public Guid? ReplacementId { get; set; }
    public string? ReplacementLabel { get; set; }
    public string? ReplacementColumnName { get; set; }

    [JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
    public MapStatus Status { get; set; }

    public static FilterMappingDto FromModel(FilterMapping filterMapping)
    {
        return new FilterMappingDto
        {
            OriginalId = filterMapping.OriginalId,
            OriginalLabel = filterMapping.OriginalLabel,
            OriginalColumnName = filterMapping.OriginalColumnName,
            ReplacementId = filterMapping.ReplacementId,
            ReplacementLabel = filterMapping.ReplacementLabel,
            ReplacementColumnName = filterMapping.ReplacementColumnName,
            Status = filterMapping.Status,
        };
    }
}

public class FilterGroupMappingDto
{
    public Guid OriginalId { get; set; }
    public string OriginalLabel { get; set; } = "";

    public Guid? ReplacementId { get; set; }
    public string? ReplacementLabel { get; set; }

    [JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
    public MapStatus Status { get; set; }

    public static FilterGroupMappingDto FromModel(FilterGroupMapping filterGroupMapping)
    {
        return new FilterGroupMappingDto
        {
            OriginalId = filterGroupMapping.OriginalId,
            OriginalLabel = filterGroupMapping.OriginalLabel,
            ReplacementId = filterGroupMapping.ReplacementId,
            ReplacementLabel = filterGroupMapping.ReplacementLabel,
            Status = filterGroupMapping.Status,
        };
    }
}

public class FilterItemMappingDto
{
    public Guid OriginalId { get; set; }
    public string OriginalLabel { get; set; } = "";

    public Guid? ReplacementId { get; set; }
    public string? ReplacementLabel { get; set; }

    [JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
    public MapStatus Status { get; set; }

    public static FilterItemMappingDto FromModel(FilterItemMapping filterItemMapping)
    {
        return new FilterItemMappingDto
        {
            OriginalId = filterItemMapping.OriginalId,
            OriginalLabel = filterItemMapping.OriginalLabel,
            ReplacementId = filterItemMapping.ReplacementId,
            ReplacementLabel = filterItemMapping.ReplacementLabel,
            Status = filterItemMapping.Status,
        };
    }
}
