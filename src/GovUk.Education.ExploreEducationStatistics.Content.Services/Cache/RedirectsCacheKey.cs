using GovUk.Education.ExploreEducationStatistics.Common;
using GovUk.Education.ExploreEducationStatistics.Common.Cache.Interfaces;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Cache;

public record RedirectsCacheKey : IBlobCacheKey
{
    public string Key => "redirects.json";

    public IBlobContainer Container => BlobContainers.PublicContent;
}
