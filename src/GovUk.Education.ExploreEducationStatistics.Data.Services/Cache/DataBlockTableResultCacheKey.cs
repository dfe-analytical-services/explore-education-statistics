#nullable enable
using System;
using GovUk.Education.ExploreEducationStatistics.Common.Cache.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Model;
using static GovUk.Education.ExploreEducationStatistics.Common.Services.FileStoragePathUtils;

namespace GovUk.Education.ExploreEducationStatistics.Data.Services.Cache
{
    public class DataBlockTableResultCacheKey : ICacheKey
    {
        private string PublicationSlug { get; }
        private string ReleaseSlug { get; }
        private Guid DataBlockId { get; }

        public DataBlockTableResultCacheKey(ReleaseContentBlock releaseContentBlock)
        {
            if (!(releaseContentBlock.ContentBlock is DataBlock))
            {
                throw new ArgumentException(
                    "Attempting to build key with incorrect type of content block");
            }

            var release = releaseContentBlock.Release;
            PublicationSlug = release.Publication.Slug;
            ReleaseSlug = release.Slug;
            DataBlockId = releaseContentBlock.ContentBlockId;
        }

        public string Key => PublicContentDataBlockPath(
            PublicationSlug,
            ReleaseSlug,
            DataBlockId
        );
    }
}
