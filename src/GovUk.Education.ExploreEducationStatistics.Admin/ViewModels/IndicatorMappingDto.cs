#nullable enable
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;

public class IndicatorMappingDto
{
    public Guid OriginalId { get; set; }
    public string OriginalLabel { get; set; } = "";
    public string OriginalColumnName { get; set; } = "";
    public Guid OriginalGroupId { get; set; }
    public string OriginalGroupLabel { get; set; } = "";

    public Guid? ReplacementId { get; set; }
    public string? ReplacementLabel { get; set; }
    public string? ReplacementColumnName { get; set; }
    public Guid? ReplacementGroupId { get; set; }
    public string? ReplacementGroupLabel { get; set; }

    [JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
    public MapStatus Status { get; set; }

    public static IndicatorMappingDto FromModel(IndicatorMapping indicatorMapping)
    {
        return new IndicatorMappingDto
        {
            OriginalId = indicatorMapping.OriginalId,
            OriginalLabel = indicatorMapping.OriginalLabel,
            OriginalColumnName = indicatorMapping.OriginalColumnName,
            OriginalGroupId = indicatorMapping.OriginalGroupId,
            OriginalGroupLabel = indicatorMapping.OriginalGroupLabel,
            ReplacementId = indicatorMapping.ReplacementId,
            ReplacementLabel = indicatorMapping.ReplacementLabel,
            ReplacementColumnName = indicatorMapping.ReplacementColumnName,
            ReplacementGroupId = indicatorMapping.ReplacementGroupId,
            ReplacementGroupLabel = indicatorMapping.ReplacementGroupLabel,
            Status = indicatorMapping.Status,
        };
    }
}
