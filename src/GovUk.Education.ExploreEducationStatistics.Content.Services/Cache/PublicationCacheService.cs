#nullable enable
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Cache;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.Cache;
using GovUk.Education.ExploreEducationStatistics.Content.Services.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Cache;

public class PublicationCacheService : IPublicationCacheService
{
    private readonly IPublicationService _publicationService;

    public PublicationCacheService(IPublicationService publicationService)
    {
        _publicationService = publicationService;
    }

    [BlobCache(typeof(PublicationCacheKey), ServiceName = "public")]
    public Task<Either<ActionResult, PublicationViewModel>> GetPublication(string publicationSlug)
    {
        return _publicationService.Get(publicationSlug);
    }

    [BlobCache(typeof(PublicationCacheKey), forceUpdate: true, ServiceName = "public")]
    public Task<Either<ActionResult, PublicationViewModel>> UpdatePublication(string publicationSlug)
    {
        return _publicationService.Get(publicationSlug);
    }
}
