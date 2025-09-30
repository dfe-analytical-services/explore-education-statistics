using GovUk.Education.ExploreEducationStatistics.Common.ViewModels;

namespace GovUk.Education.ExploreEducationStatistics.Content.Services.Interfaces.Cache;

public interface IGlossaryCacheService
{
    Task<List<GlossaryCategoryViewModel>> GetGlossary();

    Task<List<GlossaryCategoryViewModel>> UpdateGlossary();
}
