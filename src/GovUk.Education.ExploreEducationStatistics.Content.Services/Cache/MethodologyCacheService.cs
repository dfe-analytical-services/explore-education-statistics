using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Model;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.Cache;
using GovUk.Education.ExploreEducationStatistics.Content.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Cache;

public class MethodologyCacheService(
    IMethodologyService methodologyService,
    IPublicBlobCacheService publicBlobCacheService,
    ILogger<MethodologyCacheService> logger
) : IMethodologyCacheService
{
    public Task<Either<ActionResult, List<AllMethodologiesThemeViewModel>>> GetSummariesTree()
    {
        return publicBlobCacheService.GetOrCreateAsync(
            cacheKey: new AllMethodologiesCacheKey(),
            createIfNotExistsFn: methodologyService.GetSummariesTree,
            logger: logger
        );
    }

    public Task<Either<ActionResult, List<AllMethodologiesThemeViewModel>>> UpdateSummariesTree()
    {
        logger.LogInformation("Updating cached Methodology Tree");

        return publicBlobCacheService.Update(
            cacheKey: new AllMethodologiesCacheKey(),
            createFn: methodologyService.GetSummariesTree,
            logger: logger
        );
    }

    public Task<Either<ActionResult, List<MethodologyVersionSummaryViewModel>>> GetSummariesByPublication(
        Guid publicationId
    )
    {
        return GetSummariesTree()
            .OnSuccess(methodologiesByTheme =>
            {
                var matchingPublication = methodologiesByTheme
                    .SelectMany(theme => theme.Publications)
                    .SingleOrDefault(publication => publication.Id == publicationId);
                return matchingPublication?.Methodologies ?? new List<MethodologyVersionSummaryViewModel>();
            });
    }
}
