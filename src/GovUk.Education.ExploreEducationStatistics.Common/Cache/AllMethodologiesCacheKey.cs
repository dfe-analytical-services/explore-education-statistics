﻿#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Cache.Interfaces;

namespace GovUk.Education.ExploreEducationStatistics.Common.Cache
{
    public record AllMethodologiesCacheKey : IBlobCacheKey
    {
        public string Key => GetKey();

        public IBlobContainer Container => BlobContainers.PublicContent;

        public static string GetKey()
        {
            return "methodology-tree.json";
        }
    }
}
