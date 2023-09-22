#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model;

public class DataBlockParent
{
    public Guid Id { get; init; }

    public List<DataBlockVersion> Versions { get; init; } = new();
    
    public DataBlockVersion GetLatestVersion => Versions
        .OrderByDescending(version => version.Version)
        .First();

    public DataBlockVersion? GetLatestDraftVersion => Versions
        .Where(version => version.Published == null)
        .MaxBy(version => version.Version);
    
    public DataBlockVersion? GetLatestPublishedVersion => Versions
        .Where(version => version.Published != null)
        .MaxBy(version => version.Version);
}