#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Utils;
using GovUk.Education.ExploreEducationStatistics.Public.Data.Model;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels.Public.Data;

public record PreviewTokenViewModel
{
    public Guid Id { get; set; } = UuidUtils.UuidV7();

    public required string Label { get; set; }

    [JsonConverter(typeof(StringEnumConverter))]
    public required PreviewTokenStatus Status { get; set; }

    public required string CreatedByEmail { get; set; }

    public required DateTimeOffset Created { get; set; }

    public required DateTimeOffset Expiry { get; set; }

    public required DateTimeOffset? Updated { get; set; }
    
    public required DateTimeOffset Activates { get; set; }
}
