#nullable enable
using System;
using GovUk.Education.ExploreEducationStatistics.Common.Model;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model;

public class ReleaseRedirect : ICreatedTimestamp<DateTime>
{
    public string Slug { get; init; } = null!;

    public Guid ReleaseId { get; init; }

    public Release Release { get; init; } = null!;

    public DateTime Created { get; set; }
}
