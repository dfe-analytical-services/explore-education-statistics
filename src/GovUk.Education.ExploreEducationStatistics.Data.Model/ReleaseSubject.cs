#nullable enable
using System;
using GovUk.Education.ExploreEducationStatistics.Common.Model;

namespace GovUk.Education.ExploreEducationStatistics.Data.Model;

public class ReleaseSubject : ICreatedUpdatedTimestamps<DateTime?, DateTime?>
{
    public Subject Subject { get; set; } = null!;

    public Guid SubjectId { get; set; }

    public ReleaseVersion ReleaseVersion { get; set; } = null!;

    public Guid ReleaseVersionId { get; set; }

    public DateTime? Created { get; set; }

    public DateTime? Updated { get; set; }
}
