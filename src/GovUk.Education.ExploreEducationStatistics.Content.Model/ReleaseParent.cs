#nullable enable
using System;
using System.Collections.Generic;
using GovUk.Education.ExploreEducationStatistics.Common.Model;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model;

public class ReleaseParent : ICreatedUpdatedTimestamps<DateTime, DateTime?>
{
    public Guid Id { get; set; }

    public List<Release> Releases { get; set; } = new();

    public DateTime Created { get; set; }

    public DateTime? Updated { get; set; }
}
