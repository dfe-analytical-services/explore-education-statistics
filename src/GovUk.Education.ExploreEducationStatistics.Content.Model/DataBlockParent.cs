#nullable enable
using System;
using System.Collections.Generic;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model;

public class DataBlockParent
{
    public Guid Id { get; set; }

    public Guid LatestVersionId { get; set; }

    public DataBlockVersion LatestVersion { get; set; } = null!;

    public Guid? LatestPublishedVersionId { get; set; }

    public DataBlockVersion? LatestPublishedVersion { get; set; }

    // TODO EES-4467 - is this confusing having this do differently to the other Clone methods?
    public DataBlockParent Clone()
    {
        return (MemberwiseClone() as DataBlockParent)!;
    }
}