#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Cache;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services;

public class ContentCacheService : IContentCacheService
{
    private readonly IBlobCacheService _blobCacheService;
    private readonly IMethodologyService _methodologyService;

    public ContentCacheService(
        IBlobCacheService blobCacheService, 
        IMethodologyService methodologyService)
    {
        _blobCacheService = blobCacheService;
        _methodologyService = methodologyService;
    }

    [BlobCache(typeof(AllMethodologiesCacheKey))]
    public Task<Either<ActionResult, List<AllMethodologiesThemeViewModel>>> GetMethodologyTree()
    {
        return _methodologyService.GenerateSummariesTree();
    }

    public async Task<Either<ActionResult, List<AllMethodologiesThemeViewModel>>> UpdateMethodologyTree()
    {
        await _blobCacheService.DeleteItem(new AllMethodologiesCacheKey());
        return await GetMethodologyTree();
    }

    public Task<Either<ActionResult, List<MethodologyVersionSummaryViewModel>>> GetMethodologiesByPublication(Guid publicationId)
    {
        return GetMethodologyTree()
            .OnSuccess(methodologiesByTheme => FindMatchingPublication(publicationId, methodologiesByTheme))
            .OnSuccess(matchingPublication => matchingPublication.Methodologies);
    }

    private static Either<ActionResult, AllMethodologiesPublicationViewModel> 
        FindMatchingPublication(Guid publicationId, List<AllMethodologiesThemeViewModel> methodologiesByTheme)
    {
        var matchingPublication = methodologiesByTheme
            .SelectMany(theme => theme.Topics)
            .SelectMany(topic => topic.Publications)
            .SingleOrDefault(publication => publication.Id == publicationId);

        if (matchingPublication == null)
        {
            return new NotFoundResult();
        }

        return matchingPublication;
    }
}
