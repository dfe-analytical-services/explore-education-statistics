using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.Cache;
using GovUk.Education.ExploreEducationStatistics.Content.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Cache;

public class RedirectsCacheService(
    IRedirectsService redirectsService,
    IPublicBlobCacheService publicBlobCacheService,
    ILogger<RedirectsCacheService> logger
) : IRedirectsCacheService
{
    public Task<Either<ActionResult, RedirectsViewModel>> List()
    {
        return publicBlobCacheService.GetOrCreateAsync(
            cacheKey: new RedirectsCacheKey(),
            createIfNotExistsFn: redirectsService.List,
            logger: logger
        );
    }

    public Task<Either<ActionResult, RedirectsViewModel>> UpdateRedirects()
    {
        return publicBlobCacheService.Update(
            cacheKey: new RedirectsCacheKey(),
            createIfNotExistsFn: redirectsService.List,
            logger: logger
        );
    }
}
