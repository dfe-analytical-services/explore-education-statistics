#nullable enable
using GovUk.Education.ExploreEducationStatistics.Common.Cache;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.Cache;
using GovUk.Education.ExploreEducationStatistics.Content.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Cache;

public class RedirectsCacheService : IRedirectsCacheService
{
    private readonly IRedirectsService _redirectsService;

    public RedirectsCacheService(IRedirectsService redirectsService)
    {
        _redirectsService = redirectsService;
    }

    [BlobCache(typeof(RedirectsCacheKey), ServiceName = "public")]
    public async Task<Either<ActionResult,RedirectsViewModel>> List()
    {
        return await _redirectsService.List();
    }

    [BlobCache(typeof(RedirectsCacheKey), forceUpdate: true, ServiceName = "public")]
    public async Task<Either<ActionResult,RedirectsViewModel>> UpdateRedirects()
    {
        return await _redirectsService.List();
    }
}
