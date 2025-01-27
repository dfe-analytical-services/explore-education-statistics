#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using System;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model;

public record Feedback : ICreatedTimestamp<DateTime>
{
    public Guid Id { get; set; }

    public DateTime Created { get; set; }

    public required string Url { get; set; }

    public string? UserAgent { get; set; }

    public FeedbackResponse Response { get; set; }

    public string? Context { get; set; }

    public string? Issue { get; set; }

    public string? Intent { get; set; }

    public bool Read { get; set; }
}
