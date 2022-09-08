#nullable enable
using System.Collections.Generic;
using System.Threading.Tasks;
using GovUk.Education.ExploreEducationStatistics.Common.Cache;
using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces;
using GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.Cache;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Cache;

public class GlossaryCacheService : IGlossaryCacheService
{
    private readonly IGlossaryService _glossaryService;

    public GlossaryCacheService(IGlossaryService glossaryService)
    {
        _glossaryService = glossaryService;
    }

    [BlobCache(typeof(GlossaryCacheKey), ServiceName = "public")]
    public Task<List<GlossaryCategoryViewModel>> GetGlossary()
    {
        return _glossaryService.GetAllGlossaryEntries();
    }

    [BlobCache(typeof(GlossaryCacheKey), forceUpdate: true, ServiceName = "public")]
    public Task<List<GlossaryCategoryViewModel>> UpdateGlossary()
    {
        return _glossaryService.GetAllGlossaryEntries();
    }
}
