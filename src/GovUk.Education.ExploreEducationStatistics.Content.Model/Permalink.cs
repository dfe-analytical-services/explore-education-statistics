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
    /// Indicates whether the permalink originated from legacy data before the introduction of permalink snapshots.
    /// This flag is temporarily retained to facilitate fallback to legacy data for resolving migration related issues.
    /// - True: The permalink's origin is in the legacy data, but it has been migrated to snapshot format.
    /// - False: The permalink was created after the introduction of snapshots.
    /// </summary>
    public bool MigratedFromLegacy { get; init; }

    public DateTime Created { get; set; }
}
