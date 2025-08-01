#nullable enable
using System;
using GovUk.Education.ExploreEducationStatistics.Common;
using GovUk.Education.ExploreEducationStatistics.Common.Cache.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.FileStoragePathUtils;

namespace GovUk.Education.ExploreEducationStatistics.Data.Api.Cache;

public record DataBlockTableResultCacheKey : IBlobCacheKey
{
    private string PublicationSlug { get; }
    private string ReleaseSlug { get; }
    private Guid DataBlockParentId { get; }

    // ReSharper disable once UnusedMember.Global
    public DataBlockTableResultCacheKey(DataBlockVersion dataBlockVersion) : this(
        publicationSlug: dataBlockVersion.ReleaseVersion.Release.Publication.Slug,
        releaseSlug: dataBlockVersion.ReleaseVersion.Release.Slug,
        dataBlockParentId: dataBlockVersion.DataBlockParentId)
    {
    }

    public DataBlockTableResultCacheKey(string publicationSlug, string releaseSlug, Guid dataBlockParentId)
    {
        PublicationSlug = publicationSlug;
        ReleaseSlug = releaseSlug;
        DataBlockParentId = dataBlockParentId;
    }

    public IBlobContainer Container => BlobContainers.PublicContent;

    public string Key => PublicContentDataBlockPath(
        PublicationSlug,
        ReleaseSlug,
        DataBlockParentId
    );
}
