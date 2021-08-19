#nullable enable
using System;
using GovUk.Education.ExploreEducationStatistics.Common;
using GovUk.Education.ExploreEducationStatistics.Common.Cache.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Requests;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Cache
{
    public record PublicationTreeCacheKey : IBlobCacheKey
    {
        public string Key { get; }

        public IBlobContainer Container => BlobContainers.PublicContent;

        public PublicationTreeCacheKey(PublicationTreeFilter? filter = null)
        {
            Key = filter switch
            {
                PublicationTreeFilter.LatestData => "publication-tree-latest-data.json",
                null => "publication-tree.json",
                _ => throw new ArgumentOutOfRangeException(nameof(filter), filter, null)
            };
        }
    }
}