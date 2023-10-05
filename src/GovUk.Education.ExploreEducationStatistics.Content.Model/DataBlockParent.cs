#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model;

public class DataBlockParent
{
    public Guid Id { get; init; }

    public Guid LatestVersionId { get; init; }

    public DataBlockVersion LatestVersion { get; init; } = null!;

    public Guid? LatestPublishedVersionId { get; set; }

    public DataBlockVersion? LatestPublishedVersion { get; set; }
}