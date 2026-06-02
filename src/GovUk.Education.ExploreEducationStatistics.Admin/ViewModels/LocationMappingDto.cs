#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;

public class LocationMappingDto
{
    public Guid OriginalId { get; set; }
    public string OriginalGeographicLevel { get; set; } = "";
    public string OriginalCode { get; set; } = "";
    public string OriginalName { get; set; } = "";

    public Guid? ReplacementId { get; set; }
    public string? ReplacementGeographicLevel { get; set; }
    public string? ReplacementCode { get; set; }
    public string? ReplacementName { get; set; }

    public string Status { get; set; } = "";

    public static LocationMappingDto FromModel(LocationMapping locationMapping)
    {
        return new LocationMappingDto
        {
            OriginalId = locationMapping.OriginalId,
            OriginalGeographicLevel = locationMapping.OriginalGeographicLevel.GetEnumValue(),
            OriginalCode = locationMapping.OriginalCode,
            OriginalName = locationMapping.OriginalName,
            ReplacementId = locationMapping.ReplacementId,
            ReplacementGeographicLevel = locationMapping.ReplacementGeographicLevel?.GetEnumValue(),
            ReplacementCode = locationMapping.ReplacementCode,
            ReplacementName = locationMapping.ReplacementName,
            Status = locationMapping.Status.ToString(),
        };
    }
}
