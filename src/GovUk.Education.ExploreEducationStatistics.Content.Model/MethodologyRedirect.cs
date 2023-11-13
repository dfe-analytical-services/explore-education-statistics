#nullable enable
using System;
using GovUk.Education.ExploreEducationStatistics.Common.Model;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model;

public class MethodologyRedirect : ICreatedTimestamp<DateTime>
{
    public string Slug { get; init; } = null!;

    public Guid MethodologyVersionId { get; init; }

    public MethodologyVersion MethodologyVersion { get; init; } = null!;

    public DateTime Created { get; set; }
}
