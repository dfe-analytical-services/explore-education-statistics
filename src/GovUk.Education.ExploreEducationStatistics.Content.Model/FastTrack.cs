#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;

namespace GovUk.Education.ExploreEducationStatistics.Content.Model;

public class FastTrack
{
    public Guid Id { get; init; }

    public List<FastTrackVersion> Versions { get; init; } = new();
    
    public FastTrackVersion GetLatestVersion => Versions
        .OrderByDescending(version => version.Version)
        .First();

    public FastTrackVersion? GetLatestDraftVersion => Versions
        .Where(version => version.Published == null)
        .MaxBy(version => version.Version);
    
    public FastTrackVersion? GetLatestPublishedVersion => Versions
        .Where(version => version.Published != null)
        .MaxBy(version => version.Version);
}