#nullable enable
using System;
using GovUk.Education.ExploreEducationStatistics.Common.Model;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model;

public class Permalink : ICreatedTimestamp<DateTime>
{
    public Guid Id { get; init; }

    public string PublicationTitle { get; init; } = string.Empty;

    public string DataSetTitle { get; init; } = string.Empty;

    public Guid? ReleaseId { get; init; }

    public Guid SubjectId { get; init; }

    /// <summary>
    /// True if this is a Permalink created before the migration to use Permalink snapshots
    /// in the work done by epic EES-4226. We plan to temporarily retain this flag while there might be a need to
    /// fall back to the legacy Permalink data to fix any issues that might arise from the migration.
    /// </summary>
    public bool Legacy { get; init; }

    public DateTime Created { get; set; }
}
