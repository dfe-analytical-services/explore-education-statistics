#nullable enable
using System;
using GovUk.Education.ExploreEducationStatistics.Common.Model;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model;

public class PublicationRedirect : ICreatedTimestamp<DateTime>
{
    public string Slug { get; init; } = null!;

    public Guid PublicationId { get; init; }

    public Publication Publication { get; init; } = null!;

    public DateTime Created { get; set; }
}
