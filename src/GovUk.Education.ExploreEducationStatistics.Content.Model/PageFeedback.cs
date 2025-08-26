#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common;
using GovUk.Education.ExploreEducationStatistics.Common.Model;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model;

public record PageFeedback : ICreatedTimestamp<DateTime>
{
    public Guid Id { get; set; }

    public DateTime Created { get; set; }

    public required string Url { get; set; }

    public string? UserAgent { get; set; }

    public PageFeedbackResponse Response { get; set; }

    public string? Context { get; set; }

    public string? Issue { get; set; }

    public string? Intent { get; set; }

    public bool Read { get; set; }
}
