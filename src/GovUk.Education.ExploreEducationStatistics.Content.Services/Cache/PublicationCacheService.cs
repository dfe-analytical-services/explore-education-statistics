using System.Text.RegularExpressions;
using GovUk.Education.ExploreEducationStatistics.Common;
using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.Cache;
using GovUk.Education.ExploreEducationStatistics.Content.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Cache;

public class PublicationCacheService(
    IPublicationService publicationService,
    IPublicBlobStorageService publicBlobStorageService,
    IPublicBlobCacheService publicBlobCacheService,
    ILogger<PublicationCacheService> logger
) : IPublicationCacheService
{
    public Task<Either<ActionResult, PublicationCacheViewModel>> GetPublication(string publicationSlug)
    {
        return publicBlobCacheService.GetOrCreateAsync(
            new PublicationCacheKey(PublicationSlug: publicationSlug),
            () => publicationService.Get(publicationSlug),
            logger: logger
        );
    }

    public Task<Either<ActionResult, PublicationCacheViewModel>> UpdatePublication(string publicationSlug)
    {
        return publicBlobCacheService.Update(
            new PublicationCacheKey(PublicationSlug: publicationSlug),
            () => publicationService.Get(publicationSlug),
            logger: logger
        );
    }

    public async Task<Either<ActionResult, Unit>> RemovePublication(string publicationSlug)
    {
        await publicBlobStorageService.DeleteBlobs(
            containerName: BlobContainers.PublicContent,
            options: new IBlobStorageService.DeleteBlobsOptions
            {
                IncludeRegex = new Regex($"^publications/{publicationSlug}/.+$"),
            }
        );

        return Unit.Instance;
    }
}
