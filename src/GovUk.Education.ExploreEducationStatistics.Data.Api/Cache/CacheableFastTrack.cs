#nullable enable
using System;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Release = GovUk.Education.ExploreEducationStatistics.Content.Model.Release;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Cache;

public record CacheableFastTrack
{
    // TODO DW - caches will need to be rebuilt
    public Guid FastTrackId { get; }
    private Release Release { get; }

    public CacheableFastTrack(Guid fastTrackId, Release release)
    {
        if (release.Publication == null)
        {
            throw new ArgumentException("Publication must be hydrated");
        }

        FastTrackId = fastTrackId;
        Release = release;
    }

    public CacheableFastTrack(FastTrackVersion fastTrackVersion)
    {
        if (fastTrackVersion.Release == null)
        {
            throw new ArgumentException("Release must be hydrated");
        }

        if (fastTrackVersion.Release.Publication == null)
        {
            throw new ArgumentException("Publication must be hydrated");
        }

        FastTrackId = fastTrackVersion.FastTrackId;
        Release = fastTrackVersion.Release;
    }

    public Guid ReleaseId => Release.Id;
    public string ReleaseSlug => Release.Slug;
    public string PublicationSlug => Release.Publication.Slug;
    public DateTime? LastModified => Release.Published;
}
