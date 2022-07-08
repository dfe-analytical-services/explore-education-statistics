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

    public CacheableDataBlock(ReleaseContentBlock releaseContentBlock)
    {
        if (releaseContentBlock.ContentBlock is not DataBlock)
        {
            throw new ArgumentException(
                $"ContentBlock must be of type DataBlock. Found {releaseContentBlock.ContentBlock?.GetType().Name ?? "null"}.");
        }

        if (releaseContentBlock.Release == null)
        {
            throw new ArgumentException("Release must be hydrated");
        }

        if (releaseContentBlock.Release.Publication == null)
        {
            throw new ArgumentException("Publication must be hydrated");
        }

        DataBlockId = releaseContentBlock.ContentBlockId;
        Release = releaseContentBlock.Release;
    }

    public Guid ReleaseId => Release.Id;
    public string ReleaseSlug => Release.Slug;
    public string PublicationSlug => Release.Publication.Slug;
    public DateTime? LastModified => Release.Published;
}
