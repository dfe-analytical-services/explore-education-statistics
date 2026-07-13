#nullable enable
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;

public class FiltersMappingDto
{
    public List<FilterMappingDto> Filters { get; set; }
    public List<FilterGroupMappingDto> FilterGroups { get; set; }
    public List<FilterItemMappingDto> FilterItems { get; set; }
}

public class FilterMappingDto
{
    public Guid OriginalId { get; set; }
    public string OriginalLabel { get; set; } = "";
    public string OriginalColumnName { get; set; } = "";

    public Guid? ReplacementId { get; set; }
    public string? ReplacementLabel { get; set; }
    public string? ReplacementColumnName { get; set; }

    public string Status { get; set; } = "";

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
            Status = filterMapping.Status.ToString(),
        };
    }
}

public class FilterGroupMappingDto
{
    public Guid OriginalId { get; set; }
    public string OriginalLabel { get; set; } = "";

    public Guid? ReplacementId { get; set; }
    public string? ReplacementLabel { get; set; }

    public string Status { get; set; } = "";

    public static FilterGroupMappingDto FromModel(FilterGroupMapping filterGroupMapping)
    {
        return new FilterGroupMappingDto
        {
            OriginalId = filterGroupMapping.OriginalId,
            OriginalLabel = filterGroupMapping.OriginalLabel,
            ReplacementId = filterGroupMapping.ReplacementId,
            ReplacementLabel = filterGroupMapping.ReplacementLabel,
            Status = filterGroupMapping.Status.ToString(),
        };
    }
}

public class FilterItemMappingDto
{
    public Guid OriginalId { get; set; }
    public string OriginalLabel { get; set; } = "";

    public Guid? ReplacementId { get; set; }
    public string? ReplacementLabel { get; set; }

    public string Status { get; set; } = "";

    public static FilterItemMappingDto FromModel(FilterItemMapping filterItemMapping)
    {
        return new FilterItemMappingDto
        {
            OriginalId = filterItemMapping.OriginalId,
            OriginalLabel = filterItemMapping.OriginalLabel,
            ReplacementId = filterItemMapping.ReplacementId,
            ReplacementLabel = filterItemMapping.ReplacementLabel,
            Status = filterItemMapping.Status.ToString(),
        };
    }
}
