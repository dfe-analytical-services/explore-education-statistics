using GovUk.Education.ExploreEducationStatistics.Common.Extensions;
using GovUk.Education.ExploreEducationStatistics.Common.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.Cache;
using Microsoft.Extensions.Logging;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Cache;

public class GlossaryCacheService(
    IGlossaryService glossaryService,
    IPublicBlobCacheService publicBlobCacheService,
    ILogger<GlossaryCacheService> logger
) : IGlossaryCacheService
{
    public Task<List<GlossaryCategoryViewModel>> GetGlossary()
    {
        return publicBlobCacheService.GetOrCreateAsync(
            cacheKey: new GlossaryCacheKey(),
            createIfNotExistsFn: glossaryService.GetGlossary,
            logger: logger
        );
    }

    public Task<List<GlossaryCategoryViewModel>> UpdateGlossary()
    {
        return publicBlobCacheService.Update(
            cacheKey: new GlossaryCacheKey(),
            createFn: glossaryService.GetGlossary,
            logger: logger
        );
    }
}
