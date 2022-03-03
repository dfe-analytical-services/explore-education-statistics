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
            Key = GetKey(filter);
        }

        public static string GetKey(PublicationTreeFilter? filter = null)
        {
            return filter switch
            {
                PublicationTreeFilter.AnyData => "publication-tree-any-data.json",
                PublicationTreeFilter.LatestData => "publication-tree-latest-data.json",
                PublicationTreeFilter.LatestDataNotSuperseded => "publication-tree-latest-data-not-superseded.json",
                PublicationTreeFilter.NotSuperseded => "publication-tree-not-superseded.json",
                null => "publication-tree.json",
                _ => throw new ArgumentOutOfRangeException(nameof(filter), filter, null)
            };
        }
    }
}
