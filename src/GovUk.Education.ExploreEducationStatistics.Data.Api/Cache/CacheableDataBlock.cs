#nullable enable
using System;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using Release = GovUk.Education.ExploreEducationStatistics.Content.Model.Release;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Cache;

public record CacheableDataBlock
{
    public Guid DataBlockId { get; }
    private Release Release { get; }

    public CacheableDataBlock(Guid dataBlockId, Release release)
    {
        if (release.Publication == null)
        {
            throw new ArgumentException("Publication must be hydrated");
        }

        DataBlockId = dataBlockId;
        Release = release;
    }

    public CacheableDataBlock(DataBlock dataBlock)
    {
        if (dataBlock.Release == null)
        {
            throw new ArgumentException("Release must be hydrated");
        }

        if (dataBlock.Release.Publication == null)
        {
            throw new ArgumentException("Publication must be hydrated");
        }

        DataBlockId = dataBlock.Id;
        Release = dataBlock.Release;
    }

    public Guid ReleaseId => Release.Id;
    public string ReleaseSlug => Release.Slug;
    public string PublicationSlug => Release.Publication.Slug;
    public DateTime? LastModified => Release.Published;
}
