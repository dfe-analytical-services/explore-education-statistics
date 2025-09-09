#nullable enable
using Newtonsoft.Json;

namespace GovUk.Education.ExploreEducationStatistics.Admin.ViewModels;

public class EinSummaryViewModel
{
    public Guid Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Slug { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public int Version { get; set; }

    public DateTimeOffset? Published { get; set; }

    [JsonIgnore]
    public int Order { get; set; }
}

public class EinSummaryWithPrevVersionViewModel :  EinSummaryViewModel
{
    public Guid? PreviousVersionId { get; set; }
}
