#nullable enable
using System;
using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model;

public class DataBlockParent
{
    public Guid Id { get; set; }

    public Guid? LatestVersionId { get; set; }

    public DataBlockVersion? LatestVersion { get; set; }

    public Guid? LatestPublishedVersionId { get; set; }

    public DataBlockVersion? LatestPublishedVersion { get; set; }
}