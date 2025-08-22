#nullable enable
using GovUk.Education.ExploreEducationStatistics.Content.Model;

namespace GovUk.Education.ExploreEducationStatistics.Admin.Requests;

public record EinContentBlockAddRequest
{
    public EinBlockType Type { get; set; }

    public int? Order { get; set; }
}

public record EinContentBlockUpdateRequest
{
    public string? Heading { get; set; }

    public string Body { get; set; } = string.Empty;
}
