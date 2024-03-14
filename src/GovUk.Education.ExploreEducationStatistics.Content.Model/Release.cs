#nullable enable
using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Common.Model;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model;

public class Release : ICreatedUpdatedTimestamps<DateTime, DateTime?>
{
    public Guid Id { get; set; }

    public List<ReleaseVersion> Versions { get; set; } = new();

    public DateTime Created { get; set; }

    public DateTime? Updated { get; set; }
}
