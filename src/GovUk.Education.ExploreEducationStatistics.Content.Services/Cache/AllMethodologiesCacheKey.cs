#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common;
using GovUk.Education.ExploreEducationStatistics.Common.Cache.Interfaces;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Cache;

public record AllMethodologiesCacheKey : IBlobCacheKey
{
    public IBlobContainer Container => BlobContainers.PublicContent;

    public string Key => "methodology-tree.json";
}
